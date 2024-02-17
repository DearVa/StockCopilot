using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Xml.Serialization;
using StockCopilot.Abstractions.Enums;
using StockCopilot.Abstractions.Interfaces;
using StockCopilot.Abstractions.Models;
using StockCopilot.Core.Internals;

namespace StockCopilot.Core.Services;

/// <summary>
/// 读取本地离线数据源
/// </summary>
[SupportedOSPlatform("windows")]
public unsafe class EastMoneyOfflineKLinesDataSource : IKLinesDataSource
{
    [XmlRoot(ElementName = "KLineInfoItem")]
    public class KLineInfoItem
    {
        [XmlAttribute(AttributeName = "FilePath")]
        public required string FilePath { get; set; }

        [XmlAttribute(AttributeName = "FileSize")]
        public required string FileSize { get; set; }
    }

    [XmlRoot(ElementName = "KLineInfo")]
    public class KLineInfo
    {
        [XmlElement(ElementName = "KLineInfoItem")]
        public required List<KLineInfoItem> KLineInfoItems { get; set; }
    }

    [XmlRoot(ElementName = "Root")]
    public class Root
    {
        [XmlElement(ElementName = "KLineInfo")]
        public required KLineInfo KLineInfo { get; set; }
    }

    private Root? kLineFileInfo;
    private DateTime previousFileModifyTime;
    private readonly Dictionary<string, KLine[]> code2kLines = new();

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Root))]
    private void LoadData()
    {
        var xmlFilePath = Path.Combine(
            EasyMoneyHelper.InstallPath, "data", "ClosedDownload", "Data", "KLineFileInfo.xml");
        if (!File.Exists(xmlFilePath)) throw new FileNotFoundException("未下载日线数据");
        var fileInfo = new FileInfo(xmlFilePath);
        if (kLineFileInfo == null || fileInfo.LastWriteTimeUtc != previousFileModifyTime)
        {
            using var stream = File.OpenRead(xmlFilePath);
            kLineFileInfo = (Root?)new XmlSerializer(typeof(Root)).Deserialize(stream);
            previousFileModifyTime = fileInfo.LastWriteTimeUtc;
        }

        code2kLines.Clear();
        if (kLineFileInfo == null) throw new IOException("无法解析KLineFileInfo.xml");
        foreach (var kLineInfoItem in kLineFileInfo.KLineInfo.KLineInfoItems)
        {
            var filePath = Path.Combine(EasyMoneyHelper.InstallPath, kLineInfoItem.FilePath);
            if (!File.Exists(filePath)) throw new FileNotFoundException("未下载日线数据");
            using var fs = File.OpenRead(filePath);
            using var mmf = MemoryMappedFile.CreateFromFile(
                fs, null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, false);

            BinaryReader CreateBinaryReader(long offset, long size) =>
                new(mmf.CreateViewStream(offset, size, MemoryMappedFileAccess.Read), Encoding.ASCII);

            // 文件头，0x30字节
            int stockCount, storageDayCount, totalPreservedStockCount, bytesCountPerDay;
            using (var br = CreateBinaryReader(0, 0x30))
            {
                if (br.ReadInt32() != 0x00000001 || br.ReadInt32() != 0x00000003) throw new DatException(filePath, "文件头");
                stockCount = br.ReadInt32();
                if (stockCount <= 0) throw new DatException(filePath, "文件头");
                br.BaseStream.Seek(0x4, SeekOrigin.Current); // 不详
                storageDayCount = br.ReadInt32(); // 股票 日期保存天数，通常为 90 01 00 00 也就是 0x190, 400 天
                totalPreservedStockCount = br.ReadInt32(); // 保存股票代码预留总个数 一般为 20 4e 00 00 也就是 0x4e20, 20000 个
                br.BaseStream.Seek(0x6, SeekOrigin.Current); // 不详
                bytesCountPerDay = br.ReadInt16(); // 每日数据长度 2字节 一般为 0x28, 40 字节
            }

            // 股票索引部分，共stockCount * 0x204字节
            var indexItems = new DatIndexItem[stockCount];
            using (var br = CreateBinaryReader(0x30, stockCount * 0x204))
            {
                for (var i = 0; i < stockCount; i++)
                {
                    br.BaseStream.Seek(0x204 * i, SeekOrigin.Begin);
                    var code = GetCode(br.ReadChars(0x8)); // 股票代码 8字节 前8字节为股票代码
                    br.BaseStream.Seek(0x10, SeekOrigin.Current); // 全0 16字节
                    var dayCount = br.ReadInt32(); // 股票数据天数 4字节
                    br.BaseStream.Seek(0x4, SeekOrigin.Current); // 股票数字编号，从0开始 4字节 不是市场
                    var offsets = new List<int>(DivideRoundUp(dayCount, storageDayCount));
                    while (true)
                    {
                        var dataOffset = br.ReadInt32(); // 股票数据偏移量 4字节
                        if (dataOffset <= 0) break;
                        offsets.Add(dataOffset);
                    }
                    indexItems[i] = new DatIndexItem(code, dayCount, offsets);

                    static string GetCode(char[] chars)
                    {
                        for (var i = 0; i < chars.Length; i++)
                        {
                            if (chars[i] == '\0')
                            {
                                return new string(chars, 0, i);
                            }
                        }
                        return new string(chars);
                    }
                }
            }

            // 股票数据部分
            // 起始位置：0x30 + 0x4e20 * 0x204 = 0x9d78b0
            // 每天的数据长度为 bytesCountPerDay
            // 每个股票的数据长度为 bytesCountPerDay * storageDayCount
            var dataPosition = 0x30 + 0x204 * totalPreservedStockCount;
            var bytesCountPerStockChunk = bytesCountPerDay * storageDayCount;
            using (var accessor = mmf.CreateViewAccessor(dataPosition, fs.Length - dataPosition, MemoryMappedFileAccess.Read))
            {
                var ptr = (byte*)accessor.SafeMemoryMappedViewHandle.DangerousGetHandle() + accessor.PointerOffset;
                for (var i = 0; i < stockCount; i++)
                {
                    var index = indexItems[i];
                    if (index.DayCount <= 0)
                    {
                        code2kLines.Add(index.Code, Array.Empty<KLine>());
                        continue;
                    }

                    var kLineList = new List<KLine>(index.DayCount);
                    foreach (var offset in index.Offsets)
                    {
                        var pKLineData = (KLineData*)(ptr + (long)offset * bytesCountPerStockChunk);

                        for (var j = 0; kLineList.Count < index.DayCount && j < storageDayCount; j++, pKLineData++)
                        {
                            if (pKLineData->Date <= 0) break;
                            kLineList.Add(new KLine
                            {
                                DateTime = new DateTime(pKLineData->Date / 10000, pKLineData->Date / 100 % 100,
                                    pKLineData->Date % 100),
                                Opening = pKLineData->Opening,
                                Closing = pKLineData->Closing,
                                Highest = pKLineData->Highest,
                                Lowest = pKLineData->Lowest,
                                Volume = pKLineData->Volume / 100f,
                                PriceChangeAmount = kLineList.Count > 0 ? pKLineData->Closing - kLineList[^1].Closing : float.NaN,
                            });
                        }
                    }

                    code2kLines.Add(index.Code, kLineList.ToArray());
                }
            }
        }

        // 向上取整相除
        static int DivideRoundUp(int dividend, int divisor) => (dividend + divisor - 1) / divisor;
    }

    private readonly record struct DatIndexItem(string Code, int DayCount, List<int> Offsets);

    [StructLayout(LayoutKind.Sequential)]
    private struct KLineData
    {
        /// <summary>
        /// 日期 4字节 如20220104
        /// </summary>
        public int Date;

        public int Unknown1;
        public float Opening;
        public float Closing;
        public float Highest;
        public float Lowest;
        public uint Volume;
        public fixed byte Unknown2[12];
    }

    private class DatException(string filePath, string message) : IOException($"文件{filePath}解析错误：{message}");

    public ValueTask<IReadOnlyList<KLine>> GetKLinesAsync(
        string market, string code1, DateTime begin, DateTime end, TimeSpan interval,
        AdjustmentType adjustmentType = AdjustmentType.None)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(interval, TimeSpan.FromDays(1));

        LoadData();

        // var startDateTime = DateTime.Now - TimeSpan.FromDays(60);
        bool DateTimeComparer(KLine k1) => k1.DateTime is { Year: 2023, Month: 12 };

        static bool TrendComparer(KLine k1, KLine k2) =>
            k1.DateTime == k2.DateTime &&
            k1.PriceChangeAmount <= 0 && k2.PriceChangeAmount <= 0 ||
            k1.PriceChangeAmount >= 0 && k2.PriceChangeAmount >= 0;

        var szIndex = "000001";
        var szKLines = code2kLines[szIndex].Where(DateTimeComparer).ToList();
        var sameTrendCodes = new List<string>();
        foreach (var (code, kLines) in code2kLines.Select(kv => (kv.Key, kv.Value.Where(DateTimeComparer).ToList())))
        {
            if (code == szIndex) continue;
            var equalCount = szKLines.Count(szKLine => kLines.Any(kLine => TrendComparer(kLine, szKLine)));
            if (equalCount == szKLines.Count) sameTrendCodes.Add(code);
        }
        
        Console.WriteLine($"同走势股票共{sameTrendCodes.Count}只");
        Console.WriteLine($"同走势股票代码：{string.Join(", ", sameTrendCodes)}");

        if (!code2kLines.TryGetValue(code1, out var result))
        {
            return new ValueTask<IReadOnlyList<KLine>>(Array.Empty<KLine>());
        }

        return new ValueTask<IReadOnlyList<KLine>>(result
            .Where(k => k.DateTime >= begin && k.DateTime <= end)
            .ToList());
    }
}
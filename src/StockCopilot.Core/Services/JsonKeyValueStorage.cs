using System.Text.Json;
using System.Text.Json.Serialization;
using StockCopilot.Abstractions.Interfaces;

namespace StockCopilot.Core.Services;

public partial class JsonKeyValueStorage : IKeyValueStorage
{
    private const string FileName = "storage.json";

    private static FileStream? TryOpenFileStream(string filePath)
    {
        try
        {
            return File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }
        catch
        {
            return null;
        }
    }

    private static FileStream? TryOpenFileStream()
    {
        var fileStream = TryOpenFileStream(Path.Combine(Environment.CurrentDirectory, FileName));
        if (fileStream != null) return fileStream;
        return TryOpenFileStream(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            nameof(StockCopilot),
            FileName));
    }

    private static Dictionary<string, string> TryReadStorage(Stream fileStream)
    {
        try
        {
            var result = (Dictionary<string, string>?)JsonSerializer.Deserialize(
                fileStream,
                typeof(Dictionary<string, string>),
                StorageJsonSerializerContext.Default);
            return result ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    public string? Read(string key)
    {
        using var fileStream = TryOpenFileStream();
        if (fileStream == null) return null;
        var storage = TryReadStorage(fileStream);
        return storage.GetValueOrDefault(key);
    }

    public void Save(string key, string? value)
    {
        using var fileStream = TryOpenFileStream();
        if (fileStream == null) return;
        
        var storage = TryReadStorage(fileStream);
        if (value == null)
        {
            storage.Remove(key);
        }
        else
        {
            storage[key] = value;
        }

        fileStream.Seek(0, SeekOrigin.Begin);
        JsonSerializer.Serialize(fileStream, storage, typeof(Dictionary<string, string>), StorageJsonSerializerContext.Default);
        fileStream.SetLength(fileStream.Position);
    }
    
    [JsonSerializable(typeof(Dictionary<string, string>))]
    private partial class StorageJsonSerializerContext : JsonSerializerContext;
}
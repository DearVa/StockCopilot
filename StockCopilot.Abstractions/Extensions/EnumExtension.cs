using System.Runtime.CompilerServices;

namespace StockCopilot.Abstractions.Extensions; 

public static class EnumExtension {
	/// <summary>
	/// 比<see cref="Enum.HasFlag"/>更快的方法，去掉了装箱和类型检查，<b>只适用于int类型的enum</b>
	/// </summary>
	/// <typeparam name="TEnum"></typeparam>
	/// <param name="enum1"></param>
	/// <param name="enum2"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasFlagUnsafe<TEnum>(this TEnum enum1, TEnum enum2) where TEnum : Enum {
		return (Unsafe.As<TEnum, int>(ref enum1) & Unsafe.As<TEnum, int>(ref enum2)) != 0;
	}
	
	/// <summary>
	/// 判断是否是单个flag，即是否只有一位1
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static bool IsSingleFlagSet<TEnum>(this TEnum value) where TEnum : Enum 
	{
		var longValue = Convert.ToInt64(value);
		return (longValue != 0) && ((longValue & (longValue - 1)) == 0);
	}
}
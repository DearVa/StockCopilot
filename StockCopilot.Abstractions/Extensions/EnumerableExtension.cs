using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace StockCopilot.Abstractions.Extensions;

public static class EnumerableExtension {
	/// <summary>
	/// Python: enumerator
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="enumerable"></param>
	/// <param name="startIndex"></param>
	/// <param name="step"></param>
	/// <returns></returns>
	public static IEnumerable<(int index, T item)> WithIndex<T>(
		this IEnumerable<T> enumerable, 
		int startIndex = 0,
		int step = 1) {
		
		foreach (var item in enumerable) {
			yield return (startIndex, item);
			startIndex += step;
		}
	}

	public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable) {
		return new(enumerable);
	}
	
	public static IEnumerable<T> Reversed<T>(this IList<T> list) {
		for (var i = list.Count - 1; i >= 0; i--) {
			yield return list[i]; 
		}
	}

	public static int FindIndexOf<T>(this IList<T> list, Predicate<T> predicate) {
		for (var i = 0; i < list.Count; i++) {
			if (predicate(list[i])) {
				return i;
			}
		}

		return -1;
	}
	
	/// <summary>
	/// 完全枚举一个 <see cref="IEnumerable"/>，并丢弃所有元素
	/// </summary>
	/// <param name="enumerable"></param>
	[MethodImpl(MethodImplOptions.NoOptimization)]
	public static void Discard(this IEnumerable enumerable) {
		foreach (var _ in enumerable) { }
	}
	
	/// <summary>
	/// 完全枚举一个 <see cref="IEnumerable{T}"/>，并丢弃所有元素
	/// </summary>
	/// <param name="enumerable"></param>
	/// <typeparam name="T"></typeparam>
	[MethodImpl(MethodImplOptions.NoOptimization)]
	public static void Discard<T>(this IEnumerable<T> enumerable) {
		foreach (var _ in enumerable) { }
	}
	
	public static async ValueTask<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> enumerable) {
		var list = new List<T>();
		await foreach (var item in enumerable) {
			list.Add(item);
		}

		return list;
	}

	public static void Invoke<T>(this IEnumerable<T> source, Action<T> action)
	{
		foreach (var item in source)
		{
			action(item);
		}
	}
}
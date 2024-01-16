using System.Runtime.CompilerServices;

namespace StockCopilot.Abstractions.Extensions; 

public static class TaskExtension {
	public delegate void ExceptionHandler(Exception e);

	public static event ExceptionHandler? UnhandledExceptionThrown;

	public static void Detach(this Task task, ExceptionHandler? exceptionHandler = null) {
		if (exceptionHandler != null) {
			task.ContinueWith(t => {
				if (t.Exception != null) {
					exceptionHandler.Invoke(t.Exception);
				}
			});
		} else {
			task.ContinueWith(static t => {
				if (t.Exception != null) {
					if (UnhandledExceptionThrown != null) {
						UnhandledExceptionThrown.Invoke(t.Exception);
					} else {
						throw t.Exception;
					}
				}
			});
		}
	}

	public static Task AsTask(this CancellationToken cancellationToken)
	{
		var tcs = new TaskCompletionSource();
		cancellationToken.Register(() => tcs.SetCanceled(cancellationToken));
		return tcs.Task;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Detach<T>(this Task<T> task, ExceptionHandler? exceptionHandler = null) => Detach((Task)task, exceptionHandler);
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Detach(this ValueTask task, ExceptionHandler? exceptionHandler = null) => Detach(task.AsTask(), exceptionHandler);
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Detach<T>(this ValueTask<T> task, ExceptionHandler? exceptionHandler = null) => Detach((Task)task.AsTask(), exceptionHandler);
}
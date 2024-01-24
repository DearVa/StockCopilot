using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace StockCopilot.ViewModels;

public partial class BusyViewModelBase : ObservableObject
{
    [ObservableProperty] private bool isBusy;
    
    private Task? runningTask, enqueueTask;
    
    /// <summary>
    /// 如果当前不是Busy，就直接执行
    /// 如果当前正在Busy，就排队，但是队列中只有一个任务，再次排队会覆盖上一个任务
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    protected async Task ExecuteBusyAction(Func<Task> action)
    {
        Dispatcher.UIThread.VerifyAccess();

        IsBusy = true;

        try
        {
            if (runningTask == null)
            {
                runningTask = action();
            }
            else
            {
                enqueueTask = action();
            }

            try
            {
                await runningTask;
            }
            finally
            {
                runningTask = enqueueTask;
                enqueueTask = null;
            }

            if (runningTask != null)
            {
                await runningTask;
                runningTask = null;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
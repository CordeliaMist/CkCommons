using System.Threading;

namespace CkCommons.Classes;

/// <summary>
///   ThreadPool from ECommons, used as a placeholder while 
///   Emoji management gets more optimized and we no longer need it.
/// </summary>
public class SimpleThreadPool : IDisposable
{
    private ConcurrentQueue<(Action Action, Action<Exception?>? OnCompletion)> TaskQueue = new();

    private readonly int  MaxThreads = 8;
    private volatile uint ThreadNum;
    private volatile uint BusyThreads;
    private volatile bool Disposed;
    public SimpleThreadPool()
    {
        MaxThreads = Math.Clamp(Environment.ProcessorCount / 3, 1, 8);
    }

    public SimpleThreadPool(int maxThreads)
    {
        MaxThreads = maxThreads;
    }

    public bool IsWorking => BusyThreads != 0;
    public (uint RunningThreads, uint BusyThreads, int TasksQueued) State => (RunningThreads: ThreadNum, BusyThreads: BusyThreads, TasksQueued: TaskQueue.Count);

    public void Dispose()
    {
        Disposed = true;
    }

    public void Run(Action task, Action<Exception?>? onCompletion = null)
    {
        TaskQueue.Enqueue((task, onCompletion));
        long num = Math.Max(1L, Math.Min(MaxThreads, TaskQueue.Count + BusyThreads));
        if (ThreadNum < num)
        {
            Svc.Log.Verbose($"{ThreadNum} threads running, {BusyThreads} are busy, requested {num} threads, Creating new thread to deal with tasks...");
            ThreadNum++;
            new Thread(ThreadRun).Start();
        }
    }

    private void ThreadRun()
    {
        string text = $"{Random.Shared.Next():X8}";
        Svc.Log.Verbose($"Beginning Thread {text}!");
        int num = 0;
        while (!Disposed)
        {
            if (TaskQueue.TryDequeue(out (Action, Action<Exception?>?) result))
            {
                BusyThreads++;
                num = 0;
                Exception obj = null!;
                try
                {
                    result.Item1();
                }
                catch (Exception ex)
                {
                    if (result.Item2 == null)
                        Svc.Log.Error(ex, $"Exception in thread {text} with no error handler!");
                    else
                        obj = ex;
                }

                if (result.Item2 != null)
                {
                    try
                    {
                        result.Item2(obj);
                    }
                    catch (Exception e)
                    {
                        Svc.Log.Error(e, $"Exception in thread {text} while running error handler!");
                    }
                }

                BusyThreads--;
            }
            else
            {
                num++;
                Thread.Sleep(100);
                if (num > 100 || Disposed)
                {
                    ThreadNum--;
                    break;
                }
            }
        }

        Svc.Log.Verbose($"Thread {text} is ending!");
    }
}


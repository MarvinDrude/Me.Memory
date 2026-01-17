using System.Threading.Channels;
using Me.Memory.Extensions;

namespace Me.Memory.Threading;

public sealed class WorkPool : IAsyncDisposable
{
   private readonly Channel<WorkItemBase> _items;
   private readonly List<Task> _workers = [];
   private readonly CancellationTokenSource _cts = new();
   
   private bool _accepting = true;
   private bool _disposed = false; 

   public WorkPool(WorkPoolOptions? options)
   {
      options ??= new WorkPoolOptions();

      var channelOptions = new BoundedChannelOptions(options.Capacity)
      {
         FullMode = options.FullMode,
         SingleReader = options.SingleReader,
         SingleWriter = false,
         AllowSynchronousContinuations = false,
      };
      _items = Channel.CreateBounded<WorkItemBase>(channelOptions);

      for (var e = 0; e < options.MaxDegreeOfParallelism; e++)
      {
         var task = Task.Factory.StartNew(
            () => RunWorker(_cts.Token), _cts.Token, 
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
         
         _workers.Add(task);
      }
   }

   private async Task RunWorker(CancellationToken ct)
   {
      try
      {
         while (await _items.Reader.WaitToReadAsync(ct))
         {
            while (_items.Reader.TryRead(out var item))
            {
               await item.Execute(_cts.Token);
            }
         }
      }
      catch (OperationCanceledException)
      {
         // normal shutdown
      }
   }

   public Task<T> Enqueue<T>(
      Func<CancellationToken, Task<T>> func,
      CancellationToken ct = default)
   {
      if (!_accepting)
      {
         throw new InvalidOperationException("WorkPool is already completed or disposed.");
      }
      
      var tcs = new TaskCompletionSource<T>();
      var item = new WorkItem<T>(func, tcs, ct);

      var writeTask = _items.Writer.WriteAsync(item, ct);
      return !writeTask.IsCompletedSuccessfully 
         ? AwaitWriteThenResult(writeTask, tcs) 
         : tcs.Task;

      static async Task<T> AwaitWriteThenResult(
         ValueTask writeTask, TaskCompletionSource<T> tcs)
      {
         await writeTask;
         return await tcs.Task;
      }
   }

   public Task<T> Enqueue<T>(
      Func<T> func, CancellationToken ct = default)
   {
      return Enqueue(cancel =>
      {
         if (cancel.IsCancellationRequested) return Task.FromCanceled<T>(cancel);

         var result = func();
         return Task.FromResult(result);
         
      }, ct);
   }
   
   public async Task Complete()
   {
      _accepting = false;
      _items.Writer.TryComplete();
      
      await Task.WhenAll(_workers)
         .WithAggregateException();
      // maybe expose exceptions later
   }
   
   public async ValueTask DisposeAsync()
   {
      if (_disposed)
      {
         return;
      }

      _disposed = true;
      _accepting = false;
      
      await _cts.CancelAsync();
      _items.Writer.TryComplete();

      await Task.WhenAll(_workers)
         .WithAggregateException();
      // maybe expose exceptions later
      
      _cts.Dispose();
   }

   private abstract class WorkItemBase
   {
      public abstract Task Execute(CancellationToken poolToken);
   }
   
   private sealed class WorkItem<T> : WorkItemBase
   {
      private readonly Func<CancellationToken, Task<T>> _func;
      private readonly TaskCompletionSource<T> _tcs;
      private readonly CancellationToken _ct;

      public WorkItem(
         Func<CancellationToken, Task<T>> func,
         TaskCompletionSource<T> tcs,
         CancellationToken ct)
      {
         _func = func;
         _tcs = tcs;
         _ct = ct;
      }

      public override async Task Execute(CancellationToken poolToken)
      {
         if (_ct.IsCancellationRequested)
         {
            _tcs.TrySetCanceled(_ct);
            return;
         }

         try
         {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(poolToken, _ct);
            var linkedToken = linkedCts.Token;
            
            var result = await _func(linkedToken);
            _tcs.TrySetResult(result);
         }
         catch (OperationCanceledException cancelled)
         {
            _tcs.TrySetCanceled(cancelled.CancellationToken.IsCancellationRequested 
               ? cancelled.CancellationToken : _ct);
         }
         catch (Exception ex)
         {
            _tcs.TrySetException(ex);
         }
      }
   }
}
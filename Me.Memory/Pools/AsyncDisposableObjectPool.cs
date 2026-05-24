using System;
using System.Threading.Tasks;

namespace Me.Memory.Pools;

public sealed class AsyncDisposableObjectPool<T>(ObjectPoolOptions<T> options)
   : ObjectPool<T>(options), IAsyncDisposable
   where T : class, IAsyncDisposable
{
   private volatile bool _isDisposed;

   public override T Get(Func<T>? factoryFunc)
   {
      if (_isDisposed)
      {
         ThrowObjectDisposedException();
      }
      
      return base.Get(factoryFunc);
   }

   public override bool Return(T item)
   {
      // requires async dispose method
      throw new InvalidOperationException("Call ReturnAsync instead of Return");
   }

   public ValueTask<bool> ReturnAsync(T item)
   {
      return !Return(item) || _isDisposed
         ? Awaited(item) 
         : new ValueTask<bool>(true);

      static async ValueTask<bool> Awaited(T inner)
      {
         await inner.DisposeAsync();
         return false;
      }
   }

   public async ValueTask DisposeAsync()
   {
      if (_isDisposed) return;
      _isDisposed = true;

      await DisposeEntryAsync(_head);
      _head = null;
      
      while (_queue.TryDequeue(out var item))
         await DisposeEntryAsync(item);
   }

   private static async ValueTask DisposeEntryAsync(T? item)
   {
      if (item is not null)
      {
         await item.DisposeAsync();
      }
   }

   private static void ThrowObjectDisposedException()
   {
      throw new InvalidOperationException("Object has been disposed");
   }
}

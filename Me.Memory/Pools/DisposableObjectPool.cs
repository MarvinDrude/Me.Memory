namespace Me.Memory.Pools;

public sealed class DisposableObjectPool<T>(ObjectPoolOptions<T> options)
   : ObjectPool<T>(options), IDisposable
   where T : class, IDisposable
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
      if (!_isDisposed && base.Return(item)) 
         return true;
      
      DisposeEntry(item);
      return false;

   }
   
   public void Dispose()
   {
      if (_isDisposed) return;
      _isDisposed = true;

      DisposeEntry(_head);
      _head = null;

      while (_queue.TryDequeue(out var item))
      {
         DisposeEntry(item);
      }
   }
   
   private static void DisposeEntry(T? item)
   {
      item?.Dispose();
   }
   
   private static void ThrowObjectDisposedException()
   {
      throw new InvalidOperationException("Object has been disposed");
   }
}
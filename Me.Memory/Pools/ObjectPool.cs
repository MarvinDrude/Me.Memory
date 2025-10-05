using System.Collections.Concurrent;

namespace Me.Memory.Pools;

public sealed class ObjectPool<T>
   where T : class
{
   private readonly Func<T> _factoryFunc;
   private readonly Func<T, bool> _returnFunc;

   private readonly int _maxSize;
   private readonly int _initialSize;

   private int _currentSize;
   private readonly ConcurrentQueue<T> _queue = [];

   private T? _head;

   public ObjectPool(ObjectPoolOptions<T> options)
   {
      _factoryFunc = options.FactoryFunc;
      _returnFunc = options.ReturnFunc;
      
      _maxSize = options.MaxSize;
      _initialSize = options.InitialSize;

      for (var i = 0; i < options.InitialSize; i++)
      {
         _queue.Enqueue(_factoryFunc());
      }

      _queue.TryDequeue(out _head);
      _currentSize = _queue.Count;
   }
   
   public T Get(Func<T>? factoryFunc)
   {
      var candidate = _head;
      
      // only dequeue if we can't get the head item
      if (candidate == null || Interlocked.CompareExchange(ref _head, null, candidate) != candidate)
      {
         if (!_queue.TryDequeue(out candidate))
         {
            // none left, create new one
            return factoryFunc?.Invoke() ?? _factoryFunc();
         }

         // candidate got from dequeue
         Interlocked.Decrement(ref _currentSize);
      }
      
      return candidate;
   }

   public bool Return(T item)
   {
      if (!_returnFunc(item))
      {
         return false;
      }

      // Only enqueue if we can't atomically set it as new head item
      if (_head != null || Interlocked.CompareExchange(ref _head, item, null) != null)
      {
         if (Interlocked.Increment(ref _currentSize) <= _maxSize)
         {
            _queue.Enqueue(item);
            return true;
         }
         
         // max reached, revert back
         Interlocked.Decrement(ref _currentSize);
         return false;
      }

      return true;
   }
}
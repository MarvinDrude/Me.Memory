using Me.Memory.Pools;

namespace Me.Memory.Tests.Pools;

public class AsyncDisposableObjectPoolTests
{
   private class DisposableItem : IAsyncDisposable
   {
      public int DisposeCount { get; private set; }

      public ValueTask DisposeAsync()
      {
         DisposeCount++;
         return ValueTask.CompletedTask;
      }
   }

   [Test]
   public async Task DisposeAsyncDisposesHeadAndQueueItems()
   {
      // Arrange
      var items = new List<DisposableItem>();
      var options = new ObjectPoolOptions<DisposableItem>
      {
         FactoryFunc = () =>
         {
            var item = new DisposableItem();
            items.Add(item);
            return item;
         },
         InitialSize = 3,
         MaxSize = 10
      };

      var pool = new AsyncDisposableObjectPool<DisposableItem>(options);

      // We expect 3 items to be instantiated initially.
      // One goes to _head, two go to _queue.
      await Assert.That(items.Count).IsEqualTo(3);

      // Act
      await pool.DisposeAsync();

      // Assert
      foreach (var item in items)
      {
         await Assert.That(item.DisposeCount).IsEqualTo(1);
      }
   }

   [Test]
   public async Task DisposeAsyncIsIdempotent()
   {
      // Arrange
      var items = new List<DisposableItem>();
      var options = new ObjectPoolOptions<DisposableItem>
      {
         FactoryFunc = () =>
         {
            var item = new DisposableItem();
            items.Add(item);
            return item;
         },
         InitialSize = 3,
         MaxSize = 10
      };

      var pool = new AsyncDisposableObjectPool<DisposableItem>(options);

      // Act
      await pool.DisposeAsync();
      await pool.DisposeAsync(); // second call should be a no-op

      // Assert
      foreach (var item in items)
      {
         await Assert.That(item.DisposeCount).IsEqualTo(1);
      }
   }

   [Test]
   public async Task DisposeAsyncHandlesEmptyPool()
   {
      // Arrange
      var options = new ObjectPoolOptions<DisposableItem>
      {
         FactoryFunc = () => new DisposableItem(),
         InitialSize = 0,
         MaxSize = 10
      };

      var pool = new AsyncDisposableObjectPool<DisposableItem>(options);

      // Act & Assert (Should not throw)
      var act = async () => await pool.DisposeAsync();
      await Assert.That(act).ThrowsNothing();
   }
}

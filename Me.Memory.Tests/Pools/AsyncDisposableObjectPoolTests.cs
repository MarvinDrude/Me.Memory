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

   [Test]
   public async Task ReturnAsyncSuccessfullyReturnsItemToPool()
   {
      // Arrange
      var options = new ObjectPoolOptions<DisposableItem>
      {
         FactoryFunc = () => new DisposableItem(),
         InitialSize = 0,
         MaxSize = 5
      };

      var pool = new AsyncDisposableObjectPool<DisposableItem>(options);
      var item = pool.Get(null);

      // Act
      var returned = await pool.ReturnAsync(item);

      // Assert
      await Assert.That(returned).IsTrue();
      await Assert.That(item.DisposeCount).IsEqualTo(0);

      // Verify we get the same item back
      var item2 = pool.Get(null);
      await Assert.That(item2).IsSameReferenceAs(item);
   }

   [Test]
   public async Task ReturnAsyncDisposesItemIfDisposed()
   {
      // Arrange
      var options = new ObjectPoolOptions<DisposableItem>
      {
         FactoryFunc = () => new DisposableItem(),
         InitialSize = 0,
         MaxSize = 5
      };

      var pool = new AsyncDisposableObjectPool<DisposableItem>(options);
      var item = pool.Get(null);

      await pool.DisposeAsync();

      // Act
      var returned = await pool.ReturnAsync(item);

      // Assert
      await Assert.That(returned).IsFalse();
      await Assert.That(item.DisposeCount).IsEqualTo(1);
   }

   [Test]
   public async Task ReturnAsyncDisposesItemIfFull()
   {
      // Arrange
      var options = new ObjectPoolOptions<DisposableItem>
      {
         FactoryFunc = () => new DisposableItem(),
         InitialSize = 0,
         MaxSize = 1 // Head + Queue capacity (1) = Total 2 effective capacity
      };

      var pool = new AsyncDisposableObjectPool<DisposableItem>(options);
      var item1 = pool.Get(null);
      var item2 = pool.Get(null);
      var item3 = pool.Get(null);

      // Return first two successfully (one to Head, one to Queue)
      await pool.ReturnAsync(item1);
      await pool.ReturnAsync(item2);

      // Act: returning third item should exceed MaxSize and thus dispose it immediately
      var returned = await pool.ReturnAsync(item3);

      // Assert
      await Assert.That(returned).IsFalse();
      await Assert.That(item3.DisposeCount).IsEqualTo(1);
   }

   [Test]
   public async Task ReturnThrowsInvalidOperationException()
   {
      // Arrange
      var options = new ObjectPoolOptions<DisposableItem>
      {
         FactoryFunc = () => new DisposableItem(),
         InitialSize = 0,
         MaxSize = 5
      };

      var pool = new AsyncDisposableObjectPool<DisposableItem>(options);
      var item = pool.Get(null);

      // Act & Assert
      var act = () => pool.Return(item);
      await Assert.That(act).Throws<InvalidOperationException>().WithMessage("Call ReturnAsync instead of Return");
   }

   [Test]
   public async Task GetThrowsInvalidOperationExceptionIfDisposed()
   {
      // Arrange
      var options = new ObjectPoolOptions<DisposableItem>
      {
         FactoryFunc = () => new DisposableItem(),
         InitialSize = 0,
         MaxSize = 5
      };

      var pool = new AsyncDisposableObjectPool<DisposableItem>(options);
      await pool.DisposeAsync();

      // Act & Assert
      var act = () => pool.Get(null);
      await Assert.That(act).Throws<InvalidOperationException>().WithMessage("Object has been disposed");
   }
}

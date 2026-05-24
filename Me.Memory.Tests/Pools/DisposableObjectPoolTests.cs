using Me.Memory.Pools;

namespace Me.Memory.Tests.Pools;

public class DisposableObjectPoolTests
{
   private class DisposableItem : IDisposable
   {
      public int DisposeCount { get; private set; }

      public void Dispose()
      {
         DisposeCount++;
      }
   }

   [Test]
   public async Task RetrievalAndReturnWorksCorrectly()
   {
      // Arrange
      var options = new ObjectPoolOptions<DisposableItem>
      {
         FactoryFunc = () => new DisposableItem(),
         InitialSize = 0,
         MaxSize = 5
      };

      var pool = new DisposableObjectPool<DisposableItem>(options);
      var item = pool.Get(null);

      // Act
      var returned = pool.Return(item);

      // Assert
      await Assert.That(returned).IsTrue();
      await Assert.That(item.DisposeCount).IsEqualTo(0);

      // Verify we get the same item back
      var item2 = pool.Get(null);
      await Assert.That(item2).IsSameReferenceAs(item);
   }

   [Test]
   public async Task DisposeDisposesAllItems()
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

      var pool = new DisposableObjectPool<DisposableItem>(options);

      // We expect 3 items to be instantiated initially.
      // One goes to _head, two go to _queue.
      await Assert.That(items.Count).IsEqualTo(3);

      // Act
      pool.Dispose();

      // Assert
      foreach (var item in items)
      {
         await Assert.That(item.DisposeCount).IsEqualTo(1);
      }
   }

   [Test]
   public async Task DisposeIsIdempotent()
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

      var pool = new DisposableObjectPool<DisposableItem>(options);

      // Act
      pool.Dispose();
      pool.Dispose(); // second call should be a no-op

      // Assert
      foreach (var item in items)
      {
         await Assert.That(item.DisposeCount).IsEqualTo(1);
      }
   }

   [Test]
   public async Task DisposeHandlesEmptyPool()
   {
      // Arrange
      var options = new ObjectPoolOptions<DisposableItem>
      {
         FactoryFunc = () => new DisposableItem(),
         InitialSize = 0,
         MaxSize = 10
      };

      var pool = new DisposableObjectPool<DisposableItem>(options);

      // Act & Assert (Should not throw)
      var act = () => pool.Dispose();
      await Assert.That(act).ThrowsNothing();
   }

   [Test]
   public async Task ReturnDisposesItemIfDisposed()
   {
      // Arrange
      var options = new ObjectPoolOptions<DisposableItem>
      {
         FactoryFunc = () => new DisposableItem(),
         InitialSize = 0,
         MaxSize = 5
      };

      var pool = new DisposableObjectPool<DisposableItem>(options);
      var item = pool.Get(null);

      pool.Dispose();

      // Act
      var returned = pool.Return(item);

      // Assert
      await Assert.That(returned).IsFalse();
      await Assert.That(item.DisposeCount).IsEqualTo(1);
   }

   [Test]
   public async Task ReturnDisposesItemIfFull()
   {
      // Arrange
      var options = new ObjectPoolOptions<DisposableItem>
      {
         FactoryFunc = () => new DisposableItem(),
         InitialSize = 0,
         MaxSize = 1 // Head + Queue capacity (1) = Total 2 effective capacity
      };

      var pool = new DisposableObjectPool<DisposableItem>(options);
      var item1 = pool.Get(null);
      var item2 = pool.Get(null);
      var item3 = pool.Get(null);

      // Return first two successfully (one to Head, one to Queue)
      pool.Return(item1);
      pool.Return(item2);

      // Act: returning third item should exceed MaxSize and thus dispose it immediately
      var returned = pool.Return(item3);

      // Assert
      await Assert.That(returned).IsFalse();
      await Assert.That(item3.DisposeCount).IsEqualTo(1);
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

      var pool = new DisposableObjectPool<DisposableItem>(options);
      pool.Dispose();

      // Act & Assert
      var act = () => pool.Get(null);
      await Assert.That(act).Throws<InvalidOperationException>().WithMessage("Object has been disposed");
   }
}

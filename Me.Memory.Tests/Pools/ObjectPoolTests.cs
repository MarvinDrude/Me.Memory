using System.Collections.Concurrent;
using Me.Memory.Pools;

namespace Me.Memory.Tests.Pools;

public class ObjectPoolTests
{
   public class PooledItem { public int Id { get; set; } }

   [Test]
   public async Task ConstructorInitializesWithPreFilledItems()
   {
      // Setup: InitialSize 3.
      // 1 item goes to _head, 2 items go to _queue.
      var options = new ObjectPoolOptions<PooledItem>
      {
         FactoryFunc = () => new PooledItem(),
         InitialSize = 3,
         MaxSize = 10
      };

      var pool = new ObjectPool<PooledItem>(options);

      // Act: Get 3 items. None should trigger the factory (we check distinctness later)
      var item1 = pool.Get(null);
      var item2 = pool.Get(null);
      var item3 = pool.Get(null);

      // Assert
      await Assert.That(item1).IsNotNull();
      await Assert.That(item2).IsNotNull();
      await Assert.That(item3).IsNotNull();
      
      // Ensure they are distinct instances
      await Assert.That(item1).IsNotEqualTo(item2);
      await Assert.That(item2).IsNotEqualTo(item3);
   }

   [Test]
   public async Task GetUsesHeadOptimisation()
   {
      var options = new ObjectPoolOptions<PooledItem>
      {
         FactoryFunc = () => new PooledItem(),
         InitialSize = 0,
         MaxSize = 5
      };
      
      var pool = new ObjectPool<PooledItem>(options);
      var item = new PooledItem { Id = 99 };

      // Return item (fills Head first)
      pool.Return(item);

      // Act
      var retrieved = pool.Get(null);

      // Assert: Should get the exact same instance back from Head
      await Assert.That(retrieved).IsSameReferenceAs(item);
   }

   [Test]
   public async Task GetCreatesNewIfPoolEmpty()
   {
      var createdCount = 0;
      var options = new ObjectPoolOptions<PooledItem>
      {
         FactoryFunc = () => { createdCount++; return new PooledItem(); },
         InitialSize = 0
      };

      var pool = new ObjectPool<PooledItem>(options);

      // Act
      var item = pool.Get(null);

      // Assert
      await Assert.That(item).IsNotNull();
      await Assert.That(createdCount).IsEqualTo(1);
   }

   [Test]
   public async Task GetUsesProvidedFactoryOverride()
   {
      var options = new ObjectPoolOptions<PooledItem>
      {
         FactoryFunc = () => new PooledItem { Id = 1 }, // Default factory
         InitialSize = 0
      };

      var pool = new ObjectPool<PooledItem>(options);

      // Act: Pass a specific factory
      var item = pool.Get(() => new PooledItem { Id = 999 });

      // Assert
      await Assert.That(item.Id).IsEqualTo(999);
   }

   [Test]
   public async Task ReturnResetsToQueueWhenHeadFull()
   {
      var options = new ObjectPoolOptions<PooledItem>
      {
         FactoryFunc = () => new PooledItem(),
         InitialSize = 0,
         MaxSize = 5
      };

      var pool = new ObjectPool<PooledItem>(options);
      var item1 = new PooledItem { Id = 1 };
      var item2 = new PooledItem { Id = 2 };

      // Act
      // 1. First return fills _head
      var r1 = pool.Return(item1);
      // 2. Second return fills _queue
      var r2 = pool.Return(item2);

      // Assert
      await Assert.That(r1).IsTrue();
      await Assert.That(r2).IsTrue();

      // Verification: Get should retrieve _head (item1) first, then _queue (item2)
      // Note: Implementation detail -> Get checks Head first.
      var retrieved1 = pool.Get(null);
      var retrieved2 = pool.Get(null);

      await Assert.That(retrieved1).IsSameReferenceAs(item1); // From Head
      await Assert.That(retrieved2).IsSameReferenceAs(item2); // From Queue
   }

   [Test]
   public async Task ReturnRejectsInvalidItems()
   {
      var options = new ObjectPoolOptions<PooledItem>
      {
         FactoryFunc = () => new PooledItem(),
         // Logic: Only allow items with Id > 0
         ReturnFunc = (i) => i.Id > 0, 
         InitialSize = 0
      };

      var pool = new ObjectPool<PooledItem>(options);
      var valid = new PooledItem { Id = 10 };
      var invalid = new PooledItem { Id = 0 };

      // Act
      var resultValid = pool.Return(valid);
      var resultInvalid = pool.Return(invalid);

      // Assert
      await Assert.That(resultValid).IsTrue();
      await Assert.That(resultInvalid).IsFalse();
      
      // Ensure only valid is in pool
      var got = pool.Get(null);
      await Assert.That(got).IsSameReferenceAs(valid);
   }

   [Test]
   public async Task ReturnRespectsMaxSize()
   {
      // Setup: MaxSize = 1.
      // This means we can hold: 1 in _head + 1 in _queue = Total 2 capacity effectively?
      // Looking at code:
      // Return checks: if (Increment(currentSize) <= MaxSize).
      // So _queue holds MaxSize items. Plus _head holds 1.
      // Total capacity = MaxSize + 1.
      
      var options = new ObjectPoolOptions<PooledItem>
      {
         FactoryFunc = () => new PooledItem(),
         InitialSize = 0,
         MaxSize = 1 // Queue capacity
      };

      var pool = new ObjectPool<PooledItem>(options);
      var i1 = new PooledItem();
      var i2 = new PooledItem();
      var i3 = new PooledItem();

      // Act
      var r1 = pool.Return(i1); // Goes to Head
      var r2 = pool.Return(i2); // Goes to Queue (currentSize becomes 1)
      var r3 = pool.Return(i3); // currentSize would be 2 > MaxSize(1) -> Fail

      // Assert
      await Assert.That(r1).IsTrue();
      await Assert.That(r2).IsTrue();
      await Assert.That(r3).IsFalse(); // Dropped
   }

   [Test]
   public async Task ConcurrencySmokeTest()
   {
      // A simple smoke test to ensure no exceptions under load
      var options = new ObjectPoolOptions<PooledItem>
      {
         FactoryFunc = () => new PooledItem(),
         InitialSize = 10,
         MaxSize = 100
      };
      
      var pool = new ObjectPool<PooledItem>(options);
      const int parallelCount = 1000;
      var exceptions = new ConcurrentBag<Exception>();

      await Task.Run(() =>
      {
         Parallel.For(0, parallelCount, (i) =>
         {
            try
            {
               var item = pool.Get(null);
               item.Id = i; // Simulate work
               pool.Return(item);
            }
            catch (Exception ex)
            {
               exceptions.Add(ex);
            }
         });
      });

      await Assert.That(exceptions.IsEmpty).IsTrue();
   }
}
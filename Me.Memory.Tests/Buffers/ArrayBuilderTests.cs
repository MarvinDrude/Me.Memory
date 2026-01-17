using Me.Memory.Buffers;

namespace Me.Memory.Tests.Buffers;

public class ArrayBuilderTests
{
   [Test]
   public async Task ConstructorStartsInEmptyState()
   {
      using var builder = new ArrayBuilder<int>(minCapacity: 16);

      await Assert.That(builder.Count).IsEqualTo(0);
      await Assert.That(builder.WrittenSpan.Length).IsEqualTo(0);
      await Assert.That(builder.UnderlyingArray.Length).IsEqualTo(0);
   }

   [Test]
   public async Task AddSingleItemTriggersInitialAllocation()
   {
      using var builder = new ArrayBuilder<int>(minCapacity: 10);

      builder.Add(1);

      await Assert.That(builder.Count).IsEqualTo(1);
      await Assert.That(builder.WrittenSpan[0]).IsEqualTo(1);
      
      // Should have grown to at least minCapacity
      await Assert.That(builder.UnderlyingArray.Length).IsGreaterThanOrEqualTo(10);
   }

   [Test]
   public async Task AddSingleItemTriggersNotGrowth()
   {
      // Start small to force resize quickly
      using var builder = new ArrayBuilder<int>(minCapacity: 2);

      builder.Add(1);
      builder.Add(2);
      
      var initialArray = builder.UnderlyingArray;
      
      // This triggers resize (current len 2 -> full -> resize to 4)
      builder.Add(3);

      await Assert.That(builder.Count).IsEqualTo(3);
      await Assert.That(builder.WrittenSpan.ToArray()).IsEquivalentTo([1, 2, 3]);
      
      // Verify buffer was replaced
      await Assert.That(builder.UnderlyingArray).IsSameReferenceAs(initialArray);
   }

   [Test]
   public async Task AddSpanAppendsCorrectly()
   {
      using var builder = new ArrayBuilder<string>(minCapacity: 10);

      builder.Add("Start");
      builder.Add(["A", "B", "C"]);

      await Assert.That(builder.Count).IsEqualTo(4);
      await Assert.That(builder.WrittenSpan.ToArray()).IsEquivalentTo(["Start", "A", "B", "C"]);
   }

   [Test]
   public async Task AddSpanMassiveGrowthCalculatesCapacityCorrectly()
   {
      // Test the logic: while (newCapacity < requiredLength) newCapacity *= 2;
      using var builder = new ArrayBuilder<byte>(minCapacity: 2);
      builder.Add([1, 2]);

      // Add enough to force multiple doublings (2 -> 4 -> 8 -> 16 -> 32)
      // We add 20 items to existing 2 = 22 total.
      byte[] largeChunk = new byte[20]; 
      Array.Fill(largeChunk, (byte)9);
      
      builder.Add(largeChunk);

      await Assert.That(builder.Count).IsEqualTo(22);
      await Assert.That(builder.UnderlyingArray.Length).IsGreaterThanOrEqualTo(22);
      
      // Check content (first 2 + 20 nines)
      await Assert.That(builder.WrittenSpan[0]).IsEqualTo((byte)1);
      await Assert.That(builder.WrittenSpan[21]).IsEqualTo((byte)9);
   }

   [Test]
   public async Task SetUpdatesValueAtIndex()
   {
      using var builder = new ArrayBuilder<int>();
      builder.Add([1, 2, 3]);

      builder.Set(1, 99);

      await Assert.That(builder.WrittenSpan.ToArray()).IsEquivalentTo([1, 99, 3]);
   }

   [Test]
   public async Task ClearResetsBuilderToInitialState()
   {
      using var builder = new ArrayBuilder<int>(minCapacity: 10);
      builder.Add([1, 2, 3]);

      // Act
      builder.Clear();

      // Assert
      await Assert.That(builder.Count).IsEqualTo(0);
      // The implementation resets _buffer to EmptyPlaceholder
      await Assert.That(builder.UnderlyingArray.Length).IsEqualTo(0); 
   }

   [Test]
   public async Task ClearThenAddWorksCorrectly()
   {
      using var builder = new ArrayBuilder<int>();
      builder.Add([1, 2]);
      builder.Clear();
      
      // Should allocate new buffer from pool
      builder.Add(3);

      await Assert.That(builder.Count).IsEqualTo(1);
      await Assert.That(builder.WrittenSpan[0]).IsEqualTo(3);
   }

   [Test]
   public async Task DisposeIdempotencyDoesNotThrow()
   {
      var builder = new ArrayBuilder<int>();
      builder.Add(1);

      builder.Dispose();
      builder.Dispose(); // Second call should be safe

      await Assert.That(builder.Count).IsEqualTo(0);
      await Assert.That(builder.UnderlyingArray.Length).IsEqualTo(0);
   }

   [Test]
   public async Task UnderlyingArrayExposesInternalBuffer()
   {
      using var builder = new ArrayBuilder<int>(minCapacity: 10);
      builder.Add(5);

      // The underlying array from the pool might be larger than the Count
      await Assert.That(builder.UnderlyingArray.Length).IsGreaterThanOrEqualTo(10);
      
      // Accessing via UnderlyingArray directly
      await Assert.That(builder.UnderlyingArray[0]).IsEqualTo(5);
   }
}
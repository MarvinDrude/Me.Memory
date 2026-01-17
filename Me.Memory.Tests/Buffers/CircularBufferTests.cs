using Me.Memory.Buffers;

namespace Me.Memory.Tests.Buffers;

public class CircularBufferTests
{
   [Test]
   public async Task ConstructorInitializesCorrectly()
   {
      int count;
      int capacity;

      {
         using var buffer = new CircularBuffer<byte>(10);
         count = buffer.Count;
         capacity = buffer.Capacity;
      }

      await Assert.That(count).IsEqualTo(0);
      await Assert.That(capacity).IsEqualTo(10);
   }

   [Test]
   public async Task AddIncrementsCountAndStoresItem()
   {
      byte resultItem;
      int count;

      {
         using var buffer = new CircularBuffer<byte>(5);
         buffer.Add(0xAA);
         
         count = buffer.Count;
         resultItem = buffer[0];
      }

      await Assert.That(count).IsEqualTo(1);
      await Assert.That(resultItem).IsEqualTo((byte)0xAA);
   }

   [Test]
   public async Task AddFillsBufferWithoutWrapping()
   {
      byte[] result;

      {
         using var buffer = new CircularBuffer<byte>(3);
         buffer.Add(1);
         buffer.Add(2);
         buffer.Add(3);

         // Helper to extract data via indexer for verification
         result = new byte[3];
         for (var i = 0; i < 3; i++)
         {
            result[i] = buffer[i];
         }
      }

      byte[] shouldBe = [1, 2, 3];
      await Assert.That(result).IsEquivalentTo(shouldBe);
   }

   [Test]
   public async Task AddOverwritesOldestWhenFull()
   {
      byte[] result;
      int count;

      {
         using var buffer = new CircularBuffer<byte>(3);
         buffer.Add(1);
         buffer.Add(2);
         buffer.Add(3);
         
         // This should overwrite '1'
         buffer.Add(4);

         count = buffer.Count;
         
         result = new byte[3];
         for (var i = 0; i < 3; i++)
         {
            result[i] = buffer[i];
         }
      }

      await Assert.That(count).IsEqualTo(3); // Capacity remains 3

      byte[] shouldBe = [2, 3, 4];
      await Assert.That(result).IsEquivalentTo(shouldBe);
   }

   [Test]
   public async Task WrittenTwoSpanReturnsSingleSpanWhenContiguous()
   {
      byte[] firstSpanResult;
      byte[] secondSpanResult;

      {
         using var buffer = new CircularBuffer<byte>(5);
         buffer.Add(10);
         buffer.Add(20);

         var twoSpan = buffer.WrittenTwoSpan;
         
         // Assuming TwoSpan has .First and .Second properties based on constructor usage
         firstSpanResult = twoSpan.First.ToArray();
         secondSpanResult = twoSpan.Second.ToArray();
      }

      byte[] expectedFirst = [10, 20];
      byte[] expectedSecond = [];

      await Assert.That(firstSpanResult).IsEquivalentTo(expectedFirst);
      await Assert.That(secondSpanResult).IsEquivalentTo(expectedSecond);
   }

   [Test]
   public async Task WrittenTwoSpanReturnsSplitSpansWhenWrapped()
   {
      byte[] firstSpanResult;
      byte[] secondSpanResult;

      {
         using var buffer = new CircularBuffer<byte>(3);
         buffer.Add(1);
         buffer.Add(2);
         buffer.Add(3);
         buffer.Add(4);

         var twoSpan = buffer.WrittenTwoSpan;
         
         firstSpanResult = twoSpan.First.ToArray();
         secondSpanResult = twoSpan.Second.ToArray();
      }

      byte[] expectedFirst = [2, 3];
      byte[] expectedSecond = [4];

      await Assert.That(firstSpanResult).IsEquivalentTo(expectedFirst);
      await Assert.That(secondSpanResult).IsEquivalentTo(expectedSecond);
   }

   [Test]
   public async Task ClearResetsCountAndStart()
   {
      int count;
      byte[] resultAfterClear;

      {
         using var buffer = new CircularBuffer<byte>(5);
         buffer.Add(1);
         buffer.Add(2);
         
         buffer.Clear();
         
         count = buffer.Count;
         resultAfterClear = buffer.Buffer.ToArray();
      }

      await Assert.That(count).IsEqualTo(0);

      var expectedEmpty = "\0\0\0\0\0"u8.ToArray();
      await Assert.That(resultAfterClear).IsEquivalentTo(expectedEmpty);
   }

   [Test]
   public async Task EnumeratorIteratesInLogicalOrder()
   {
      byte[] result;

      {
         using var buffer = new CircularBuffer<byte>(3);
         buffer.Add(1);
         buffer.Add(2);
         buffer.Add(3);
         buffer.Add(4);

         var list = new List<byte>();
         foreach (var b in buffer)
         {
            list.Add(b);
         }
         result = list.ToArray();
      }

      byte[] shouldBe = [2, 3, 4];
      await Assert.That(result).IsEquivalentTo(shouldBe);
   }
}
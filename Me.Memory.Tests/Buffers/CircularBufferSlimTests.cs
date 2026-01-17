using Me.Memory.Buffers;

namespace Me.Memory.Tests.Buffers;

public class CircularBufferSlimTests
{
   [Test]
   public async Task ConstructorInitializesCorrectly()
   {
      int count;
      int capacity;

      {
         var buffer = new CircularBufferSlim<byte>(stackalloc byte[10]);
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
         var backingArray = new byte[5];
         var buffer = new CircularBufferSlim<byte>(backingArray);
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
         var buffer = new CircularBufferSlim<byte>(stackalloc byte[3]);
         buffer.Add(1);
         buffer.Add(2);
         buffer.Add(3);

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
         var buffer = new CircularBufferSlim<byte>(stackalloc byte[3]);
         buffer.Add(1);
         buffer.Add(2);
         buffer.Add(3);
         
         buffer.Add(4);

         count = buffer.Count;
         
         result = new byte[3];
         for (var i = 0; i < 3; i++)
         {
            result[i] = buffer[i];
         }
      }

      await Assert.That(count).IsEqualTo(3); 

      byte[] shouldBe = [2, 3, 4];
      await Assert.That(result).IsEquivalentTo(shouldBe);
   }

   [Test]
   public async Task WrittenTwoSpanReturnsSingleSpanWhenContiguous()
   {
      byte[] firstSpanResult;
      byte[] secondSpanResult;

      {
         var buffer = new CircularBufferSlim<byte>(stackalloc byte[5]);
         buffer.Add(10);
         buffer.Add(20);

         var twoSpan = buffer.WrittenTwoSpan;
         
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
         var buffer = new CircularBufferSlim<byte>(stackalloc byte[3]);
         buffer.Add(1);
         buffer.Add(2);
         buffer.Add(3);
         buffer.Add(4); // Overwrites 1. 

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
      var backingArray = new byte[5];

      {
         var buffer = new CircularBufferSlim<byte>(backingArray);
         buffer.Add(1);
         buffer.Add(2);
         
         buffer.Clear();
         
         count = buffer.Count;
      }

      // Check the backing array was zeroed out

      await Assert.That(count).IsEqualTo(0);

      var expectedEmpty = "\0\0\0\0\0"u8.ToArray();
      await Assert.That(backingArray).IsEquivalentTo(expectedEmpty);
   }

   [Test]
   public async Task EnumeratorIteratesInLogicalOrder()
   {
      byte[] result;

      {
         var buffer = new CircularBufferSlim<byte>(stackalloc byte[3]);
         buffer.Add(1);
         buffer.Add(2);
         buffer.Add(3);
         buffer.Add(4); // Wraps. Order: 2, 3, 4

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
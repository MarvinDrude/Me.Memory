using Me.Memory.Buffers;

namespace Me.Memory.Tests.Buffers;

public class BufferWriterTests
{
   [Test]
   public async Task ConstructorWithInitialSizeSetsCapacityCorrectly()
   {
      int capacity;
      int position;
      int writtenLength;

      {
         using var writer = new BufferWriter<int>(100);
         capacity = writer.Capacity;
         position = writer.Position;
         writtenLength = writer.WrittenSpan.Length;
      }

      await Assert.That(capacity).IsEqualTo(100);
      await Assert.That(position).IsEqualTo(0);
      await Assert.That(writtenLength).IsEqualTo(0);
   }
   
   [Test]
   public async Task SimpleGrowTest()
   {
      var writer = new BufferWriter<byte>(stackalloc byte[4], 2);
      byte[] arr;
      
      try
      {
         writer.Write([1, 2, 3, 4]);
         writer.Add(5);
         writer.Write([6, 7, 8, 9]);
      }
      finally
      {
         arr = writer.WrittenSpan.ToArray();
         writer.Dispose();
      }

      byte[] test = [1, 2, 3, 4, 5, 6, 7, 8, 9];
      await Assert.That(arr.SequenceEqual(test)).IsTrue();
   }
   
   [Test]
   public async Task InitialMinGrowCapacityIsRespectedOnFirstResize()
   {
      int capacity;
      const int startSize = 4;
      const int minGrow = 100;

      {
         using var writer = new BufferWriter<byte>(
            stackalloc byte[startSize],
            initalMinGrowCapacity: minGrow);

         writer.Advance(startSize);
         writer.Add(1);
            
         capacity = writer.Capacity;
      }

      await Assert.That(capacity).IsGreaterThanOrEqualTo(startSize + minGrow);
   }
   
   [Test]
   public async Task AdvanceIncreasesPositionAndResizesIfNeeded()
   {
      byte[] result;
      int position;

      {
         using var writer = new BufferWriter<byte>(stackalloc byte[5]);

         writer.Write([1, 2, 3]);
         
         writer.Advance(4); 
         writer.Write([9]);

         position = writer.Position;
         result = writer.WrittenSpan.ToArray();
      }

      await Assert.That(position).IsEqualTo(8);
      
      byte[] test = [1, 2, 3, 0, 0, 0, 0, 9];
      await Assert.That(result.SequenceEqual(test)).IsTrue();
   }
   
   [Test]
   public async Task PositionSetThrowsIfOutOfRange()
   {
      await Assert.That(() =>
      {
         var writer = new BufferWriter<byte>(stackalloc byte[10]);
         writer.Position = 11; 
      }).Throws<ArgumentOutOfRangeException>();

      await Assert.That(() =>
      {
         var writer = new BufferWriter<byte>(stackalloc byte[10]);
         writer.Position = -1;
      }).Throws<ArgumentOutOfRangeException>();
   }
   
   [Test]
   public async Task AcquireSpanReturnsValidSliceAndMovesPosition()
   {
      byte[] result;
      int position;

      {
         using var writer = new BufferWriter<byte>(stackalloc byte[10]);
         writer.Write([1, 2]);

         var span = writer.AcquireSpan(3, movePosition: true);
         span[0] = 3;
         span[1] = 4;
         span[2] = 5;
            
         position = writer.Position;
         result = writer.WrittenSpan.ToArray();
      }

      await Assert.That(position).IsEqualTo(5);
      
      byte[] test = [1, 2, 3, 4, 5];
      await Assert.That(result.SequenceEqual(test)).IsTrue();
   }
   
   [Test]
   public async Task MoveShiftsDataAndResizesWhenMovingBeyondCapacity()
   {
      byte[] result;
      int position;

      {
         using var writer = new BufferWriter<byte>(stackalloc byte[4]);
         writer.Write([1, 2, 3, 4]);

         writer.Move(fromStart: 0, fromSize: 2, toStart: 10, movePosition: true);
            
         position = writer.Position;
         result = writer.WrittenSpan.ToArray();
      }

      await Assert.That(position).IsEqualTo(12);
        
      await Assert.That(result[10]).IsEqualTo((byte)1);
      await Assert.That(result[11]).IsEqualTo((byte)2);
      
      await Assert.That(result[0]).IsEqualTo((byte)1);
   }
}
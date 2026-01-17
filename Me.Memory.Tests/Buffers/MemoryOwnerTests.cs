using Me.Memory.Buffers;

namespace Me.Memory.Tests.Buffers;

public class MemoryOwnerTests
{
   [Test]
   public async Task ConstructorRentsBufferWithCorrectSize()
   {
      int length;
      int capacity;

      {
         using var owner = new MemoryOwner<byte>(10);
         length = owner.Length;
         capacity = owner.Capacity;
      }

      await Assert.That(length).IsEqualTo(10);
      await Assert.That(capacity).IsGreaterThanOrEqualTo(10);
   }

   [Test]
   public async Task ConstructorClearsBufferByDefault()
   {
      var dirtyArray = System.Buffers.ArrayPool<byte>.Shared.Rent(100);
      dirtyArray.AsSpan().Fill(0xFF);
      System.Buffers.ArrayPool<byte>.Shared.Return(dirtyArray);

      byte[] result;

      {
         using var owner = new MemoryOwner<byte>(100); 
         result = owner.Span.ToArray();
      }

      var expected = new byte[100];
      await Assert.That(result).IsEquivalentTo(expected);
   }

   [Test]
   public async Task ConstructorRespectsClearArrayFalse()
   {
      var dirtyArray = System.Buffers.ArrayPool<byte>.Shared.Rent(50);
      dirtyArray.AsSpan().Fill(0xAA);
      System.Buffers.ArrayPool<byte>.Shared.Return(dirtyArray);

      var containsData = false;
      
      {
         using var owner = new MemoryOwner<byte>(50, clearArray: false);
         
         foreach (var b in owner.Span)
         {
            if (b != 0xAA) continue;
            
            containsData = true;
            break;
         }
      }

      await Assert.That(containsData).IsTrue(); 
   }

   [Test]
   public async Task TryResizeUpdatesLengthIfWithinCapacity()
   {
      int newLength;
      bool success;

      {
         using var owner = new MemoryOwner<byte>(100);
         
         success = owner.TryResize(50);
         newLength = owner.Length;
      }

      await Assert.That(success).IsTrue();
      await Assert.That(newLength).IsEqualTo(50);
   }

   [Test]
   public async Task TryResizeFailsIfNewSizeExceedsCapacity()
   {
      bool success;
      int originalLength;

      {
         using var owner = new MemoryOwner<byte>(10);
         var capacity = owner.Capacity;
         
         success = owner.TryResize(capacity + 1);
         originalLength = owner.Length;
      }

      await Assert.That(success).IsFalse();
      await Assert.That(originalLength).IsEqualTo(10);
   }

   [Test]
   public async Task SpanAndMemoryReflectCurrentLength()
   {
      byte[] spanResult;
      
      {
         using var owner = new MemoryOwner<byte>(10);
         owner.Span.Fill(1);
         
         // Resize to 5
         owner.TryResize(5);
         
         // Span should now only show 5 elements
         spanResult = owner.Span.ToArray();
      }

      byte[] expected = [1, 1, 1, 1, 1];
      await Assert.That(spanResult).IsEquivalentTo(expected);
   }

   [Test]
   public Task DisposeDoesNotCrashOnDoubleDispose()
   {
      // It is good practice that double dispose does not throw
      var owner = new MemoryOwner<byte>(10);
      
      owner.Dispose();
      
      // Act
      try 
      {
         owner.Dispose();
      }
      catch (Exception)
      {
         Assert.Fail("Double dispose should not throw");
      }
      
      return Task.CompletedTask;
   }
}
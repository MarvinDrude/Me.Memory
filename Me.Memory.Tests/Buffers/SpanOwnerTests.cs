using Me.Memory.Buffers;

namespace Me.Memory.Tests.Buffers;

public class SpanOwnerTests
{
   [Test]
   public async Task ConstructorRentsBufferWithCorrectSize()
   {
      int length;
      int spanLength;

      {
         using var owner = new SpanOwner<byte>(10);
         length = owner.Length;
         
         spanLength = owner.Span.Length;
      }

      await Assert.That(length).IsEqualTo(10);
      await Assert.That(spanLength).IsGreaterThanOrEqualTo(10);
   }

   [Test]
   public async Task ConstructorClearsBufferByDefault()
   {
      byte[] result;

      {
         // We request a buffer and expect it to be zeroed
         using var owner = new SpanOwner<byte>(50);
         result = owner.Span.ToArray();
      }

      var expected = new byte[50]; // Default zeros
      await Assert.That(result).IsEquivalentTo(expected);
   }

   [Test]
   public async Task ConstructorWrapsExistingSpan()
   {
      byte[] result;
      int length;

      {
         // Create a stack-allocated span to wrap
         Span<byte> initialSpan = [1, 2, 3];
         
         // Wrap it
         using var owner = new SpanOwner<byte>(initialSpan);
         
         length = owner.Length;
         result = owner.Span.ToArray();
      }

      await Assert.That(length).IsEqualTo(3);
      
      byte[] expected = [1, 2, 3];
      await Assert.That(result).IsEquivalentTo(expected);
   }

   [Test]
   public Task DisposeSafetyCheck()
   {
      // Ensure calling Dispose works without exception
      {
         using var owner = new SpanOwner<byte>(10);
         // Implicit Dispose at end of scope
      }

      // Explicit Dispose check
      {
         var owner = new SpanOwner<byte>(10);
         owner.Dispose();
      }
      
      return Task.CompletedTask;
   }

   [Test]
   public async Task DisposeResetsLength()
   {
      int lengthAfterDispose;

      {
         var owner = new SpanOwner<byte>(10);
         owner.Dispose();
         lengthAfterDispose = owner.Length;
      }

      await Assert.That(lengthAfterDispose).IsEqualTo(0);
   }
}
using Me.Memory.Buffers;

namespace Me.Memory.Tests.Buffers;

public class BufferWriterTests
{
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
   
   
}
using Me.Memory.Buffers;
using Me.Memory.Constants;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization;

public static class SerializerCache<T>
{
   public static readonly ISerializer<T> Instance = SerializerRegistry.For<T>();

   public static MemoryOwner<byte> SerializeMemory(T value)
   {
      var writer = new ByteWriter(stackalloc byte[BufferConstants.StackSafeByteBufferSize]);
      MemoryOwner<byte> result = default;

      try
      {
         Instance.Write(ref writer, ref value);

         result = new MemoryOwner<byte>(writer.Position);
         writer.WrittenSpan.CopyTo(result.Span);
      }
      catch
      {
         result.Dispose();
         throw;
      }
      finally
      {
         writer.Dispose();
      }

      return result;
   }

   public static T DeserializeMemory(scoped in ReadOnlySpan<byte> span)
   {
      var reader = new ByteReader(span);
      return Instance.TryRead(ref reader, out var value) 
         ? value 
         : throw new InvalidOperationException($"Failed to deserialize {typeof(T)}");
   }

   public static void SerializeStream(Stream stream, T value, int chunkSize = 1024 * 1024)
   {
      var writer = new ByteWriter(stream, chunkSize);
      
      try
      {
         Instance.Write(ref writer, ref value);
      }
      finally
      {
         writer.Dispose();
      }
   }

   public static T DeserializeStream(Stream stream, int chunkSize = 1024 * 1024)
   {
      var reader = new ByteReader(stream, chunkSize);
      return Instance.TryRead(ref reader, out var value) 
         ? value 
         : throw new InvalidOperationException($"Failed to deserialize {typeof(T)}");
   }
}
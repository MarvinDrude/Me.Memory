using System.Runtime.CompilerServices;
using Me.Memory.Buffers;
using Me.Memory.Serialization;

namespace Me.Memory.Extensions;

public static class SerializeExtensions
{
   extension<T>(T value)
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public MemoryOwner<byte> SerializeMemory()
      {
         return SerializerCache<T>.SerializeMemory(value);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static T DeserializeMemory(scoped in ReadOnlyMemory<byte> memory)
      {
         return SerializerCache<T>.DeserializeMemory(memory.Span);       
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void SerializeStream(Stream stream, int chunkSize = 1024 * 1024)
      {
         SerializerCache<T>.SerializeStream(stream, value, chunkSize);
      }
      
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public static T DeserializeStream(Stream stream, int chunkSize = 1024 * 1024)
      {
         return SerializerCache<T>.DeserializeStream(stream, chunkSize);
      }
   }
}
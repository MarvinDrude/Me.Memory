using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Collections;

public abstract class ArraySerializer<T> : ISerializer<T[]>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in T[] value)
   {
      throw new NotImplementedException();
   }

   public static bool TryRead(ref SequenceReader<byte> reader, [MaybeNullWhen(false)] out T[] value)
   {
      throw new NotImplementedException();
   }

   public static int CalculateByteLength(scoped in T[] value)
   {
      throw new NotImplementedException();
   }
}
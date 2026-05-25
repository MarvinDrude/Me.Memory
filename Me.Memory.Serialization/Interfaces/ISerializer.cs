using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Me.Memory.Buffers;

namespace Me.Memory.Serialization.Interfaces;

public interface ISerializer<T> : ISerializer
   where T : allows ref struct
{
   public int Write(ref BufferWriter<byte> writer, scoped in T value);
   
   public bool TryRead(ref SequenceReader<byte> reader, [MaybeNullWhen(false)] out T value);

   public int CalculateByteLength(scoped in T value);
}

public interface ISerializer;

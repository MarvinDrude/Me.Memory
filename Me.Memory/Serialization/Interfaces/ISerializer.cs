using System.Diagnostics.CodeAnalysis;
using Me.Memory.Buffers;

namespace Me.Memory.Serialization.Interfaces;

public interface ISerializer<T> : ISerializer
   where T : allows ref struct
{
   public void Write(ref ByteWriter writer, ref T value);
   
   public bool TryRead(ref ByteReader reader, [MaybeNullWhen(false)] out T value);
}

public interface ISerializer;
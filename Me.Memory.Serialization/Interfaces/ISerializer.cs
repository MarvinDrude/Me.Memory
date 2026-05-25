using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Me.Memory.Buffers;

namespace Me.Memory.Serialization.Interfaces;

/// <summary>
/// Core compile-time generic serializer interface using zero-overhead static abstract methods.
/// </summary>
public interface ISerializer<T> : ISerializer
   where T : allows ref struct
{
   public static abstract int Write(ref BufferWriter<byte> writer, scoped in T value);
   
   public static abstract bool TryRead(ref SequenceReader<byte> reader, [MaybeNullWhen(false)] out T value);

   public static abstract int CalculateByteLength(scoped in T value);
}

/// <summary>
/// Core instance-based generic serializer interface used for dynamic lookup and runtime resolution.
/// </summary>
public interface IInstanceSerializer<T>
   where T : allows ref struct
{
   public int Write(ref BufferWriter<byte> writer, scoped in T value);
   
   public bool TryRead(ref SequenceReader<byte> reader, [MaybeNullWhen(false)] out T value);

   public int CalculateByteLength(scoped in T value);
}

public interface ISerializer;

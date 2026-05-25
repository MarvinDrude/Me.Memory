using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters;

/// <summary>
/// A zero-cost, stateless wrapper that converts a static <see cref="ISerializer{T}"/> 
/// into an <see cref="IInstanceSerializer{T}"/> for dynamic lookup and registry caching.
/// </summary>
public sealed class StaticSerializerWrapper<T, TSerializer> : IInstanceSerializer<T>
   where TSerializer : ISerializer<T>
   where T : allows ref struct
{
   public int Write(ref BufferWriter<byte> writer, scoped in T value)
   {
      return TSerializer.Write(ref writer, in value);
   }

   public bool TryRead(ref SequenceReader<byte> reader, [MaybeNullWhen(false)] out T value)
   {
      return TSerializer.TryRead(ref reader, out value);
   }

   public int CalculateByteLength(scoped in T value)
   {
      return TSerializer.CalculateByteLength(in value);
   }
}

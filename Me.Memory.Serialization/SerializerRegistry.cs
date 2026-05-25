using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Me.Memory.Buffers;

namespace Me.Memory.Serialization;

public delegate int WriteDelegate<T>(ref BufferWriter<byte> writer, scoped in T value)
   where T : allows ref struct;

public delegate bool TryReadDelegate<T>(ref SequenceReader<byte> reader, [MaybeNullWhen(false)] out T value)
   where T : allows ref struct;

public delegate int CalculateByteLengthDelegate<T>(scoped in T value)
   where T : allows ref struct;

/// <summary>
/// A zero-overhead, reflection-free static registry for serializers using delegate caching.
/// </summary>
public static class SerializerRegistry<T>
   where T : allows ref struct
{
   /// <summary>
   /// The cached static write delegate for type <typeparamref name="T"/>.
   /// </summary>
   public static WriteDelegate<T>? Write { get; set; }

   /// <summary>
   /// The cached static read delegate for type <typeparamref name="T"/>.
   /// </summary>
   public static TryReadDelegate<T>? TryRead { get; set; }

   /// <summary>
   /// The cached static length calculation delegate for type <typeparamref name="T"/>.
   /// </summary>
   public static CalculateByteLengthDelegate<T>? CalculateByteLength { get; set; }
   
   /// <summary>
   /// Gets the cached static write delegate for type <typeparamref name="T"/>.
   /// </summary>
   public static WriteDelegate<T> GetWrite() => Write 
      ?? throw new InvalidOperationException($"Write delegate not set for type {typeof(T)}.");
   
   /// <summary>
   /// Gets the cached static read delegate for type <typeparamref name="T"/>.
   /// </summary>
   public static TryReadDelegate<T> GetTryRead() => TryRead 
      ?? throw new InvalidOperationException($"TryRead delegate not set for type {typeof(T)}.");
   
   /// <summary>
   /// Gets the cached static length calculation delegate for type <typeparamref name="T"/>.
   /// </summary>
   public static CalculateByteLengthDelegate<T> GetCalculateByteLength() => CalculateByteLength 
      ?? throw new InvalidOperationException($"CalculateByteLength delegate not set for type {typeof(T)}.");
}

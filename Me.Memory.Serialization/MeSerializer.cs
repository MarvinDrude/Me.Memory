using System.Buffers;
using Me.Memory.Buffers;

namespace Me.Memory.Serialization;

/// <summary>
/// High-performance, zero-overhead helper utility for easy serialization and deserialization.
/// </summary>
public static class MeSerializer
{
   /// <summary>
   /// Calculates the number of bytes required to serialize a value of type <typeparamref name="T"/>.
   /// </summary>
   /// <typeparam name="T">The type of the value.</typeparam>
   /// <param name="value">The value to calculate the length for.</param>
   /// <returns>The calculated byte length.</returns>
   public static int CalculateByteLength<T>(scoped in T value)
      where T : allows ref struct
   {
      return SerializerRegistry<T>.GetCalculateByteLength()(in value);
   }

   /// <summary>
   /// Serializes a value of type <typeparamref name="T"/> into a newly allocated byte array.
   /// </summary>
   /// <typeparam name="T">The type of the value to serialize.</typeparam>
   /// <param name="value">The scoped reference to the value to serialize.</param>
   /// <returns>A byte array containing the serialized representation of the value.</returns>
   public static byte[] Serialize<T>(scoped in T value)
      where T : allows ref struct
   {
      var length = SerializerRegistry<T>.GetCalculateByteLength()(in value);
      if (length == 0)
      {
         return [];
      }

      var buffer = new byte[length];
      var writer = new BufferWriter<byte>(buffer);
      try
      {
         SerializerRegistry<T>.GetWrite()(ref writer, in value);
      }
      finally
      {
         writer.Dispose();
      }

      return buffer;
   }

   /// <summary>
   /// Serializes a value of type <typeparamref name="T"/> into the provided destination span.
   /// </summary>
   /// <typeparam name="T">The type of the value to serialize.</typeparam>
   /// <param name="value">The value to serialize.</param>
   /// <param name="destination">The span to write the serialized data to.</param>
   /// <returns>The number of bytes written to the destination span.</returns>
   /// <exception cref="ArgumentException">Thrown if the destination span is too small to hold the serialized value.</exception>
   public static int Serialize<T>(scoped in T value, Span<byte> destination)
      where T : allows ref struct
   {
      var length = SerializerRegistry<T>.GetCalculateByteLength()(in value);
      if (length > destination.Length)
      {
         throw new ArgumentException($"Destination span is too small. Required: {length}, Actual: {destination.Length}.", nameof(destination));
      }

      if (length == 0)
      {
         return 0;
      }

      var writer = new BufferWriter<byte>(destination);
      try
      {
         SerializerRegistry<T>.GetWrite()(ref writer, in value);
      }
      finally
      {
         writer.Dispose();
      }
      return length;
   }

   /// <summary>
   /// Attempts to serialize a value of type <typeparamref name="T"/> into the provided destination span.
   /// </summary>
   /// <typeparam name="T">The type of the value to serialize.</typeparam>
   /// <param name="value">The value to serialize.</param>
   /// <param name="destination">The span to write the serialized data to.</param>
   /// <param name="bytesWritten">When this method returns, contains the number of bytes written to the destination span.</param>
   /// <returns>True if the serialization succeeded, otherwise false.</returns>
   public static bool TrySerialize<T>(scoped in T value, Span<byte> destination, out int bytesWritten)
      where T : allows ref struct
   {
      var length = SerializerRegistry<T>.GetCalculateByteLength()(in value);
      if (length > destination.Length)
      {
         bytesWritten = 0;
         return false;
      }

      if (length == 0)
      {
         bytesWritten = 0;
         return true;
      }

      var writer = new BufferWriter<byte>(destination);
      try
      {
         SerializerRegistry<T>.GetWrite()(ref writer, in value);
      }
      finally
      {
         writer.Dispose();
      }
      bytesWritten = length;
      return true;
   }

   /// <summary>
   /// Serializes a value of type <typeparamref name="T"/> into the provided destination span without pre-calculating the byte length.
   /// </summary>
   /// <typeparam name="T">The type of the value to serialize.</typeparam>
   /// <param name="value">The value to serialize.</param>
   /// <param name="destination">The span to write the serialized data to.</param>
   /// <returns>The number of bytes written to the destination span.</returns>
   /// <exception cref="ArgumentException">Thrown if the destination span is too small to hold the serialized value.</exception>
   public static int SerializeWithoutPrecalculation<T>(scoped in T value, Span<byte> destination)
      where T : allows ref struct
   {
      var writer = new BufferWriter<byte>(destination);
      try
      {
         SerializerRegistry<T>.GetWrite()(ref writer, in value);

         if (writer.Capacity > destination.Length)
         {
            throw new ArgumentException("Destination span is too small.", nameof(destination));
         }

         return writer.Position;
      }
      finally
      {
         writer.Dispose();
      }
   }

   /// <summary>
   /// Attempts to serialize a value of type <typeparamref name="T"/> into the provided destination span without pre-calculating the byte length.
   /// </summary>
   /// <typeparam name="T">The type of the value to serialize.</typeparam>
   /// <param name="value">The value to serialize.</param>
   /// <param name="destination">The span to write the serialized data to.</param>
   /// <param name="bytesWritten">When this method returns, contains the number of bytes written to the destination span.</param>
   /// <returns>True if the serialization succeeded, otherwise false.</returns>
   public static bool TrySerializeWithoutPrecalculation<T>(scoped in T value, Span<byte> destination, out int bytesWritten)
      where T : allows ref struct
   {
      var writer = new BufferWriter<byte>(destination);
      try
      {
         SerializerRegistry<T>.GetWrite()(ref writer, in value);

         if (writer.Capacity > destination.Length)
         {
            bytesWritten = 0;
            return false;
         }

         bytesWritten = writer.Position;
         return true;
      }
      finally
      {
         writer.Dispose();
      }
   }

   /// <summary>
   /// Serializes a value of type <typeparamref name="T"/> into a newly allocated byte array without pre-calculating the byte length.
   /// </summary>
   /// <typeparam name="T">The type of the value to serialize.</typeparam>
   /// <param name="value">The value to serialize.</param>
   /// <param name="initialCapacity">The initial capacity of the internal buffer writer (default is 256 bytes).</param>
   /// <returns>A byte array containing the serialized representation of the value.</returns>
   public static byte[] SerializeWithoutPrecalculation<T>(scoped in T value, int initialCapacity = 256)
      where T : allows ref struct
   {
      var writer = new BufferWriter<byte>(initialCapacity);
      try
      {
         SerializerRegistry<T>.GetWrite()(ref writer, in value);
         return writer.WrittenSpan.ToArray();
      }
      finally
      {
         writer.Dispose();
      }
   }

   /// <summary>
   /// Attempts to deserialize a value of type <typeparamref name="T"/> from a read-only memory block.
   /// </summary>
   /// <typeparam name="T">The type of the value to deserialize.</typeparam>
   /// <param name="data">The read-only memory containing the serialized data.</param>
   /// <param name="value">When this method returns, contains the deserialized value if successful, or default otherwise.</param>
   /// <returns>True if deserialization succeeded, otherwise false.</returns>
   public static bool TryDeserialize<T>(ReadOnlyMemory<byte> data, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out T value)
      where T : allows ref struct
   {
      var sequence = new ReadOnlySequence<byte>(data);
      var reader = new SequenceReader<byte>(sequence);
      return SerializerRegistry<T>.GetTryRead()(ref System.Runtime.CompilerServices.Unsafe.AsRef(in reader), out value);
   }

   /// <summary>
   /// Attempts to deserialize a value of type <typeparamref name="T"/> from a read-only sequence.
   /// </summary>
   /// <typeparam name="T">The type of the value to deserialize.</typeparam>
   /// <param name="sequence">The read-only sequence containing the serialized data.</param>
   /// <param name="value">When this method returns, contains the deserialized value if successful, or default otherwise.</param>
   /// <returns>True if deserialization succeeded, otherwise false.</returns>
   public static bool TryDeserialize<T>(ReadOnlySequence<byte> sequence, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out T value)
      where T : allows ref struct
   {
      var reader = new SequenceReader<byte>(sequence);
      return SerializerRegistry<T>.GetTryRead()(ref System.Runtime.CompilerServices.Unsafe.AsRef(in reader), out value);
   }

   /// <summary>
   /// Attempts to deserialize a value of type <typeparamref name="T"/> from a byte array.
   /// </summary>
   /// <typeparam name="T">The type of the value to deserialize.</typeparam>
   /// <param name="data">The byte array containing the serialized data.</param>
   /// <param name="value">When this method returns, contains the deserialized value if successful, or default otherwise.</param>
   /// <returns>True if deserialization succeeded, otherwise false.</returns>
   public static bool TryDeserialize<T>(byte[] data, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out T value)
      where T : allows ref struct
   {
      var sequence = new ReadOnlySequence<byte>(data);
      var reader = new SequenceReader<byte>(sequence);
      return SerializerRegistry<T>.GetTryRead()(ref System.Runtime.CompilerServices.Unsafe.AsRef(in reader), out value);
   }

   /// <summary>
   /// Deserializes a value of type <typeparamref name="T"/> from a read-only memory block.
   /// </summary>
   /// <typeparam name="T">The type of the value to deserialize.</typeparam>
   /// <param name="data">The read-only memory containing the serialized data.</param>
   /// <returns>The deserialized value.</returns>
   /// <exception cref="InvalidOperationException">Thrown if deserialization fails.</exception>
   public static T Deserialize<T>(ReadOnlyMemory<byte> data)
      where T : allows ref struct
   {
      if (!TryDeserialize<T>(data, out var value))
      {
         throw new InvalidOperationException($"Failed to deserialize type {typeof(T)}.");
      }
      return value;
   }

   /// <summary>
   /// Deserializes a value of type <typeparamref name="T"/> from a byte array.
   /// </summary>
   /// <typeparam name="T">The type of the value to deserialize.</typeparam>
   /// <param name="data">The byte array containing the serialized data.</param>
   /// <returns>The deserialized value.</returns>
   /// <exception cref="InvalidOperationException">Thrown if deserialization fails.</exception>
   public static T Deserialize<T>(byte[] data)
      where T : allows ref struct
   {
      if (!TryDeserialize<T>(data, out var value))
      {
         throw new InvalidOperationException($"Failed to deserialize type {typeof(T)}.");
      }
      return value;
   }

   /// <summary>
   /// Deserializes a value of type <typeparamref name="T"/> from a read-only sequence.
   /// </summary>
   /// <typeparam name="T">The type of the value to deserialize.</typeparam>
   /// <param name="sequence">The read-only sequence containing the serialized data.</param>
   /// <returns>The deserialized value.</returns>
   /// <exception cref="InvalidOperationException">Thrown if deserialization fails.</exception>
   public static T Deserialize<T>(ReadOnlySequence<byte> sequence)
      where T : allows ref struct
   {
      if (!TryDeserialize<T>(sequence, out var value))
      {
         throw new InvalidOperationException($"Failed to deserialize type {typeof(T)}.");
      }
      return value;
   }
}

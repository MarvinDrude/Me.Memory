using System.Buffers;
using Me.Memory.Buffers;

namespace Me.Memory.Serialization;

/// <summary>
/// High-performance, zero-overhead helper utility for easy serialization and deserialization.
/// </summary>
public static class MeSerializer
{
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
      SerializerRegistry<T>.GetWrite()(ref writer, in value);

      return buffer;
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
}

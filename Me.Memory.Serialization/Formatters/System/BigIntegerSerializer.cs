using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.System;

/// <summary>
/// Serializer for BigInteger values.
/// </summary>
public abstract class BigIntegerSerializer : ISerializer<BigInteger>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in BigInteger value)
   {
      var byteCount = value.GetByteCount();
      var span = writer.AcquireSpan(sizeof(int) + byteCount);
      
      BinaryPrimitives.WriteInt32LittleEndian(span, byteCount);
      if (!value.TryWriteBytes(span[sizeof(int)..], out _))
      {
         throw new InvalidOperationException("Failed to write BigInteger bytes.");
      }
      
      return sizeof(int) + byteCount;
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out BigInteger value)
   {
      if (!reader.TryReadLittleEndian(out int length))
      {
         value = default;
         return false;
      }

      if (length < 0)
      {
         throw new InvalidOperationException("Serialized BigInteger length cannot be negative.");
      }

      if (reader.Remaining < length)
      {
         value = default;
         return false;
      }

      if (reader.UnreadSpan.Length >= length)
      {
         value = new BigInteger(reader.UnreadSpan[..length]);
         reader.Advance(length);
         return true;
      }

      using var owner = length <= 256
         ? new SpanOwner<byte>(stackalloc byte[length])
         : new SpanOwner<byte>(length);
      var bytes = owner.Span;
      
      reader.UnreadSequence.Slice(0, length).CopyTo(bytes);
      value = new BigInteger(bytes);
      reader.Advance(length);
      
      return true;
   }

   public static int CalculateByteLength(scoped in BigInteger value)
   {
      return sizeof(int) + value.GetByteCount();
   }
}

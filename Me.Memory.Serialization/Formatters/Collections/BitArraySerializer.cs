using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Collections;

/// <summary>
/// Serializer for BitArray values.
/// </summary>
public abstract class BitArraySerializer : ISerializer<BitArray?>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in BitArray? value)
   {
      if (value is null)
      {
         var span = writer.AcquireSpan(sizeof(int));
         BinaryPrimitives.WriteInt32LittleEndian(span, -1);
         
         return sizeof(int);
      }

      var byteCount = (value.Length + 7) / 8;
      var spanData = writer.AcquireSpan(sizeof(int) + byteCount);
      
      BinaryPrimitives.WriteInt32LittleEndian(spanData, value.Length);
      var dest = spanData[sizeof(int)..];
      dest.Clear();

      for (var i = 0; i < value.Length; i++)
      {
         if (value[i])
         {
            dest[i >> 3] |= (byte)(1 << (i & 7));
         }
      }

      return sizeof(int) + byteCount;
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out BitArray? value)
   {
      if (!reader.TryReadLittleEndian(out int length))
      {
         value = null;
         return false;
      }

      if (length < 0)
      {
         value = null;
         return true;
      }

      var byteCount = (length + 7) / 8;
      if (reader.Remaining < byteCount)
      {
         value = null;
         return false;
      }

      var bitArray = new BitArray(length);
      
      if (reader.UnreadSpan.Length >= byteCount)
      {
         var source = reader.UnreadSpan[..byteCount];
         
         for (var i = 0; i < length; i++)
         {
            var isSet = (source[i >> 3] & (1 << (i & 7))) != 0;
            bitArray[i] = isSet;
         }
         
         reader.Advance(byteCount);
         value = bitArray;
         
         return true;
      }

      using var owner = byteCount <= 256
         ? new SpanOwner<byte>(stackalloc byte[byteCount])
         : new SpanOwner<byte>(byteCount);
      
      var bytes = owner.Span;
      reader.UnreadSequence.Slice(0, byteCount).CopyTo(bytes);

      for (var i = 0; i < length; i++)
      {
         var isSet = (bytes[i >> 3] & (1 << (i & 7))) != 0;
         bitArray[i] = isSet;
      }
      
      reader.Advance(byteCount);

      value = bitArray;
      return true;
   }

   public static int CalculateByteLength(scoped in BitArray? value)
   {
      if (value is null)
      {
         return sizeof(int);
      }

      return sizeof(int) + ((value.Length + 7) / 8);
   }
}

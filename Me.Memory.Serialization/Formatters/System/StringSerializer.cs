using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.System;

public sealed class StringSerializer : ISerializer<string?>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in string? value)
   {
      if (value is null)
      {
         var lengthSpan = writer.AcquireSpan(sizeof(int));
         BinaryPrimitives.WriteInt32LittleEndian(lengthSpan, -1);
         
         return sizeof(int);
      }

      var byteCount = Encoding.UTF8.GetByteCount(value);
      var span = writer.AcquireSpan(sizeof(int) + byteCount);
      
      BinaryPrimitives.WriteInt32LittleEndian(span, byteCount);
      Encoding.UTF8.GetBytes(value, span[sizeof(int)..]);
      
      return sizeof(int) + byteCount;
   }

   public static bool TryRead(ref SequenceReader<byte> reader, [MaybeNullWhen(false)] out string? value)
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

      if (reader.Remaining < length)
      {
         value = null;
         return false;
      }

      if (reader.UnreadSpan.Length >= length)
      {
         value = Encoding.UTF8.GetString(reader.UnreadSpan[..length]);
         reader.Advance(length);
         
         return true;
      }

      using var owner = length <= 256
         ? new SpanOwner<byte>(stackalloc byte[length])
         : new SpanOwner<byte>(length);
      var stringBytes = owner.Span;
      
      reader.UnreadSequence.Slice(0, length).CopyTo(stringBytes);
      
      value = Encoding.UTF8.GetString(stringBytes);
      reader.Advance(length);
      
      return true;
   }

   public static int CalculateByteLength(scoped in string? value)
   {
      if (value is null) return sizeof(int);
      return sizeof(int) + Encoding.UTF8.GetByteCount(value);
   }
}
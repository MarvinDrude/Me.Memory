using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.System;

/// <summary>
/// Serializer for Uri values.
/// </summary>
public abstract class UriSerializer : ISerializer<Uri>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in Uri value)
   {
      ArgumentNullException.ThrowIfNull(value);

      var originalString = value.OriginalString;
      var byteCount = Encoding.UTF8.GetByteCount(originalString);
      var span = writer.AcquireSpan(sizeof(int) + byteCount);
      
      BinaryPrimitives.WriteInt32LittleEndian(span, byteCount);
      Encoding.UTF8.GetBytes(originalString, span[sizeof(int)..]);
      
      return sizeof(int) + byteCount;
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out Uri value)
   {
      if (!reader.TryReadLittleEndian(out int length))
      {
         value = null!;
         return false;
      }

      if (length < 0)
      {
         throw new InvalidOperationException("Serialized Uri length cannot be negative.");
      }

      if (reader.Remaining < length)
      {
         value = null!;
         return false;
      }

      string uriString;
      if (reader.UnreadSpan.Length >= length)
      {
         uriString = Encoding.UTF8.GetString(reader.UnreadSpan[..length]);
         reader.Advance(length);
      }
      else
      {
         using var owner = length <= 256
            ? new SpanOwner<byte>(stackalloc byte[length])
            : new SpanOwner<byte>(length);
         var stringBytes = owner.Span;
         
         reader.UnreadSequence.Slice(0, length).CopyTo(stringBytes);
         uriString = Encoding.UTF8.GetString(stringBytes);
         reader.Advance(length);
      }

      value = new Uri(uriString, UriKind.RelativeOrAbsolute);
      return true;
   }

   public static int CalculateByteLength(scoped in Uri value)
   {
      ArgumentNullException.ThrowIfNull(value);
      return sizeof(int) + Encoding.UTF8.GetByteCount(value.OriginalString);
   }
}

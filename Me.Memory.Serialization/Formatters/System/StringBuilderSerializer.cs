using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.System;

/// <summary>
/// Serializer for StringBuilder values.
/// </summary>
public abstract class StringBuilderSerializer : ISerializer<StringBuilder?>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in StringBuilder? value)
   {
      if (value is null)
      {
         var lengthSpan = writer.AcquireSpan(sizeof(int));
         BinaryPrimitives.WriteInt32LittleEndian(lengthSpan, -1);
         return sizeof(int);
      }

      var byteCount = 0;
      foreach (var chunk in value.GetChunks())
      {
         byteCount += Encoding.UTF8.GetByteCount(chunk.Span);
      }

      var span = writer.AcquireSpan(sizeof(int) + byteCount);
      BinaryPrimitives.WriteInt32LittleEndian(span, byteCount);

      var dest = span[sizeof(int)..];
      foreach (var chunk in value.GetChunks())
      {
         var written = Encoding.UTF8.GetBytes(chunk.Span, dest);
         dest = dest[written..];
      }

      return sizeof(int) + byteCount;
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out StringBuilder? value)
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
         var str = Encoding.UTF8.GetString(reader.UnreadSpan[..length]);
         value = new StringBuilder(str);
         reader.Advance(length);
         
         return true;
      }

      using var owner = length <= 256
         ? new SpanOwner<byte>(stackalloc byte[length])
         : new SpanOwner<byte>(length);
      var stringBytes = owner.Span;
      
      reader.UnreadSequence.Slice(0, length).CopyTo(stringBytes);
      var str2 = Encoding.UTF8.GetString(stringBytes);
      
      value = new StringBuilder(str2);
      reader.Advance(length);
      
      return true;
   }

   public static int CalculateByteLength(scoped in StringBuilder? value)
   {
      if (value is null)
      {
         return sizeof(int);
      }

      var byteCount = 0;
      foreach (var chunk in value.GetChunks())
      {
         byteCount += Encoding.UTF8.GetByteCount(chunk.Span);
      }
      
      return sizeof(int) + byteCount;
   }
}

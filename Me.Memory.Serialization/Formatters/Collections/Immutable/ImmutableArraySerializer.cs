using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Immutable;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Collections.Immutable;

/// <summary>
/// Serializer for ImmutableArray values.
/// </summary>
public abstract class ImmutableArraySerializer<T> : ISerializer<ImmutableArray<T>>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in ImmutableArray<T> value)
   {
      if (value.IsDefault)
      {
         var lengthSpan = writer.AcquireSpan(sizeof(int));
         BinaryPrimitives.WriteInt32LittleEndian(lengthSpan, -1);
         
         return sizeof(int);
      }

      var writeElement = SerializerRegistry<T>.GetWrite();
      var lengthSpanNotNil = writer.AcquireSpan(sizeof(int));
      BinaryPrimitives.WriteInt32LittleEndian(lengthSpanNotNil, value.Length);

      var written = sizeof(int);
      for (var i = 0; i < value.Length; i++)
      {
         written += writeElement(ref writer, value[i]);
      }

      return written;
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out ImmutableArray<T> value)
   {
      if (!reader.TryReadLittleEndian(out int length))
      {
         value = default;
         return false;
      }

      if (length < 0)
      {
         value = default;
         return true;
      }

      var tryReadElement = SerializerRegistry<T>.GetTryRead();
      var builder = ImmutableArray.CreateBuilder<T>(length);

      for (var i = 0; i < length; i++)
      {
         if (!tryReadElement(ref reader, out var element))
         {
            value = default;
            return false;
         }
         
         builder.Add(element);
      }

      value = builder.MoveToImmutable();
      return true;
   }

   public static int CalculateByteLength(scoped in ImmutableArray<T> value)
   {
      if (value.IsDefault)
      {
         return sizeof(int);
      }

      var calculateElement = SerializerRegistry<T>.GetCalculateByteLength();
      var length = sizeof(int);
      
      for (var i = 0; i < value.Length; i++)
      {
         length += calculateElement(value[i]);
      }

      return length;
   }
}

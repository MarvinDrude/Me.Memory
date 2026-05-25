using System;
using System.Buffers;
using System.Buffers.Binary;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Collections;

/// <summary>
/// Serializer for ArraySegment values.
/// </summary>
public abstract class ArraySegmentSerializer<T> : ISerializer<ArraySegment<T>>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in ArraySegment<T> value)
   {
      var writeElement = SerializerRegistry<T>.GetWrite();
      var lengthSpan = writer.AcquireSpan(sizeof(int));
      
      BinaryPrimitives.WriteInt32LittleEndian(lengthSpan, value.Count);

      var written = sizeof(int);
      for (var i = 0; i < value.Count; i++)
      {
         written += writeElement(ref writer, value[i]);
      }

      return written;
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out ArraySegment<T> value)
   {
      if (!reader.TryReadLittleEndian(out int length))
      {
         value = default;
         return false;
      }

      if (length < 0)
      {
         throw new InvalidOperationException("Serialized ArraySegment length cannot be negative.");
      }

      var tryReadElement = SerializerRegistry<T>.GetTryRead();
      var array = new T[length];

      for (var i = 0; i < length; i++)
      {
         if (!tryReadElement(ref reader, out var element))
         {
            value = default;
            return false;
         }
         
         array[i] = element;
      }

      value = new ArraySegment<T>(array);
      return true;
   }

   public static int CalculateByteLength(scoped in ArraySegment<T> value)
   {
      var calculateElement = SerializerRegistry<T>.GetCalculateByteLength();
      var length = sizeof(int);
      
      for (var i = 0; i < value.Count; i++)
      {
         length += calculateElement(value[i]);
      }

      return length;
   }
}

using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Collections;

/// <summary>
/// Serializer for List values.
/// </summary>
public abstract class ListSerializer<T> : ISerializer<List<T>?>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in List<T>? value)
   {
      if (value is null)
      {
         var lengthSpan = writer.AcquireSpan(sizeof(int));
         BinaryPrimitives.WriteInt32LittleEndian(lengthSpan, -1);
         return sizeof(int);
      }

      var writeElement = SerializerRegistry<T>.GetWrite();
      var lengthSpanNotNil = writer.AcquireSpan(sizeof(int));
      
      BinaryPrimitives.WriteInt32LittleEndian(lengthSpanNotNil, value.Count);

      var written = sizeof(int);
      for (var i = 0; i < value.Count; i++)
      {
         written += writeElement(ref writer, value[i]);
      }

      return written;
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out List<T>? value)
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

      var tryReadElement = SerializerRegistry<T>.GetTryRead();
      var list = new List<T>(length);

      for (var i = 0; i < length; i++)
      {
         if (!tryReadElement(ref reader, out var element))
         {
            value = null;
            return false;
         }
         
         list.Add(element);
      }

      value = list;
      return true;
   }

   public static int CalculateByteLength(scoped in List<T>? value)
   {
      if (value is null)
      {
         return sizeof(int);
      }

      var calculateElement = SerializerRegistry<T>.GetCalculateByteLength();
      var length = sizeof(int);
      
      for (var i = 0; i < value.Count; i++)
      {
         length += calculateElement(value[i]);
      }

      return length;
   }
}

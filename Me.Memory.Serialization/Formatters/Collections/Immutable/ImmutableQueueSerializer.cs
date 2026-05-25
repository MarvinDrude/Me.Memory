using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.Immutable;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Collections.Immutable;

/// <summary>
/// Serializer for ImmutableQueue values.
/// </summary>
public abstract class ImmutableQueueSerializer<T> : ISerializer<ImmutableQueue<T>?>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in ImmutableQueue<T>? value)
   {
      if (value is null)
      {
         var lengthSpan = writer.AcquireSpan(sizeof(int));
         BinaryPrimitives.WriteInt32LittleEndian(lengthSpan, -1);
         return sizeof(int);
      }

      var writeElement = SerializerRegistry<T>.GetWrite();
      
      // Calculate count first
      var count = 0;
      foreach (var _ in value) count++;

      var lengthSpanNotNil = writer.AcquireSpan(sizeof(int));
      BinaryPrimitives.WriteInt32LittleEndian(lengthSpanNotNil, count);

      var written = sizeof(int);
      foreach (var item in value)
      {
         written += writeElement(ref writer, item);
      }

      return written;
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out ImmutableQueue<T>? value)
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
      var array = new T[length];

      for (var i = 0; i < length; i++)
      {
         if (!tryReadElement(ref reader, out var element))
         {
            value = null;
            return false;
         }
         
         array[i] = element;
      }

      value = ImmutableQueue.CreateRange(array);
      return true;
   }

   public static int CalculateByteLength(scoped in ImmutableQueue<T>? value)
   {
      if (value is null)
      {
         return sizeof(int);
      }

      var calculateElement = SerializerRegistry<T>.GetCalculateByteLength();
      var length = sizeof(int);
      
      foreach (var item in value)
      {
         length += calculateElement(item);
      }

      return length;
   }
}

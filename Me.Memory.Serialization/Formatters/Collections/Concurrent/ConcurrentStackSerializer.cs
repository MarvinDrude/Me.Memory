using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Collections.Concurrent;

/// <summary>
/// Serializer for ConcurrentStack values.
/// </summary>
public abstract class ConcurrentStackSerializer<T> : ISerializer<ConcurrentStack<T>?>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in ConcurrentStack<T>? value)
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
      foreach (var item in value)
      {
         written += writeElement(ref writer, item);
      }

      return written;
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out ConcurrentStack<T>? value)
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

      var stack = new ConcurrentStack<T>();
      for (var i = length - 1; i >= 0; i--)
      {
         stack.Push(array[i]);
      }

      value = stack;
      return true;
   }

   public static int CalculateByteLength(scoped in ConcurrentStack<T>? value)
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

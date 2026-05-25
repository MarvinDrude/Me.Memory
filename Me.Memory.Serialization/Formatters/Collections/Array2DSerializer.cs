using System.Buffers;
using System.Buffers.Binary;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Collections;

/// <summary>
/// Serializer for 2D multi-dimensional arrays.
/// </summary>
public abstract class Array2DSerializer<T> : ISerializer<T[,]?>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in T[,]? value)
   {
      if (value is null)
      {
         var lengthSpan = writer.AcquireSpan(sizeof(int));
         BinaryPrimitives.WriteInt32LittleEndian(lengthSpan, -1);
         
         return sizeof(int);
      }

      var writeElement = SerializerRegistry<T>.GetWrite();
      var dim0 = value.GetLength(0);
      var dim1 = value.GetLength(1);

      var headerSpan = writer.AcquireSpan(sizeof(int) * 2);
      BinaryPrimitives.WriteInt32LittleEndian(headerSpan, dim0);
      BinaryPrimitives.WriteInt32LittleEndian(headerSpan[sizeof(int)..], dim1);

      var written = sizeof(int) * 2;
      for (var i = 0; i < dim0; i++)
      {
         for (var j = 0; j < dim1; j++)
         {
            written += writeElement(ref writer, value[i, j]);
         }
      }

      return written;
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out T[,]? value)
   {
      if (!reader.TryReadLittleEndian(out int dim0))
      {
         value = null;
         return false;
      }

      if (dim0 < 0)
      {
         value = null;
         return true;
      }

      if (!reader.TryReadLittleEndian(out int dim1))
      {
         value = null;
         return false;
      }

      if (dim1 < 0)
      {
         throw new InvalidOperationException("Dimension 1 length cannot be negative.");
      }

      var tryReadElement = SerializerRegistry<T>.GetTryRead();
      var array = new T[dim0, dim1];

      for (var i = 0; i < dim0; i++)
      {
         for (var j = 0; j < dim1; j++)
         {
            if (!tryReadElement(ref reader, out var element))
            {
               value = null;
               return false;
            }
            array[i, j] = element;
         }
      }

      value = array;
      return true;
   }

   public static int CalculateByteLength(scoped in T[,]? value)
   {
      if (value is null)
      {
         return sizeof(int);
      }

      var calculateElement = SerializerRegistry<T>.GetCalculateByteLength();
      var dim0 = value.GetLength(0);
      var dim1 = value.GetLength(1);

      var length = sizeof(int) * 2;
      for (var i = 0; i < dim0; i++)
      {
         for (var j = 0; j < dim1; j++)
         {
            length += calculateElement(value[i, j]);
         }
      }

      return length;
   }
}

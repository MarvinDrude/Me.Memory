using System.Buffers;
using System.Buffers.Binary;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Unmanaged;

/// <summary>
/// Serializer for double-precision floating point numbers.
/// </summary>
public abstract class DoubleSerializer : ISerializer<double>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in double value)
   {
      writer.WriteLittleEndian(value);
      return sizeof(double);
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out double value)
   {
      if (reader.UnreadSpan.Length >= sizeof(double))
      {
         value = BinaryPrimitives.ReadDoubleLittleEndian(reader.UnreadSpan);
         reader.Advance(sizeof(double));
         
         return true;
      }
      
      if (reader.TryReadLittleEndian(out long l))
      {
         value = BitConverter.Int64BitsToDouble(l);
         return true;
      }

      value = 0;
      return false;
   }

   public static int CalculateByteLength(scoped in double value)
   {
      return sizeof(double);
   }
}

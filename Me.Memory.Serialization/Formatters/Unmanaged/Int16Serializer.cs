using System.Buffers;
using System.Buffers.Binary;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Unmanaged;

/// <summary>
/// Serializer for 16-bit signed integers.
/// </summary>
public abstract class Int16Serializer : ISerializer<short>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in short value)
   {
      writer.WriteLittleEndian(value);
      return sizeof(short);
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out short value)
   {
      if (reader.UnreadSpan.Length >= sizeof(short))
      {
         value = BinaryPrimitives.ReadInt16LittleEndian(reader.UnreadSpan);
         reader.Advance(sizeof(short));
         
         return true;
      }
      
      return reader.TryReadLittleEndian(out value);
   }

   public static int CalculateByteLength(scoped in short value)
   {
      return sizeof(short);
   }
}

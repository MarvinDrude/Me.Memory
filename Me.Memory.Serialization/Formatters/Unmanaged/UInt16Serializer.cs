using System.Buffers;
using System.Buffers.Binary;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Unmanaged;

/// <summary>
/// Serializer for 16-bit unsigned integers.
/// </summary>
public sealed class UInt16Serializer : ISerializer<ushort>
{
   public int Write(ref BufferWriter<byte> writer, scoped in ushort value)
   {
      writer.WriteLittleEndian(value);
      return sizeof(ushort);
   }

   public bool TryRead(ref SequenceReader<byte> reader, out ushort value)
   {
      if (reader.UnreadSpan.Length >= sizeof(ushort))
      {
         value = BinaryPrimitives.ReadUInt16LittleEndian(reader.UnreadSpan);
         reader.Advance(sizeof(ushort));
         
         return true;
      }
      
      if (reader.TryReadLittleEndian(out short s))
      {
         value = (ushort)s;
         return true;
      }

      value = 0;
      return false;
   }

   public int CalculateByteLength(ref ushort value)
   {
      return sizeof(ushort);
   }
}

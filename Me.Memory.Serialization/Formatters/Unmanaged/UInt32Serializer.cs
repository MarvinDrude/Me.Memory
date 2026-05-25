using System.Buffers;
using System.Buffers.Binary;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Unmanaged;

/// <summary>
/// Serializer for 32-bit unsigned integers.
/// </summary>
public sealed class UInt32Serializer : ISerializer<uint>
{
   public int Write(ref BufferWriter<byte> writer, scoped in uint value)
   {
      writer.WriteLittleEndian(value);
      return sizeof(uint);
   }

   public bool TryRead(ref SequenceReader<byte> reader, out uint value)
   {
      if (reader.UnreadSpan.Length >= sizeof(uint))
      {
         value = BinaryPrimitives.ReadUInt32LittleEndian(reader.UnreadSpan);
         reader.Advance(sizeof(uint));
         
         return true;
      }
      
      if (reader.TryReadLittleEndian(out int i))
      {
         value = (uint)i;
         return true;
      }

      value = 0;
      return false;
   }

   public int CalculateByteLength(ref uint value)
   {
      return sizeof(uint);
   }
}

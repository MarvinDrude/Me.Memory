using System.Buffers;
using System.Buffers.Binary;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Unmanaged;

/// <summary>
/// Serializer for 64-bit unsigned integers.
/// </summary>
public sealed class UInt64Serializer : ISerializer<ulong>
{
   public int Write(ref BufferWriter<byte> writer, scoped in ulong value)
   {
      writer.WriteLittleEndian(value);
      return sizeof(ulong);
   }

   public bool TryRead(ref SequenceReader<byte> reader, out ulong value)
   {
      if (reader.UnreadSpan.Length >= sizeof(ulong))
      {
         value = BinaryPrimitives.ReadUInt64LittleEndian(reader.UnreadSpan);
         reader.Advance(sizeof(ulong));
         
         return true;
      }
      
      if (reader.TryReadLittleEndian(out long l))
      {
         value = (ulong)l;
         return true;
      }

      value = 0;
      return false;
   }

   public int CalculateByteLength(scoped in ulong value)
   {
      return sizeof(ulong);
   }
}

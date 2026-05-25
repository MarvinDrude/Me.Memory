using System.Buffers;
using System.Buffers.Binary;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Unmanaged;

/// <summary>
/// Serializer for 64-bit signed integers.
/// </summary>
public sealed class Int64Serializer : ISerializer<long>
{
   public int Write(ref BufferWriter<byte> writer, scoped in long value)
   {
      writer.WriteLittleEndian(value);
      return sizeof(long);
   }

   public bool TryRead(ref SequenceReader<byte> reader, out long value)
   {
      if (reader.UnreadSpan.Length >= sizeof(long))
      {
         value = BinaryPrimitives.ReadInt64LittleEndian(reader.UnreadSpan);
         reader.Advance(sizeof(long));
         
         return true;
      }
      
      return reader.TryReadLittleEndian(out value);
   }

   public int CalculateByteLength(scoped in long value)
   {
      return sizeof(long);
   }
}

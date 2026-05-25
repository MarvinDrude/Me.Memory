using System.Buffers;
using System.Buffers.Binary;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Unmanaged;

/// <summary>
/// Serializer for decimal numbers.
/// </summary>
public sealed class DecimalSerializer : ISerializer<decimal>
{
   public int Write(ref BufferWriter<byte> writer, scoped in decimal value)
   {
      var span = writer.AcquireSpan(sizeof(decimal));
      
      Span<int> bits = stackalloc int[4];
      decimal.GetBits(value, bits);
      
      BinaryPrimitives.WriteInt32LittleEndian(span, bits[0]);
      BinaryPrimitives.WriteInt32LittleEndian(span[4..], bits[1]);
      BinaryPrimitives.WriteInt32LittleEndian(span[8..], bits[2]);
      BinaryPrimitives.WriteInt32LittleEndian(span[12..], bits[3]);
      
      return sizeof(decimal);
   }

   public bool TryRead(ref SequenceReader<byte> reader, out decimal value)
   {
      if (reader.UnreadSpan.Length >= sizeof(decimal))
      {
         Span<int> bits = stackalloc int[4];
         
         bits[0] = BinaryPrimitives.ReadInt32LittleEndian(reader.UnreadSpan);
         bits[1] = BinaryPrimitives.ReadInt32LittleEndian(reader.UnreadSpan[4..]);
         bits[2] = BinaryPrimitives.ReadInt32LittleEndian(reader.UnreadSpan[8..]);
         bits[3] = BinaryPrimitives.ReadInt32LittleEndian(reader.UnreadSpan[12..]);
         
         value = new decimal(bits);
         reader.Advance(sizeof(decimal));
         
         return true;
      }
      
      if (reader.TryReadLittleEndian(out int i1) &&
          reader.TryReadLittleEndian(out int i2) &&
          reader.TryReadLittleEndian(out int i3) &&
          reader.TryReadLittleEndian(out int i4))
      {
         Span<int> bits = [i1, i2, i3, i4];
         value = new decimal(bits);
         
         return true;
      }

      value = 0;
      return false;
   }

   public int CalculateByteLength(scoped in decimal value)
   {
      return sizeof(decimal);
   }
}

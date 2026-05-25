using System.Buffers;
using System.Buffers.Binary;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.System;

/// <summary>
/// Serializer for DateOnly values.
/// </summary>
public sealed class DateOnlySerializer : ISerializer<DateOnly>
{
   public int Write(ref BufferWriter<byte> writer, scoped in DateOnly value)
   {
      writer.WriteLittleEndian(value.DayNumber);
      return sizeof(int);
   }

   public bool TryRead(ref SequenceReader<byte> reader, out DateOnly value)
   {
      if (reader.UnreadSpan.Length >= sizeof(int))
      {
         var dayNumber = BinaryPrimitives.ReadInt32LittleEndian(reader.UnreadSpan);
         reader.Advance(sizeof(int));
         
         value = DateOnly.FromDayNumber(dayNumber);
         return true;
      }
      
      if (reader.TryReadLittleEndian(out int dn))
      {
         value = DateOnly.FromDayNumber(dn);
         return true;
      }

      value = default;
      return false;
   }

   public int CalculateByteLength(scoped in DateOnly value)
   {
      return sizeof(int);
   }
}

using System.Buffers;
using System.Buffers.Binary;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.System;

/// <summary>
/// Serializer for TimeOnly values.
/// </summary>
public sealed class TimeOnlySerializer : ISerializer<TimeOnly>
{
   public int Write(ref BufferWriter<byte> writer, scoped in TimeOnly value)
   {
      writer.WriteLittleEndian(value.Ticks);
      return sizeof(long);
   }

   public bool TryRead(ref SequenceReader<byte> reader, out TimeOnly value)
   {
      if (reader.UnreadSpan.Length >= sizeof(long))
      {
         var ticks = BinaryPrimitives.ReadInt64LittleEndian(reader.UnreadSpan);
         reader.Advance(sizeof(long));
         value = new TimeOnly(ticks);
         
         return true;
      }
      
      if (reader.TryReadLittleEndian(out long t))
      {
         value = new TimeOnly(t);
         return true;
      }

      value = default;
      return false;
   }

   public int CalculateByteLength(scoped in TimeOnly value)
   {
      return sizeof(long);
   }
}

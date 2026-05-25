using System.Buffers;
using System.Buffers.Binary;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.System;

/// <summary>
/// Serializer for DateTimeOffset values.
/// </summary>
public abstract class DateTimeOffsetSerializer : ISerializer<DateTimeOffset>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in DateTimeOffset value)
   {
      var span = writer.AcquireSpan(sizeof(long) * 2);
      BinaryPrimitives.WriteInt64LittleEndian(span, value.Ticks);
      BinaryPrimitives.WriteInt64LittleEndian(span[sizeof(long)..], value.Offset.Ticks);
      
      return sizeof(long) * 2;
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out DateTimeOffset value)
   {
      if (reader.UnreadSpan.Length >= sizeof(long) * 2)
      {
         var ticks = BinaryPrimitives.ReadInt64LittleEndian(reader.UnreadSpan);
         var offsetTicks = BinaryPrimitives.ReadInt64LittleEndian(reader.UnreadSpan[sizeof(long)..]);
         reader.Advance(sizeof(long) * 2);
         value = new DateTimeOffset(ticks, new TimeSpan(offsetTicks));
         
         return true;
      }
      
      if (reader.TryReadLittleEndian(out long t) &&
          reader.TryReadLittleEndian(out long ot))
      {
         value = new DateTimeOffset(t, new TimeSpan(ot));
         return true;
      }

      value = default;
      return false;
   }

   public static int CalculateByteLength(scoped in DateTimeOffset value)
   {
      return sizeof(long) * 2;
   }
}

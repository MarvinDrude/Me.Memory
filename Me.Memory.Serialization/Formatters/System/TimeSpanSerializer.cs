using System.Buffers;
using System.Buffers.Binary;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.System;

/// <summary>
/// Serializer for TimeSpan values.
/// </summary>
public abstract class TimeSpanSerializer : ISerializer<TimeSpan>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in TimeSpan value)
   {
      writer.WriteLittleEndian(value.Ticks);
      return sizeof(long);
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out TimeSpan value)
   {
      if (reader.UnreadSpan.Length >= sizeof(long))
      {
         var ticks = BinaryPrimitives.ReadInt64LittleEndian(reader.UnreadSpan);
         reader.Advance(sizeof(long));
         value = new TimeSpan(ticks);
         
         return true;
      }
      
      if (reader.TryReadLittleEndian(out long t))
      {
         value = new TimeSpan(t);
         return true;
      }

      value = default;
      return false;
   }

   public static int CalculateByteLength(scoped in TimeSpan value)
   {
      return sizeof(long);
   }
}

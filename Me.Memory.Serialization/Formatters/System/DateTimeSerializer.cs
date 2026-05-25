using System.Buffers;
using System.Buffers.Binary;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.System;

/// <summary>
/// Serializer for DateTime values.
/// </summary>
public sealed class DateTimeSerializer : ISerializer<DateTime>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in DateTime value)
   {
      writer.WriteLittleEndian(value.ToBinary());
      return sizeof(long);
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out DateTime value)
   {
      if (reader.UnreadSpan.Length >= sizeof(long))
      {
         var binary = BinaryPrimitives.ReadInt64LittleEndian(reader.UnreadSpan);
         reader.Advance(sizeof(long));
         value = DateTime.FromBinary(binary);
         
         return true;
      }
      
      if (reader.TryReadLittleEndian(out long b))
      {
         value = DateTime.FromBinary(b);
         return true;
      }

      value = default;
      return false;
   }

   public static int CalculateByteLength(scoped in DateTime value)
   {
      return sizeof(long);
   }
}

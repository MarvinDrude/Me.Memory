using System.Buffers;
using System.Buffers.Binary;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Unmanaged;

/// <summary>
/// Serializer for 32-bit integers.
/// </summary>
public sealed class Int32Serializer : ISerializer<int>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in int value)
   {
      writer.WriteLittleEndian(value);
      return sizeof(int);
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out int value)
   {
      if (reader.UnreadSpan.Length >= sizeof(int))
      {
         value = BinaryPrimitives.ReadInt32LittleEndian(reader.UnreadSpan);
         reader.Advance(sizeof(int));
         
         return true;
      }
      
      return reader.TryReadLittleEndian(out value);
   }

   public static int CalculateByteLength(scoped in int value)
   {
      return sizeof(int);
   }
}
using System.Buffers;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Unmanaged;

/// <summary>
/// Serializer for bytes.
/// </summary>
public abstract class ByteSerializer : ISerializer<byte>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in byte value)
   {
      writer.WriteLittleEndian(value);
      return sizeof(byte);
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out byte value)
   {
      if (reader.UnreadSpan.Length >= sizeof(byte))
      {
         value = reader.UnreadSpan[0];
         reader.Advance(sizeof(byte));
         
         return true;
      }
      
      return reader.TryRead(out value);
   }

   public static int CalculateByteLength(scoped in byte value)
   {
      return sizeof(byte);
   }
}

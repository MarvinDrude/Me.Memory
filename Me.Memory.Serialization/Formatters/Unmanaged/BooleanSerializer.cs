using System.Buffers;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Unmanaged;

/// <summary>
/// Serializer for booleans.
/// </summary>
public sealed class BooleanSerializer : ISerializer<bool>
{
   public int Write(ref BufferWriter<byte> writer, scoped in bool value)
   {
      writer.WriteLittleEndian((byte)(value ? 1 : 0));
      return sizeof(bool);
   }

   public bool TryRead(ref SequenceReader<byte> reader, out bool value)
   {
      if (reader.UnreadSpan.Length >= sizeof(byte))
      {
         value = reader.UnreadSpan[0] != 0;
         reader.Advance(sizeof(byte));
         
         return true;
      }
      
      if (reader.TryRead(out var b))
      {
         value = b != 0;
         return true;
      }

      value = false;
      return false;
   }

   public int CalculateByteLength(scoped in bool value)
   {
      return sizeof(byte);
   }
}

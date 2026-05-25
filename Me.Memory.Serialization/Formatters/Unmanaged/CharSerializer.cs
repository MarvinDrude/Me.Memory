using System.Buffers;
using System.Buffers.Binary;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Unmanaged;

/// <summary>
/// Serializer for characters.
/// </summary>
public sealed class CharSerializer : ISerializer<char>
{
   public int Write(ref BufferWriter<byte> writer, scoped in char value)
   {
      writer.WriteLittleEndian(value);
      return sizeof(char);
   }

   public bool TryRead(ref SequenceReader<byte> reader, out char value)
   {
      if (reader.UnreadSpan.Length >= sizeof(char))
      {
         value = (char)BinaryPrimitives.ReadUInt16LittleEndian(reader.UnreadSpan);
         reader.Advance(sizeof(char));
         
         return true;
      }
      
      if (reader.TryReadLittleEndian(out short s))
      {
         value = (char)s;
         return true;
      }

      value = '\0';
      return false;
   }

   public int CalculateByteLength(scoped in char value)
   {
      return sizeof(char);
   }
}

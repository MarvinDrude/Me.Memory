using System.Buffers;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Unmanaged;

/// <summary>
/// Serializer for signed bytes.
/// </summary>
public sealed class SByteSerializer : ISerializer<sbyte>
{
   public int Write(ref BufferWriter<byte> writer, scoped in sbyte value)
   {
      writer.WriteLittleEndian(value);
      return sizeof(sbyte);
   }

   public bool TryRead(ref SequenceReader<byte> reader, out sbyte value)
   {
      if (reader.UnreadSpan.Length >= sizeof(sbyte))
      {
         value = (sbyte)reader.UnreadSpan[0];
         reader.Advance(sizeof(sbyte));
         
         return true;
      }
      
      if (reader.TryRead(out var b))
      {
         value = (sbyte)b;
         return true;
      }

      value = 0;
      return false;
   }

   public int CalculateByteLength(scoped in sbyte value)
   {
      return sizeof(sbyte);
   }
}

using System.Buffers;
using System.Buffers.Binary;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Unmanaged;

/// <summary>
/// Serializer for single-precision floating point numbers.
/// </summary>
public sealed class SingleSerializer : ISerializer<float>
{
   public int Write(ref BufferWriter<byte> writer, scoped in float value)
   {
      writer.WriteLittleEndian(value);
      return sizeof(float);
   }

   public bool TryRead(ref SequenceReader<byte> reader, out float value)
   {
      if (reader.UnreadSpan.Length >= sizeof(float))
      {
         value = BinaryPrimitives.ReadSingleLittleEndian(reader.UnreadSpan);
         reader.Advance(sizeof(float));
         
         return true;
      }
      
      if (reader.TryReadLittleEndian(out int i))
      {
         value = BitConverter.Int32BitsToSingle(i);
         return true;
      }

      value = 0f;
      return false;
   }

   public int CalculateByteLength(ref float value)
   {
      return sizeof(float);
   }
}

using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using Me.Memory.Buffers;
using Me.Memory.Extensions;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Unmanaged;

/// <summary>
/// Serializer for half-precision floating point numbers.
/// </summary>
public abstract class HalfSerializer : ISerializer<Half>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in Half value)
   {
      writer.WriteLittleEndian(value);
      return Unsafe.SizeOf<Half>();
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out Half value)
   {
      if (reader.UnreadSpan.Length >= Unsafe.SizeOf<Half>())
      {
         value = BinaryPrimitives.ReadHalfLittleEndian(reader.UnreadSpan);
         reader.Advance(Unsafe.SizeOf<Half>());
         
         return true;
      }
      
      if (reader.TryReadLittleEndian(out short s))
      {
         value = BitConverter.Int16BitsToHalf(s);
         return true;
      }

      value = default;
      return false;
   }

   public static int CalculateByteLength(scoped in Half value)
   {
      return Unsafe.SizeOf<Half>();
   }
}

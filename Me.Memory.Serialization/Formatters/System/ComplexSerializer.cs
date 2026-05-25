using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.System;

/// <summary>
/// Serializer for Complex values.
/// </summary>
public abstract class ComplexSerializer : ISerializer<Complex>
{
   private const int Size = sizeof(double) * 2;

   public static int Write(ref BufferWriter<byte> writer, scoped in Complex value)
   {
      var span = writer.AcquireSpan(Size);
      
      BinaryPrimitives.WriteDoubleLittleEndian(span, value.Real);
      BinaryPrimitives.WriteDoubleLittleEndian(span[sizeof(double)..], value.Imaginary);
      
      return Size;
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out Complex value)
   {
      if (reader.UnreadSpan.Length >= Size)
      {
         var real = BinaryPrimitives.ReadDoubleLittleEndian(reader.UnreadSpan);
         var imaginary = BinaryPrimitives.ReadDoubleLittleEndian(reader.UnreadSpan[sizeof(double)..]);
         
         value = new Complex(real, imaginary);
         reader.Advance(Size);
         
         return true;
      }

      if (reader.Remaining >= Size)
      {
         Span<byte> bytes = stackalloc byte[Size];
         reader.UnreadSequence.Slice(0, Size).CopyTo(bytes);
         
         var real = BinaryPrimitives.ReadDoubleLittleEndian(bytes);
         var imaginary = BinaryPrimitives.ReadDoubleLittleEndian(bytes[sizeof(double)..]);
         
         value = new Complex(real, imaginary);
         reader.Advance(Size);
         
         return true;
      }

      value = default;
      return false;
   }

   public static int CalculateByteLength(scoped in Complex value)
   {
      return Size;
   }
}

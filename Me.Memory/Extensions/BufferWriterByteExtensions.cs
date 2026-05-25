using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Me.Memory.Buffers;

namespace Me.Memory.Extensions;

public static class BufferWriterByteExtensions
{
   extension(BufferWriter<byte> writer)
   {
      public T ReadBigEndian<T>(out int read, bool movePosition = false)
         where T : unmanaged
      {
         var size = Unsafe.SizeOf<T>();
         return writer.AcquireSpan(size, movePosition)
            .ReadBigEndian<T>(out read);
      }
      
      public T ReadLittleEndian<T>(out int read, bool movePosition = false)
         where T : unmanaged
      {
         var size = Unsafe.SizeOf<T>();
         return writer.AcquireSpan(size, movePosition)
            .ReadLittleEndian<T>(out read);
      }

      public int WriteLittleEndian<T>(scoped in T value)
         where T : unmanaged
      {
         var size = Unsafe.SizeOf<T>();
         var span = writer.AcquireSpan(size);
         
         if (BitConverter.IsLittleEndian)
         {
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), value);
         }
         else
         {
            value.WriteLittleEndian(span);
         }
         
         return size;
      }
   }
}
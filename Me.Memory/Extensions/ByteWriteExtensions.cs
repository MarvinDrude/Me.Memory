using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Me.Memory.Extensions;

public static class ByteWriteExtensions
{
   extension<T>(T value)
      where T : unmanaged
   {
      public int WriteBigEndian(scoped Span<byte> buffer)
      {
         var size = Unsafe.SizeOf<T>();
         buffer = buffer[..size];
         
         MemoryMarshal.Write(buffer, in value);
         if (BitConverter.IsLittleEndian) buffer.Reverse();
         
         return size;
      }

      public int WriteLittleEndian(scoped Span<byte> buffer)
      {
         var size = Unsafe.SizeOf<T>();
         buffer = buffer[..size];
         
         MemoryMarshal.Write(buffer, in value);
         if (!BitConverter.IsLittleEndian) buffer.Reverse();

         return size;
      }
   }
}
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Me.Memory.Extensions;

public static class SpanByteReadExtensions
{
   extension(scoped ReadOnlySpan<byte> buffer)
   {
      public T ReadBigEndian<T>(out int read)
         where T : unmanaged
      {
         var size = Unsafe.SizeOf<T>();
         buffer = buffer[..size];
         read = size;

         if (!BitConverter.IsLittleEndian)
         {
            return MemoryMarshal.Read<T>(buffer);
         }

         Span<byte> temp = stackalloc byte[size];
         for (var e = 0; e < size; e++)
         {
            temp[e] = buffer[size - 1 - e];
         }
         
         return MemoryMarshal.Read<T>(temp);
      }
   }
}
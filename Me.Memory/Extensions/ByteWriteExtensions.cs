using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Me.Memory.Buffers;

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

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public int WriteBigEndian(ref BufferWriter<byte> buffer, bool movePosition = true)
      {
         var size = Unsafe.SizeOf<T>();
         return value.WriteBigEndian(
            buffer.AcquireSpan(size, movePosition));
      }

      public int WriteLittleEndian(scoped Span<byte> buffer)
      {
         var size = Unsafe.SizeOf<T>();
         buffer = buffer[..size];
         
         MemoryMarshal.Write(buffer, in value);
         if (!BitConverter.IsLittleEndian) buffer.Reverse();

         return size;
      }
      
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public int WriteLittleEndian(ref BufferWriter<byte> buffer, bool movePosition = true)
      {
         var size = Unsafe.SizeOf<T>();
         return value.WriteLittleEndian(
            buffer.AcquireSpan(size, movePosition));
      }
   }
}
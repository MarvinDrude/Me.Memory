using System.Runtime.CompilerServices;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Common;

public sealed class UnmanagedSerializer<T> : ISerializer<T>
   where T : unmanaged
{
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Write(ref ByteWriter writer, ref T value)
   {
      writer.WriteLittleEndian(value);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public bool TryRead(ref ByteReader reader, out T value)
   {
      value = reader.ReadLittleEndian<T>();
      return true;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public int CalculateByteLength(ref T value)
   {
      return Unsafe.SizeOf<T>();
   }
}
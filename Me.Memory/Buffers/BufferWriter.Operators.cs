using System.Runtime.CompilerServices;

namespace Me.Memory.Buffers;

public ref partial struct BufferWriter<T>
{
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void operator += (T element) => Add(element);
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void operator += (ReadOnlySpan<T> span) => Write(span);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void operator <<= (T element) => Add(element);
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void operator <<= (ReadOnlySpan<T> span) => Write(span);
}
using System.Runtime.CompilerServices;

namespace Me.Memory.Buffers;

public ref partial struct TextWriterIndentSlim
{
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void operator += (char element) => _buffer.Add(element);
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void operator += (ReadOnlySpan<char> span) => Write(span);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void operator <<= (char element) => _buffer.Add(element);
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void operator <<= (ReadOnlySpan<char> span) => Write(span);
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void operator |= (ReadOnlySpan<char> span) => WriteLine(span);
}
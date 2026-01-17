using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Me.Memory.Buffers;

[StructLayout(LayoutKind.Auto)]
public ref struct SpanOwner<T> : IDisposable
{
   public int Length
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get;
      private set;
   }

   public Span<T> Span
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get;
   }

   private readonly T[]? _buffer;

   public SpanOwner(int minSize, bool clearArray = true)
   {
      _buffer = ArrayPool<T>.Shared.Rent(minSize);
      if (clearArray) _buffer.AsSpan().Clear();
      
      Span = _buffer.AsSpan(0, minSize);
      Length = minSize;
   }

   public SpanOwner(Span<T> span)
   {
      Span = span;
      Length = span.Length;
   }

   public void Dispose()
   {
      if (_buffer is not null)
      {
         ArrayPool<T>.Shared.Return(_buffer);
      }
      
      Length = 0;
   }
}
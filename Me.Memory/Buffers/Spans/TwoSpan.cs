using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Me.Memory.Buffers.Spans;

[StructLayout(LayoutKind.Auto)]
public readonly ref struct TwoSpan<T>
{
   public int Length => _first.Length + _second.Length;
   
   public ref T this[long index]
   {
      get
      {
         if (index < _first.Length) return ref _first[(int)index];
         
         var secondIndex = (int)(index - _first.Length);
         return ref _second[secondIndex];
      }
   }
   
   internal Span<T> First => _first;
   internal Span<T> Second => _second;
   
   private readonly Span<T> _first;
   private readonly Span<T> _second;

   public TwoSpan(Span<T> first, Span<T> second)
   {
      _first = first;
      _second = second;
   }
   
   public Enumerator GetEnumerator() => new (_first, _second);

   public ref struct Enumerator
   {
      private readonly Span<T> _first;
      private readonly Span<T> _second;
      private long _index;

      public Enumerator(Span<T> first, Span<T> second)
      {
         _first = first;
         _second = second;
         _index = -1;
      }

      public bool MoveNext()
      {
         _index++;
         return _index < _first.Length + _second.Length;
      }

      public ref T Current
      {
         [MethodImpl(MethodImplOptions.AggressiveInlining)]
         get
         {
            if (_index < _first.Length) return ref _first[(int)_index];
         
            var secondIndex = (int)(_index - _first.Length);
            return ref _second[secondIndex];
         }
      }
   }
}
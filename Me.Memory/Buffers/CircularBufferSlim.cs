using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Me.Memory.Buffers.Spans;

namespace Me.Memory.Buffers;

[StructLayout(LayoutKind.Auto)]
public ref struct CircularBufferSlim<T>
{
   public Span<T> Buffer
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _buffer;
   }

   /// <summary>
   /// TwoSpan since there can be a wrap around case
   /// </summary>
   public TwoSpan<T> WrittenTwoSpan
   {
      get
      {
         if (Count == 0) return new TwoSpan<T>([], []);

         var end = (_start + Count) % Capacity;
         if (_start < end)
         {
            return new TwoSpan<T>(_buffer.Slice(_start, Count), []);
         }

         var first = _buffer[_start..];
         var second = _buffer[..end];
         
         return new TwoSpan<T>(first, second);
      }
   }

   public ref T this[int index]
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => ref _buffer[(_start + index) % Capacity];
   }
   
   public int Capacity => _buffer.Length;
   public int Count { get; private set; }

   private readonly Span<T> _buffer;
   private int _start;

   public CircularBufferSlim(Span<T> buffer)
   {
      _buffer = buffer;
      _start = 0;
      Count = 0;
   }

   public void Add(T item)
   {
      var index = (_start + Count) % Capacity;
      _buffer[index] = item;

      if (Count == Capacity)
      {
         _start = (_start + 1) % Capacity;
      }
      else
      {
         Count++;
      }
   }

   public Enumerator GetEnumerator() => new(this);

   public ref struct Enumerator
   {
      private readonly Span<T> _buffer;
      private readonly int _start;
      private readonly int _count;
      private readonly int _capacity;

      private int _index;

      public Enumerator(scoped in CircularBufferSlim<T> buffer)
      {
         _buffer = buffer.Buffer;
         _start = buffer._start;
         _count = buffer.Count;
         _capacity = buffer.Capacity;

         _index = -1;
      }

      public bool MoveNext()
      {
         _index++;
         return _index < _count;
      }

      public ref T Current
      {
         [MethodImpl(MethodImplOptions.AggressiveInlining)]
         get
         {
            var physical = (_start + _index) % _capacity;
            return ref _buffer[physical];
         }
      }
   }
}
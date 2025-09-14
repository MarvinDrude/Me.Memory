using System.Runtime.CompilerServices;
using Me.Memory.Buffers.Spans;

namespace Me.Memory.Buffers;

public sealed class CircularBuffer<T> : IDisposable
{
   public Span<T> Buffer
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _memoryOwner.Span;
   }

   public TwoSpan<T> WrittenTwoSpan
   {
      get
      {
         if (Count == 0) return new TwoSpan<T>([], []);
         
         var end = (_start + Count) % Capacity;
         if (_start < end)
         {
            return new TwoSpan<T>(Buffer.Slice(_start, Count), []);
         }

         var first = Buffer[_start..];
         var second = Buffer[..end];
         
         return new TwoSpan<T>(first, second);
      }
   }

   public ref T this[int index]
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => ref Buffer[(_start + index) % Capacity];
   }

   public int Capacity => Buffer.Length;
   public int Count { get; private set; }
   
   private MemoryOwner<T> _memoryOwner;
   private bool _disposed;
   
   private int _start;
   
   public CircularBuffer(int capacity)
   {
      _memoryOwner = new MemoryOwner<T>(capacity);
   }

   public void Add(T item)
   {
      var index = (_start + Count) % Capacity;
      Buffer[index] = item;

      if (Count == Capacity)
      {
         _start = (_start + 1) % Capacity;
      }
      else
      {
         Count++;
      }
   }

   public void Clear()
   {
      Buffer.Clear();
      
      Count = 0;
      _start = 0;
   }
   
   public Enumerator GetEnumerator() => new(this);
   
   public void Dispose()
   {
      if (_disposed) return;
      _disposed = true;

      _memoryOwner.Dispose();
   }

   public ref struct Enumerator
   {
      private readonly Span<T> _buffer;
      private readonly int _start;
      private readonly int _count;
      private readonly int _capacity;

      private int _index;
      
      public Enumerator(CircularBuffer<T> buffer)
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
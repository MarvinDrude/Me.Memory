using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Me.Memory.Buffers;

[StructLayout(LayoutKind.Auto)]
public ref struct BufferWriter<T> : IDisposable
{
   public readonly int Capacity
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _span.Length;
   }

   public readonly int FreeCapacity
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _span.Length - _position;
   }

   public readonly ReadOnlySpan<T> WrittenSpan
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _span[.._position];
   }
   
   public int Position
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      readonly get => _position;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set
      {
         if (value > Capacity || value < 0)
         {
            throw new ArgumentOutOfRangeException(nameof(Position));
         }
         _position = value;
      }
   }
   
   private SpanOwner<T> _initalSpanOwner;
   private MemoryOwner<T> _memoryOwner;

   private Span<T> _span;

   private bool _isGrown = false;
   private bool _isDisposed = false;
   
   private readonly int _initialMinGrowCapacity;
   private int _position = 0;
   
   public BufferWriter(
      Span<T> startBuffer, 
      int initalMinGrowCapacity = -1)
   {
      _initalSpanOwner = new SpanOwner<T>(startBuffer);
      _memoryOwner = default;
      
      _initialMinGrowCapacity = initalMinGrowCapacity;
      _span = startBuffer;
   }

   public BufferWriter(
      int minSize,
      int initialMinGrowCapacity = -1)
   {
      _memoryOwner = new MemoryOwner<T>(minSize);
      _initalSpanOwner = default;
      
      _isGrown = true;
      _initialMinGrowCapacity = initialMinGrowCapacity;
      _span = _memoryOwner.Span;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Fill(T value)
   {
      _span.Fill(value);
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Add(ref T reference)
   {
      if (FreeCapacity < 1)
      {
         Resize(1);
      }

      ref var refr = ref MemoryMarshal.GetReference(_span);
      Unsafe.Add(ref refr, _position++) = reference;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Add(T value)
   {
      if (FreeCapacity < 1)
      {
         Resize(1);
      }

      ref var reference = ref MemoryMarshal.GetReference(_span);
      Unsafe.Add(ref reference, _position++) = value;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Write(scoped ReadOnlySpan<T> span)
   {
      if (FreeCapacity < span.Length)
      {
         Resize(span.Length - FreeCapacity);
      }
      
      if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
      {
         span.CopyTo(_span[_position..]);
      }
      else
      {
         ref var srcBase = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span));
         ref var destBase = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(_span));

         var sizeOf = Unsafe.SizeOf<T>();
         var byteCount = (uint)(span.Length * sizeOf);
         
         // slightly faster than _span[_position..] + CopyTo 
         Unsafe.CopyBlockUnaligned(
            ref Unsafe.AddByteOffset(ref destBase, (nint)(_position * sizeOf)),
            ref srcBase,
            byteCount);
      }
      
      _position += span.Length;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Span<T> AcquireSpan(int length, bool movePosition = true)
   {
      var start = _position;

      if (FreeCapacity < length)
      {
         Resize(length - FreeCapacity);
      }

      if (movePosition)
      {
         _position += length;
      }

      return _span.Slice(start, length);
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Advance(int count)
   {
      if (FreeCapacity < count)
      {
         Resize(count - FreeCapacity);
      }
      
      _position += count;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void AdvanceTo(int position)
   {
      if (Capacity < position)
      {
         Resize(position - Capacity);
      }
      
      _position = position;
   }

   public void Move(
      int fromStart, int fromSize, 
      int toStart, bool movePosition = true)
   {
      if (fromSize == 0 || fromStart == toStart)
      {
         if (toStart > Capacity)
         {
            Resize(toStart - Capacity);
         }

         return;
      }

      var oldPosition = _position;
      var newPosition = toStart + fromSize;

      if (newPosition > Capacity)
      {
         Resize(newPosition - Capacity);
      }
      
      _span.Slice(fromStart, fromSize)
         .CopyTo(_span.Slice(toStart, fromSize));
      _position = movePosition ? newPosition : oldPosition;
   }

   private void Resize(int requestedSize)
   {
      int newSize;
      if (!_isGrown && _initialMinGrowCapacity >= requestedSize)
      {
         var newSizeLong = (long)_initalSpanOwner.Length + _initialMinGrowCapacity;
         newSize = (int)Math.Min(newSizeLong, int.MaxValue - 1);
      }
      else
      {
         var growBy = _memoryOwner.Length > 0 
            ? Math.Max(requestedSize, _memoryOwner.Length) : 256;
         var newSizeLong = (long)_memoryOwner.Length + growBy;
         
         newSize = (int)Math.Min(newSizeLong, int.MaxValue - 1);
      }
      
      if (!_isGrown)
      {
         _memoryOwner = new MemoryOwner<T>(newSize);
         WrittenSpan.CopyTo(_memoryOwner.Span);
         
         _initalSpanOwner.Dispose(); // does nothing usually
         _initalSpanOwner = default;

         _span = _memoryOwner.Span;
         _isGrown = true;
         return;
      }

      var oldOwner = _memoryOwner;
      
      _memoryOwner = new MemoryOwner<T>(newSize);
      WrittenSpan.CopyTo(_memoryOwner.Span);

      oldOwner.Dispose();
      _span = _memoryOwner.Span;
   }
   
   public void Dispose()
   {
      if (_isDisposed) return;
      _isDisposed = true;
      
      if (_isGrown)
      {
         _memoryOwner.Dispose();
      }
      else
      {
         _initalSpanOwner.Dispose();
      }
   }
}
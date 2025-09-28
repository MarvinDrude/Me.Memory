using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Me.Memory.Buffers;

/// <summary>
/// Represents a lightweight, high-performance structure for managing and
/// writing to a buffer of items of type <typeparamref name="T"/>.
/// The underlying buffer can be resized as needed automatically and is backed by a <see cref="MemoryOwner{T}"/>.
/// </summary>
/// <typeparam name="T">The type of items stored in the buffer.</typeparam>
/// <remarks>
/// The <see cref="BufferWriter{T}"/> is designed to efficiently write and manage memory using a backing <see cref="Span{T}"/>.
/// It provides utilities for managing capacity, advancing the writing position, filling values, and adding elements,
/// while minimizing memory allocations and ensuring optimal performance.
/// This structure supports scenarios where low-level buffer management and precise control over memory are required.
/// It is geared towards scenarios requiring high performance, such as streaming, serialization, and manual memory handling.
/// </remarks>
/// <threadsafety>
/// Not thread-safe. Concurrent access to the same instance must be synchronized externally.
/// </threadsafety>
/// <disposable>
/// The structure is disposable and ensures the proper release of resources when used in scenarios involving allocations.
/// </disposable>
[StructLayout(LayoutKind.Auto)]
public ref partial struct BufferWriter<T> : IDisposable
{
   /// <summary>
   /// Gets the total capacity of the buffer.
   /// </summary>
   /// <remarks>
   /// This property indicates the maximum number of elements the buffer can hold.
   /// It corresponds to the length of the underlying span used by the <see cref="BufferWriter{T}"/>.
   /// </remarks>
   public readonly int Capacity
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _span.Length;
   }

   /// <summary>
   /// Gets the remaining capacity of the buffer.
   /// </summary>
   /// <remarks>
   /// This property represents the number of elements that can still be added
   /// to the buffer before it needs to resize or expand. It is calculated as
   /// the difference between the total capacity and the current position.
   /// </remarks>
   public readonly int FreeCapacity
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _span.Length - _position;
   }
   
   /// <summary>
   /// Gets a span that represents the portion of the buffer that has been written to.
   /// </summary>
   /// <remarks>
   /// This property provides a read-only span of elements that have been added to the buffer.
   /// The span starts at the beginning of the buffer and extends for the number of elements written,
   /// excluding any unused capacity of the buffer.
   /// </remarks>
   public readonly ReadOnlySpan<T> WrittenSpan
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _span[.._position];
   }
   
   /// <summary>
   /// Gets or sets the current position within the buffer.
   /// </summary>
   /// <remarks>
   /// The <see cref="Position"/> property represents the current index in the buffer where the next write operation will occur.
   /// Assigning a value to this property updates the write position and must be within the valid range of the buffer (0 to <see cref="Capacity"/>).
   /// An <see cref="ArgumentOutOfRangeException"/> is thrown if the assigned value is outside the allowable range.
   /// </remarks>
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
   
   /// <summary>
   /// Represents the initial span ownership utilized by the <see cref="BufferWriter{T}"/>.
   /// </summary>
   /// <remarks>
   /// This field stores a <see cref="SpanOwner{T}"/> that provides ownership over the initial buffer used by the writer.
   /// It is primarily used for managing and disposing the memory associated with the initial buffer
   /// and is automatically replaced during resizing operations if the buffer grows beyond its capacity.
   /// </remarks>
   private SpanOwner<T> _initalSpanOwner;

   /// <summary>
   /// Represents the underlying memory owner responsible for managing the memory buffer utilized by the writer.
   /// </summary>
   /// <remarks>
   /// This field holds an instance of <see cref="MemoryOwner{T}"/>, which governs the lifecycle of the memory.
   /// It is used to allocate, resize, and release memory as required by the <see cref="BufferWriter{T}"/> operations.
   /// </remarks>
   private MemoryOwner<T> _memoryOwner;

   /// <summary>
   /// Represents the underlying writable span of elements used by the <see cref="BufferWriter{T}"/>.
   /// </summary>
   /// <remarks>
   /// This field serves as the primary buffer for writing operations, allowing direct element access and manipulation.
   /// It forms the foundation for buffer-related operations such as writing, advancing, and span acquisition.
   /// </remarks>
   private Span<T> _span;
   
   /// <summary>
   /// Indicates whether the underlying buffer has grown beyond its initial allocation.
   /// </summary>
   /// <remarks>
   /// This field is used to track whether the buffer has been resized to accommodate additional data.
   /// It is initially set to <c>false</c> and changes to <c>true</c> when the buffer grows beyond its starting size or capacity.
   /// </remarks>
   private bool _isGrown = false;

   /// <summary>
   /// Indicates whether the <see cref="BufferWriter{T}"/> has been disposed.
   /// </summary>
   /// <remarks>
   /// This flag is used to track if the resources of the <see cref="BufferWriter{T}"/>
   /// have been released to prevent invalid operations after disposal.
   /// </remarks>
   private bool _isDisposed = false;

   /// <summary>
   /// Represents the initial minimum growth capacity for the buffer when it needs to expand.
   /// </summary>
   /// <remarks>
   /// This field is used to determine the smallest incremental increase in size for the buffer when resized.
   /// It is initialized during the creation of the buffer writer and influences the behavior of memory expansion,
   /// especially when the buffer has not yet been grown.
   /// </remarks>
   private readonly int _initialMinGrowCapacity;

   /// <summary>
   /// Represents the current position within the buffer.
   /// </summary>
   /// <remarks>
   /// This field tracks the index in the buffer where the next write operation will occur.
   /// Its value is updated as data is added to the buffer.
   /// </remarks>
   private int _position = 0;

   /// <summary>
   /// A structure for managing and writing to a buffer with expandable capacity.
   /// </summary>
   /// <typeparam name="T">The type of elements stored in the buffer.</typeparam>
   /// <remarks>
   /// This provides an efficient mechanism to write to a memory buffer while managing
   /// potential capacity expansion. It allows operations such as appending, resizing, and
   /// allocating additional memory when needed.
   /// </remarks>
   public BufferWriter(
      Span<T> startBuffer, 
      int initalMinGrowCapacity = -1)
   {
      _initalSpanOwner = new SpanOwner<T>(startBuffer);
      _memoryOwner = default;
      
      _initialMinGrowCapacity = initalMinGrowCapacity;
      _span = startBuffer;
   }

   /// <summary>
   /// A structure designed to manage and write to a memory buffer efficiently with support for growing its capacity as needed.
   /// </summary>
   /// <typeparam name="T">The type of elements stored in the buffer.</typeparam>
   /// <remarks>
   /// This structure allows for controlled memory management and optimized writes. It provides mechanisms for initializing
   /// with a minimum size and optionally specifying a minimum capacity for growth. It integrates memory ownership to ensure
   /// efficient use of resources.
   /// </remarks>
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
   
   /// <summary>
   /// Fills the entire current buffer with the specified value.
   /// </summary>
   /// <param name="value">The value to populate the buffer with.</param>
   /// <remarks>
   /// This method assigns the provided value to each element in the buffer,
   /// overwriting any existing data.
   /// </remarks>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Fill(T value)
   {
      _span.Fill(value);
   }

   /// <summary>
   /// Adds an element to the current position in the buffer and advances the position by one.
   /// </summary>
   /// <param name="reference">A reference to the element to be added to the buffer.</param>
   /// <remarks>
   /// If there is insufficient space in the buffer, the capacity is resized to accommodate the new element.
   /// </remarks>
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

   /// <summary>
   /// Adds a new element to the buffer, resizing it if necessary to accommodate the additional element.
   /// </summary>
   /// <param name="value">The element to be added to the buffer.</param>
   /// <remarks>
   /// This method ensures that there is enough capacity in the buffer for the new element by
   /// resizing the buffer if required. The element is then added at the current position, and
   /// the position is incremented accordingly.
   /// </remarks>
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

   /// <summary>
   /// Writes the specified span of elements to the underlying buffer.
   /// </summary>
   /// <param name="span">The span of elements to be written to the buffer.</param>
   /// <remarks>
   /// If the free capacity of the buffer is not enough, the buffer is resized to accommodate
   /// the new data. This method handles both reference and value types to ensure efficient memory
   /// copying. The internal position is adjusted after writing the data.
   /// </remarks>
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

   /// <summary>
   /// Acquires a span of the specified length from the internal buffer, optionally moving the position forward.
   /// <para>
   /// <b>Important:</b> You must ensure to be done writing to the span before writing any new data to the <see cref="BufferWriter{T}"/>
   /// since if there is a resize operation on the following writing, this returned span will be useless.
   /// </para>
   /// </summary>
   /// <param name="length">
   /// The length of the span to acquire. If the requested length exceeds the available capacity, the buffer is resized.
   /// </param>
   /// <param name="movePosition">
   /// A boolean indicating whether to move the current position forward by the requested length. Defaults to true.
   /// </param>
   /// <returns>
   /// A span of the requested length from the internal buffer.
   /// </returns>
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

   /// <summary>
   /// Advances the writer's position by the specified count, ensuring
   /// sufficient capacity in the buffer to accommodate the change.
   /// </summary>
   /// <param name="count">
   /// The number of positions to advance the writer.
   /// If the count exceeds the current free capacity, the buffer will be resized.
   /// </param>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Advance(int count)
   {
      if (FreeCapacity < count)
      {
         Resize(count - FreeCapacity);
      }
      
      _position += count;
   }

   /// <summary>
   /// Advances the writer's position to a specified value within the buffer.
   /// </summary>
   /// <param name="position">The position to advance to. If the specified position exceeds
   /// the current capacity, the buffer is resized to accommodate the new position.</param>
   /// <remarks>
   /// This method directly sets the position of the buffer to the specified value,
   /// expanding the buffer's capacity if necessary. It is useful for scenarios where
   /// sequential writing or direct manipulation of the position is required.
   /// </remarks>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void AdvanceTo(int position)
   {
      if (Capacity < position)
      {
         Resize(position - Capacity);
      }
      
      _position = position;
   }

   /// <summary>
   /// Moves a segment of elements within the buffer from one position to another, possibly resizing the buffer
   /// if the destination position requires additional capacity.
   /// </summary>
   /// <param name="fromStart">The starting index of the segment to move.</param>
   /// <param name="fromSize">The number of elements in the segment to move.</param>
   /// <param name="toStart">The starting index of the destination where the segment will be moved to.</param>
   /// <param name="movePosition">
   /// Determines whether to update the writer's position to the end of the moved segment.
   /// If true, the position is adjusted; otherwise, it remains unchanged.
   /// </param>
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

   /// <summary>
   /// Resizes the internal buffer to accommodate the specified requested size.
   /// </summary>
   /// <param name="requestedSize">The size, in elements, requested to ensure
   /// sufficient capacity in the internal buffer.</param>
   /// <remarks>
   /// This method expands the internal buffer either by the specified requested size or
   /// by incrementing based on the growth logic if the buffer lacks enough free capacity.
   /// It ensures any existing data in the buffer is preserved, migrating it to a new,
   /// larger buffer if necessary.
   /// </remarks>
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
         var newSizeLong = (long)(Math.Max(_memoryOwner.Length, _initalSpanOwner.Length)) + growBy;
         
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

   /// <summary>
   /// Releases unmanaged and optionally managed resources associated with the instance.
   /// </summary>
   /// <remarks>
   /// This method is responsible for cleaning up any resources used by the instance.
   /// It ensures that allocated resources such as memory or other disposable objects
   /// are properly released. Once disposed of, the instance should not be used again.
   /// </remarks>
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
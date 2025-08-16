using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Me.Memory.Buffers;

/// <summary>
/// Represents a managed wrapper around a <see cref="Span{T}"/> that owns and controls the memory it uses.
/// </summary>
/// <typeparam name="T">The type of the items in the span.</typeparam>
/// <remarks>
/// This structure is designed to provide scoped ownership of memory for operations involving spans.
/// It automatically manages the allocation and deallocation of memory using the shared <see cref="ArrayPool{T}"/>.
/// The <see cref="SpanOwner{T}"/> can be created with a specified minimum size or from an existing span.
/// Proper disposal of this structure is required to ensure the memory is correctly returned to the pool.
/// </remarks>
[StructLayout(LayoutKind.Auto)]
public ref struct SpanOwner<T> : IDisposable
{
   public int Length
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get;
      private set;
   }

   /// <summary>
   /// Gets the <see cref="Span{T}"/> that is owned and managed by this <see cref="SpanOwner{T}"/>.
   /// </summary>
   /// <remarks>
   /// This property provides access to the span which is managed by the <see cref="SpanOwner{T}"/> instance.
   /// Accessing this span does not transfer ownership or alter the memory management behavior.
   /// Use this property to perform operations on the memory segment controlled by the current <see cref="SpanOwner{T}"/>.
   /// Ensure that access is within the bounds defined by the <see cref="Length"/> property.
   /// </remarks>
   public Span<T> Span
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get;
   }

   /// <summary>
   /// Represents the underlying array buffer used by the <see cref="SpanOwner{T}"/> for memory allocation and management.
   /// </summary>
   /// <remarks>
   /// This field holds the rented array from the <see cref="ArrayPool{T}"/> shared pool.
   /// It is allocated during initialization and returned to the pool upon disposal to ensure efficient memory usage.
   /// This buffer should only be accessed internally within the <see cref="SpanOwner{T}"/> structure to manage the
   /// memory span effectively and maintain proper memory ownership semantics.
   /// </remarks>
   private readonly T[]? _buffer;

   /// <summary>
   /// Represents a ref struct that owns and works with memory in the form of a <see cref="Span{T}"/>.
   /// </summary>
   /// <typeparam name="T">The element type of the span.</typeparam>
   /// <remarks>
   /// <see cref="SpanOwner{T}"/> provides memory management capabilities by using the <see cref="ArrayPool{T}"/> for efficient allocation and deallocation.
   /// Ownership of memory is scoped and proper disposal is required to avoid memory leaks.
   /// The span can be created with a specified minimum size or from an existing span.
   /// This struct is suitable for high-performance scenarios where memory pooling and scoped lifetimes are critical.
   /// It cannot be used in asynchronous methods or stored in fields because it is a ref struct.
   /// </remarks>
   public SpanOwner(int minSize)
   {
      _buffer = ArrayPool<T>.Shared.Rent(minSize);
      Length = minSize;
   }

   /// <summary>
   /// Represents a ref struct that provides memory management operations by owning a slice of memory as a <see cref="Span{T}"/>.
   /// </summary>
   /// <typeparam name="T">The element type of the span.</typeparam>
   public SpanOwner(Span<T> span)
   {
      Span = span;
      Length = span.Length;
   }

   /// <summary>
   /// Releases the buffer back to the pool if its pool based,
   /// sets the length to 0.
   /// </summary>
   public void Dispose()
   {
      if (_buffer is not null)
      {
         ArrayPool<T>.Shared.Return(_buffer);
      }
      
      Length = 0;
   }
}
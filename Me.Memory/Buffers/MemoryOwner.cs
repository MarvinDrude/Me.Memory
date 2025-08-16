using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Me.Memory.Buffers;

/// <summary>
/// Represents a memory owner that provides managed access to a rented array of memory.
/// </summary>
/// <typeparam name="T">The type of elements in the memory buffer.</typeparam>
/// <remarks>
/// This struct facilitates working with pooled memory for improved performance by limiting reallocations.
/// It uses <see cref="System.Buffers.ArrayPool{T}"/> internally to rent and return memory buffers.
/// Users must dispose of an instance appropriately to release the rented memory back to the pool.
/// </remarks>
[StructLayout(LayoutKind.Auto)]
public struct MemoryOwner<T> : IDisposable
{
   /// <summary>
   /// Gets the total capacity of the underlying memory buffer in elements.
   /// </summary>
   /// <remarks>
   /// The capacity indicates the total size of the rented memory buffer.
   /// It defines the maximum number of elements the buffer can hold without requiring reallocation.
   /// This value is determined at the time of buffer creation or when acquired from the memory pool.
   /// It may be greater than the current <see cref="Length"/> of the buffer.
   /// </remarks>
   /// <value>
   /// An integer representing the total number of elements available in the rented memory buffer.
   /// </value>
   public int Capacity
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Buffer.Length;
   }

   /// <summary>
   /// Gets or sets the current available number of elements in the memory buffer.
   /// </summary>
   /// <remarks>
   /// The length represents the portion of the rented memory buffer currently in use.
   /// It must always be less than or equal to the total <see cref="Capacity"/> of the buffer.
   /// Modifying this value effectively adjusts the accessible range of the memory buffer.
   /// It is the responsibility of the user to ensure the length remains within valid bounds.
   /// </remarks>
   /// <value>
   /// An integer representing the current number of elements in the memory buffer.
   /// </value>
   public int Length
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set;
   }

   /// <summary>
   /// Provides direct access to a contiguous region of memory within the underlying buffer.
   /// </summary>
   /// <remarks>
   /// The span represents a view over a specified range of the buffer, starting at the beginning and extending
   /// for the number of elements defined by <see cref="Length"/>.
   /// Any modifications to this span will directly affect the corresponding memory within the buffer.
   /// This property offers an efficient mechanism for working with slices of memory without creating additional allocations.
   /// </remarks>
   /// <value>
   /// A <see cref="Span{T}"/> representing a range of elements starting at index 0 and extending up to the defined length of the buffer.
   /// </value>
   public Span<T> Span
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Buffer.AsSpan(0, Length);
   }

   /// <summary>
   /// Gets a memory region representing the rented buffer up to the specified length.
   /// </summary>
   /// <remarks>
   /// The memory segment is a managed representation of the buffer, enabling safe operations
   /// on the portion of the buffer up to the current <see cref="Length"/>.
   /// This property provides a convenient way to work with the memory buffer in operations
   /// that require a <see cref="Memory{T}"/> type, such as asynchronous I/O or tasks dependent
   /// on managed spans of data.
   /// </remarks>
   /// <value>
   /// A <see cref="Memory{T}"/> object representing the portion of the buffer currently in use,
   /// starting at index 0 and covering <see cref="Length"/> elements.
   /// </value>
   public Memory<T> Memory
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Buffer.AsMemory(0, Length);
   }

   /// <summary>
   /// Gets the underlying array of the rented memory buffer used for storing elements.
   /// </summary>
   /// <value>
   /// An array representing the rented memory buffer.
   /// </value>
   public T[] Buffer
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get;
   }

   /// <summary>
   /// Represents a memory owner that provides managed access to a rented array of memory.
   /// </summary>
   /// <typeparam name="T">The type of elements in the memory buffer.</typeparam>
   /// <remarks>
   /// This struct facilitates working with pooled memory for improved performance by limiting reallocations.
   /// It uses ArrayPool internally to rent and return memory buffers.
   /// Users must dispose of an instance appropriately to release the rented memory back to the pool.
   /// </remarks>
   public MemoryOwner(int minSize)
   {
      Buffer = ArrayPool<T>.Shared.Rent(minSize);
      Length = minSize;
   }
   
   /// <summary>
   /// Attempts to resize the memory to a specified size, ensuring the size does not exceed the current capacity.
   /// </summary>
   /// <param name="newSize">The desired new size of the memory.</param>
   /// <returns>
   /// Returns <c>true</c> if the memory was resized successfully to the specified size;
   /// otherwise, <c>false</c> if the requested size exceeds the current capacity.
   /// </returns>
   public bool TryResize(int newSize)
   {
      if (newSize > Capacity)
      {
         return false;
      }

      Length = newSize;
      return true;
   }

   /// <summary>
   /// Gives back the memory to the pool.
   /// </summary>
   public void Dispose()
   {
      if (Length <= 0 || Buffer is null) 
         return;
      
      ArrayPool<T>.Shared.Return(Buffer);
   }
}
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Me.Memory.Buffers.Dynamic;

/// <summary>
/// Represents a compact structure for storing boolean values within a single byte.
/// </summary>
/// <remarks>
/// PackedBools is designed to efficiently represent up to 8 boolean values by packing
/// them into the individual bits of a byte. This structure allows for boolean manipulation
/// at specific bit indices using the <see cref="Set"/> and <see cref="Get"/> methods.
/// </remarks>
[StructLayout(LayoutKind.Auto)]
public struct PackedBools
{
   /// <summary>
   /// Gets the raw byte value that holds the packed boolean data.
   /// </summary>
   /// <remarks>
   /// This property exposes the underlying byte where up to 8 boolean values are packed.
   /// Each bit in the byte represents a boolean value, allowing efficient storage
   /// and manipulation of multiple flags.
   /// </remarks>
   public byte RawByte
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _flags;
   }
   private byte _flags;

   /// <summary>
   /// Provides indexed access to the individual bits within the <see cref="PackedBools"/> structure.
   /// </summary>
   /// <remarks>
   /// The indexer allows direct manipulation of boolean values at specific bit positions
   /// ranging from 0 to 7. It utilizes the <see cref="Get"/> and <see cref="Set"/> methods
   /// internally for efficient access and modification of the underlying bit data.
   /// </remarks>
   /// <param name="index">The zero-based position of the bit to access.</param>
   /// <exception cref="ArgumentOutOfRangeException">
   /// Thrown when the <paramref name="index"/> is less than 0 or greater than 7.
   /// </exception>
   /// <returns>
   /// A boolean value representing the state of the bit at the specified index.
   /// </returns>
   public bool this[int index]
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Get(index);
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => Set(index, value);
   }

   /// <summary>
   /// Represents a compact structure for storing up to eight boolean values within a single byte.
   /// </summary>
   /// <remarks>
   /// This struct is designed for efficient storage and manipulation of boolean values,
   /// minimizing memory usage by packing them into the individual bits of a byte.
   /// </remarks>
   public PackedBools(byte flags)
   {
      _flags = flags;
   }

   /// <summary>
   /// Sets the state of the bit at the specified index within the <see cref="PackedBools"/> structure.
   /// </summary>
   /// <remarks>
   /// This method modifies the value of a specific bit in the underlying byte representation,
   /// ensuring efficient storage and manipulation of boolean values at individual bit positions.
   /// </remarks>
   /// <param name="index">The zero-based position of the bit to set, ranging from 0 to 7.</param>
   /// <param name="value">The boolean value to assign to the specified bit index.</param>
   /// <exception cref="ArgumentOutOfRangeException">
   /// Thrown when the <paramref name="index"/> is less than 0 or greater than 7.
   /// </exception>
   public void Set(int index, bool value)
   {
      ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 7, nameof(index));

      if (value)
      {
         _flags |= (byte)(1 << index);
         return;
      }
      
      _flags &= (byte)~(1 << index);
   }

   /// <summary>
   /// Retrieves the boolean value stored in the specified bit position within the <see cref="PackedBools"/> structure.
   /// </summary>
   /// <param name="index">The zero-based position of the bit to retrieve. Must be between 0 and 7.</param>
   /// <exception cref="ArgumentOutOfRangeException">
   /// Thrown if the <paramref name="index"/> is less than 0 or greater than 7.
   /// </exception>
   /// <returns>
   /// A boolean value indicating the state of the bit at the specified index.
   /// Returns true if the bit is set; otherwise, false.
   /// </returns>
   public bool Get(int index)
   {
      ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 7, nameof(index));
      
      return (_flags & (1 << index)) != 0;
   }
}
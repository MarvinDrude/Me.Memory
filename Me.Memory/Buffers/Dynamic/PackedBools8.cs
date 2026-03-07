using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Me.Memory.Buffers.Dynamic;

/// <summary>
/// Represents a compact structure for storing boolean values within a single byte.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public struct PackedBools8(byte flags)
{
   public byte RawByte
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _flags;
   }
   private byte _flags = flags;

   public bool this[int index]
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Get(index);
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => Set(index, value);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public bool Get(int index)
   {
      ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 7, nameof(index));
      
      return (_flags & (1 << index)) != 0;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public int CountSetBits()
   {
      return BitOperations.PopCount(_flags);
   }
}
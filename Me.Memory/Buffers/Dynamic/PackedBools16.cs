using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Me.Memory.Buffers.Dynamic;

/// <summary>
/// Represents a compact structure for storing boolean values within a 16-bit unsigned integer.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public struct PackedBools16(ushort flags)
{
   private ushort _flags = flags;

   public ushort RawValue
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _flags;
   }

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
      ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 15, nameof(index));

      if (value)
      {
         _flags |= (ushort)(1 << index);
         return;
      }
      
      _flags &= (ushort)~(1 << index);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public bool Get(int index)
   {
      ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 15, nameof(index));
      
      return (_flags & (1 << index)) != 0;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public int CountSetBits()
   {
      return BitOperations.PopCount(_flags);
   }
}
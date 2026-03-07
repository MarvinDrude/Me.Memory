using System.Runtime.CompilerServices;

namespace Me.Memory.Buffers.Dynamic;

[InlineArray(2)]
public struct Flags128
{
   private PackedBools64 _element;

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public bool Get(int bitIndex)
   {
      return this[bitIndex >> 6][bitIndex & 63];
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Set(int bitIndex, bool value)
   {
      ArgumentOutOfRangeException.ThrowIfGreaterThan(bitIndex, 127, nameof(bitIndex));

      ref var element = ref this[bitIndex >> 6];
      element[bitIndex & 63] = value;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public int CountSetBits()
   {
      return this[0].CountSetBits() + this[1].CountSetBits();
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void SetRawValues(ulong lower, ulong upper)
   {
      this[0] = new PackedBools64(lower);
      this[1] = new PackedBools64(upper);
   }
}
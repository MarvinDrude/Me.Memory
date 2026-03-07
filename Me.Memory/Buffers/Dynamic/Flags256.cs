using System.Runtime.CompilerServices;

namespace Me.Memory.Buffers.Dynamic;

[InlineArray(4)]
public struct Flags256
{
   private PackedBools64 _element;

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public bool Get(int bitIndex)
   {
      ArgumentOutOfRangeException.ThrowIfGreaterThan(bitIndex, 255, nameof(bitIndex));
      return this[bitIndex >> 6][bitIndex & 63];
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Set(int bitIndex, bool value)
   {
      ArgumentOutOfRangeException.ThrowIfGreaterThan(bitIndex, 255, nameof(bitIndex));

      ref var element = ref this[bitIndex >> 6];
      element[bitIndex & 63] = value;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public int CountSetBits()
   {
      return this[0].CountSetBits() + 
             this[1].CountSetBits() + 
             this[2].CountSetBits() + 
             this[3].CountSetBits();
   }
   
   /// <summary>
   /// Sets the raw 64-bit values for all four segments.
   /// </summary>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void SetRawValues(ulong v0, ulong v1, ulong v2, ulong v3)
   {
      this[0] = new PackedBools64(v0);
      this[1] = new PackedBools64(v1);
      this[2] = new PackedBools64(v2);
      this[3] = new PackedBools64(v3);
   }
}
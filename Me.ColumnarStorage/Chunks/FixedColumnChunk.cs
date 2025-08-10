using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Me.ColumnarStorage.Chunks;

[StructLayout(LayoutKind.Auto)]
public readonly ref struct FixedColumnChunk<T>
{
   public readonly string ColumnName;
   
   public readonly ReadOnlySpan<T> Values;
   public readonly ReadOnlySpan<byte> BitMask;

   public int RowCount
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Values.Length;
   }

   public FixedColumnChunk(
      string columnName,
      ReadOnlySpan<T> values,
      ReadOnlySpan<byte> bitMask)
   {
      ColumnName = columnName;
      
      Values = values;
      BitMask = bitMask;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public bool IsPresent(int row)
   {
      if (BitMask.IsEmpty) return false;
      return (BitMask[row >> 3] & (1 << (row & 7))) != 0;
   }
}
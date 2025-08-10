namespace Me.ColumnarStorage.Config;

public sealed class ColumnarTableOptions
{
   public int MaxRowsPerSegment { get; set; } = 32_000;
}
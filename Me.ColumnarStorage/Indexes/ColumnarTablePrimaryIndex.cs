namespace Me.ColumnarStorage.Indexes;

public readonly record struct ColumnarTablePrimaryIndex(
   int SegmentId,
   int RowIndexInSegment);
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Me.Memory.Extensions;

namespace Me.Memory.Buffers.Dynamic;

public ref partial struct RegionByteBuffer
{
   private int _handleCapacity;
   private int _handleActiveCount;
   
   private int _headerCapacity;
   private int _headerLength;
   
   private int _dataLength;
   
   private int HeaderOffset
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _handleCapacity;
   }

   private int DataOffset
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _handleCapacity + _headerCapacity;
   }
   
   private RegionHeader ReadRegionHeader(RegionId regionId)
   {
      var offset = ReadRegionHandle(regionId);
      if (offset == EmptyRegionId)
      {
         return default;
      }

      _bufferWriter.Position = HeaderOffset + offset;
      return _bufferWriter.AcquireSpan(Unsafe.SizeOf<RegionHeader>())
         .ReadLittleEndian<RegionHeader>(out _);
   }

   private void WriteRegionHeader(RegionId regionId, RegionHeader header)
   {
      var offset = ReadRegionHandle(regionId);
      var sizeOf = Unsafe.SizeOf<RegionHeader>();
      
      if (offset == EmptyRegionId)
      {
         offset = _headerLength;
         
         EnsureHeaderCapacity(_handleActiveCount + 1);
         WriteRegionHandle(regionId, offset);
         
         _headerLength += sizeOf;
      }

      _bufferWriter.Position = HeaderOffset + offset;
      header.WriteLittleEndian(_bufferWriter.AcquireSpan(sizeOf));
   }

   private void RemoveRegionHeader(RegionId regionId)
   {
      var offset = ReadRegionHandle(regionId);
      if (offset == EmptyRegionId) return;
      
      var sizeOf = Unsafe.SizeOf<RegionHeader>();
      WriteRegionHandle(regionId, EmptyRegionId);

      var start = HeaderOffset + offset;
      var length = _headerLength - start;

      if (length > sizeOf)
      {
         _bufferWriter.Move(start, length, start - sizeOf);
      }
      
      _headerLength -= sizeOf;
   }
   
   private void EnsureHandleCapacity(int newRegionCount)
   {
      var bytesNeeded = newRegionCount * sizeof(int);
      if (bytesNeeded <= _handleCapacity)
      {
         return;
      }
      
      _bufferWriter.Move(HeaderOffset, _headerCapacity + _dataLength, bytesNeeded);
      _handleCapacity = bytesNeeded;
   }

   private void EnsureHeaderCapacity(int newActiveCount)
   {
      var bytesNeeded = newActiveCount * Unsafe.SizeOf<RegionHeader>();
      if (bytesNeeded <= _headerCapacity)
      {
         return;
      }
      
      _bufferWriter.Move(DataOffset, _dataLength, HeaderOffset + bytesNeeded);
      _headerCapacity = bytesNeeded;
   }

   [StructLayout(LayoutKind.Sequential)]
   private readonly record struct RegionHeader(
      int DataOffset,
      int DataLength,
      int Count);
   
   [StructLayout(LayoutKind.Sequential)]
   private readonly record struct RegionHandle(int Offset)
   {
      public bool InUse => Offset >= 0;
   }
}
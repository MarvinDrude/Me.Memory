using System.Runtime.InteropServices;
using Me.Memory.Extensions;

namespace Me.Memory.Buffers.Dynamic;

public ref partial struct RegionByteBuffer
{
   private const int EmptyRegionId = -1;
   
   private RegionId GetNextRegionId()
   {
      return new RegionId(GetNextRegionIdRaw());
   }
   
   private int GetNextRegionIdRaw()
   {
      return _regionCount++;
   }
   
   private int ReadRegionHandle(RegionId regionId)
   {
      _bufferWriter.Position = regionId.Value;
      var handleSpan = _bufferWriter.AcquireSpan(sizeof(int));

      return handleSpan.ReadLittleEndian<int>(out _);
   }

   private void WriteRegionHandle(RegionId regionId, int headerOffset)
   {
      _bufferWriter.Position = regionId.Value;
      
      var handleSpan = _bufferWriter.AcquireSpan(sizeof(int));
      EmptyRegionId.WriteLittleEndian(handleSpan);
   }
   
   [StructLayout(LayoutKind.Auto)]
   public readonly record struct RegionId(int Value);
}
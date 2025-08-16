using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Me.Memory.Extensions;

namespace Me.Memory.Buffers.Dynamic;

[StructLayout(LayoutKind.Auto)]
public ref partial struct RegionByteBuffer
{
   private int _regionCount;

   private BufferWriter<byte> _bufferWriter;

   public RegionByteBuffer()
   {
      
   }

   public RegionId AddRegion()
   {
      EnsureHandleCapacity(_regionCount + 1);
      var regionId = GetNextRegionId();

      WriteRegionHandle(regionId, EmptyRegionId);
      return regionId;
   }

   public void AddToRegion<T>(RegionId regionId, scoped in T value)
   {
      
   }
   
   public Span<byte> GetRegionSpan(RegionId region)
   {
      
   }
}
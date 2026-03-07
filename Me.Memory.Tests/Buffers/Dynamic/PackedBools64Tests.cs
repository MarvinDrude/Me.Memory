using Me.Memory.Buffers.Dynamic;

namespace Me.Memory.Tests.Buffers.Dynamic;

public class PackedBools64Tests
{
   [Test]
   public async Task ManipulationAcrossFullRange()
   {
      var packed = new PackedBools64(0);
      packed[0] = true;
      packed[63] = true;

      await Assert.That(packed.Get(0)).IsTrue();
      await Assert.That(packed.Get(63)).IsTrue();
      await Assert.That(packed.RawValue).IsEqualTo(0x8000000000000001ul);
   }

   [Test]
   public async Task CountSetBitsForLargeValue()
   {
      var packed = new PackedBools64(ulong.MaxValue);
      await Assert.That(packed.CountSetBits()).IsEqualTo(64);
   }
}
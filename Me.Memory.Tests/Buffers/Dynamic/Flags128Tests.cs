using Me.Memory.Buffers.Dynamic;

namespace Me.Memory.Tests.Buffers.Dynamic;

public class Flags128Tests
{
   [Test]
   public async Task SetRawValuesUpdatesCorrectSegments()
   {
      var flags = new Flags128();
      flags.SetRawValues(1, 2);

      await Assert.That(flags[0].RawValue).IsEqualTo(1ul);
      await Assert.That(flags[1].RawValue).IsEqualTo(2ul);
   }

   [Test]
   public async Task ManipulationCrosses64BitBoundary()
   {
      var flags = new Flags128();
      
      flags.Set(63, true);
      flags.Set(64, true);

      await Assert.That(flags.Get(63)).IsTrue();
      await Assert.That(flags.Get(64)).IsTrue();
      
      await Assert.That(flags[0].RawValue).IsEqualTo(0x8000000000000000ul);
      await Assert.That(flags[1].RawValue).IsEqualTo(1ul);
   }

   [Test]
   public async Task CountSetBitsAggregatesSegments()
   {
      var flags = new Flags128();
      flags.SetRawValues(0xF, 0xF); // 4 bits set in each
      await Assert.That(flags.CountSetBits()).IsEqualTo(8);
   }
}
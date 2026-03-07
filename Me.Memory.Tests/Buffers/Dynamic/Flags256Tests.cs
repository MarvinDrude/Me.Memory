using Me.Memory.Buffers.Dynamic;

namespace Me.Memory.Tests.Buffers.Dynamic;

public class Flags256Tests
{
   [Test]
   public async Task FullRangeTest()
   {
      var flags = new Flags256();
      
      // Test indices in different 64-bit segments
      flags.Set(0, true);   // Segment 0
      flags.Set(70, true);  // Segment 1 (64 + 6)
      flags.Set(130, true); // Segment 2 (128 + 2)
      flags.Set(255, true); // Segment 3

      await Assert.That(flags.Get(0)).IsTrue();
      await Assert.That(flags.Get(70)).IsTrue();
      await Assert.That(flags.Get(130)).IsTrue();
      await Assert.That(flags.Get(255)).IsTrue();
   }

   [Test]
   public async Task SetRawValuesInitializesAllSegments()
   {
      var flags = new Flags256();
      flags.SetRawValues(1, 2, 3, 4);

      await Assert.That(flags[0].RawValue).IsEqualTo(1ul);
      await Assert.That(flags[1].RawValue).IsEqualTo(2ul);
      await Assert.That(flags[2].RawValue).IsEqualTo(3ul);
      await Assert.That(flags[3].RawValue).IsEqualTo(4ul);
   }

   [Test]
   public async Task OutOfBoundsThrows()
   {
      var flags = new Flags256();
      await Assert.That(() => flags.Set(256, true)).Throws<ArgumentOutOfRangeException>();
   }
}
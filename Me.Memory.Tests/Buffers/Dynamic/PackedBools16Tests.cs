using Me.Memory.Buffers.Dynamic;

namespace Me.Memory.Tests.Buffers.Dynamic;

public class PackedBools16Tests
{
   [Test]
   public async Task ConstructorInitializesRawValue()
   {
      ushort result;
      {
         // 0xAAAA = 10101010 10101010
         var packed = new PackedBools16(0xAAAA);
         result = packed.RawValue;
      }
      await Assert.That(result).IsEqualTo((ushort)0xAAAA);
   }

   [Test]
   public async Task GetSetAndIndexerWork()
   {
      var packed = new PackedBools16(0);
      packed.Set(15, true); // Highest bit
      
      await Assert.That(packed.Get(15)).IsTrue();
      await Assert.That(packed.RawValue).IsEqualTo((ushort)0x8000);

      packed[0] = true;
      await Assert.That(packed[0]).IsTrue();
      await Assert.That(packed.RawValue).IsEqualTo((ushort)0x8001);
   }

   [Test]
   public async Task MethodsThrowOnOutOfBounds()
   {
      var packed = new PackedBools16(0);
      await Assert.That(() => packed.Get(16)).Throws<ArgumentOutOfRangeException>();
   }
}
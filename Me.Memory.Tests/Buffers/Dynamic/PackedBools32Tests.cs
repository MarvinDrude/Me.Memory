using Me.Memory.Buffers.Dynamic;

namespace Me.Memory.Tests.Buffers.Dynamic;

public class PackedBools32Tests
{
   [Test]
   public async Task ConstructorInitializesRawValue()
   {
      uint result;
      {
         var packed = new PackedBools32(0xCAFEBABE);
         result = packed.RawValue;
      }
      await Assert.That(result).IsEqualTo(0xCAFEBABE);
   }

   [Test]
   public async Task SetDisablesBitCorrectly()
   {
      var packed = new PackedBools32(uint.MaxValue);
      packed.Set(31, false);
      
      const uint expected = 0x7FFFFFFF;
      await Assert.That(packed.RawValue).IsEqualTo(expected);
   }

   [Test]
   public async Task CountSetBitsReturnsCorrectValue()
   {
      var packed = new PackedBools32(0b1101); // 13
      await Assert.That(packed.CountSetBits()).IsEqualTo(3);
   }
}
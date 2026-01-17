using Me.Memory.Buffers.Dynamic;

namespace Me.Memory.Tests.Buffers.Dynamic;

public class PackedBoolsTests
{
   [Test]
   public async Task ConstructorInitializesRawByte()
   {
      byte result;

      {
         // 0xAA = 1010 1010
         var packed = new PackedBools(0xAA);
         result = packed.RawByte;
      }

      const byte expected = 0xAA;
      await Assert.That(result).IsEqualTo(expected);
   }

   [Test]
   public async Task GetReturnsCorrectBooleanForBitIndex()
   {
      bool bit0;
      bool bit1;
      bool bit7;

      {
         // 0x81 = 1000 0001 (Bit 7 and Bit 0 are set)
         var packed = new PackedBools(0x81);
         
         bit0 = packed.Get(0);
         bit1 = packed.Get(1);
         bit7 = packed.Get(7);
      }

      await Assert.That(bit0).IsTrue();
      await Assert.That(bit1).IsFalse();
      await Assert.That(bit7).IsTrue();
   }

   [Test]
   public async Task SetEnablesBitCorrectly()
   {
      byte result;

      {
         var packed = new PackedBools(0); // 0000 0000
         
         // Set bit 2 -> 0000 0100 (4)
         packed.Set(2, true);
         
         result = packed.RawByte;
      }

      const byte expected = 4;
      await Assert.That(result).IsEqualTo(expected);
   }

   [Test]
   public async Task SetDisablesBitCorrectly()
   {
      byte result;

      {
         var packed = new PackedBools(0xFF); // 1111 1111
         
         // Clear bit 0 -> 1111 1110 (0xFE)
         packed.Set(0, false);
         
         result = packed.RawByte;
      }

      const byte expected = 0xFE;
      await Assert.That(result).IsEqualTo(expected);
   }

   [Test]
   public async Task IndexerReadWritesToUnderlyingBits()
   {
      bool valRead;
      byte finalByte;

      {
         var packed = new PackedBools(0)
         {
            // Use Indexer setter
            [3] = true // 0000 1000 (8)
         };

         // Use Indexer getter
         valRead = packed[3];
         finalByte = packed.RawByte;
      }

      const byte expectedByte = 8;
      
      await Assert.That(valRead).IsTrue();
      await Assert.That(finalByte).IsEqualTo(expectedByte);
   }

   [Test]
   public async Task MultipleBitsCanBeManipulated()
   {
      byte result;

      {
         var packed = new PackedBools(0)
         {
            [1] = true, // 3
            [0] = false // 2 (0000 0010)
         };

         result = packed.RawByte;
      }

      const byte expected = 2;
      await Assert.That(result).IsEqualTo(expected);
   }

   [Test]
   public async Task MethodsThrowOnOutOfBounds()
   {
      await Assert.That(() =>
      {
         var packed = new PackedBools(0);
         packed.Get(8);
      }).Throws<ArgumentOutOfRangeException>();

      await Assert.That(() =>
      {
         var packed = new PackedBools(0);
         packed.Set(8, true);
      }).Throws<ArgumentOutOfRangeException>();

      await Assert.That(() =>
      {
         var packed = new PackedBools(0);
         var b = packed[8];
      }).Throws<ArgumentOutOfRangeException>();
   }
}
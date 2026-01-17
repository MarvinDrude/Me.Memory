using System.Text;
using Me.Memory.Buffers;

namespace Me.Memory.Tests.Buffers;

public class ByteReaderTests
{
   [Test]
   public async Task ConstructorSpanSetsPositionZero()
   {
      int pos;
      int remaining;
      
      {
         var data = (stackalloc byte[10]);
         var reader = new ByteReader(data);
         
         pos = reader.Position;
         remaining = reader.BytesRemaining;
      }

      await Assert.That(pos).IsEqualTo(0);
      await Assert.That(remaining).IsEqualTo(10);
   }

   [Test]
   public async Task ReadByteSpanAdvancesPosition()
   {
      byte b1, b2;
      int posAfter;

      {
         byte[] data = [0xAA, 0xBB, 0xCC];
         var reader = new ByteReader(data);

         b1 = reader.ReadByte();
         b2 = reader.ReadByte();
         posAfter = reader.Position;
      }

      await Assert.That(b1).IsEqualTo((byte)0xAA);
      await Assert.That(b2).IsEqualTo((byte)0xBB);
      await Assert.That(posAfter).IsEqualTo(2);
   }

   [Test]
   public async Task ReadBytesSpanReturnsSlice()
   {
      byte[] result;

      {
         byte[] data = [1, 2, 3, 4, 5];
         var reader = new ByteReader(data);

         reader.ReadByte();
         var span = reader.ReadBytes(3); 
         
         result = span.ToArray();
      }

      byte[] shouldBe = [2, 3, 4];
      await Assert.That(result).IsEquivalentTo(shouldBe);
   }

   [Test]
   public async Task ReadLittleEndianSpanReadsCorrectly()
   {
      int value;
      int pos;

      {
         // 4 bytes for Int32: 0x01, 0x00, 0x00, 0x00 -> 1 (Little Endian)
         byte[] data = [1, 0, 0, 0, 99]; 
         var reader = new ByteReader(data);

         value = reader.ReadLittleEndian<int>();
         pos = reader.Position;
      }

      await Assert.That(value).IsEqualTo(1);
      await Assert.That(pos).IsEqualTo(4);
   }

   [Test]
   public async Task ReadBigEndianSpanReadsCorrectly()
   {
      short value;
      
      {
         // 0x01, 0x02 -> 0x0102 (258) in Big Endian
         byte[] data = [0x01, 0x02]; 
         var reader = new ByteReader(data);

         value = reader.ReadBigEndian<short>();
      }

      await Assert.That(value).IsEqualTo((short)0x0102);
   }

   [Test]
   public async Task ReadStringSpanDecodesUtf8()
   {
      string result;

      {
         var text = "Hello World";
         var bytes = Encoding.UTF8.GetBytes(text);
         var reader = new ByteReader(bytes);

         result = reader.ReadString(bytes.Length, Encoding.UTF8);
      }

      await Assert.That(result).IsEqualTo("Hello World");
   }

   [Test]
   public async Task ReadStringRawSpanCastsBytesToChars()
   {
      char[] result;

      {
         // 'A' is 65 (0x41). In raw cast, we treat bytes as memory for chars.
         // A Char is 2 bytes (UTF-16). 
         // So [0x41, 0x00] -> 'A' in Little Endian architecture
         var data = "A\0B\0"u8.ToArray();
         
         var reader = new ByteReader(data);
         
         var chars = reader.ReadStringRaw(4);
         result = chars.ToArray();
      }

      await Assert.That(result).IsEquivalentTo(['A', 'B']);
   }

   [Test]
   public async Task PositionSetSpanUpdatesCursor()
   {
      byte val;

      {
         byte[] data = [10, 20, 30, 40];
         var reader = new ByteReader(data)
         {
            Position = 2 // Jump to 30
         };

         val = reader.ReadByte();
      }

      await Assert.That(val).IsEqualTo((byte)30);
   }

   [Test]
   public async Task StreamModeThrowsOnPositionAccess()
   {
      await Assert.That(() =>
      {
         using var ms = new MemoryStream([1, 2, 3]);
         var reader = new ByteReader(ms);
         var p = reader.Position;
      }).Throws<ArgumentOutOfRangeException>();

      await Assert.That(() =>
      {
         using var ms = new MemoryStream([1, 2, 3]);
         var reader = new ByteReader(ms);
         reader.Position = 0;
      }).Throws<ArgumentOutOfRangeException>();

      await Assert.That(() =>
      {
         using var ms = new MemoryStream([1, 2, 3]);
         var reader = new ByteReader(ms);
         var r = reader.BytesRemaining;
      }).Throws<ArgumentOutOfRangeException>();
   }

   [Test]
   public async Task StreamModeReadByteReadsFromStream()
   {
      byte b1;
      
      {
         using var ms = new MemoryStream([0xFA, 0xFB]);
         var reader = new ByteReader(ms);
         
         b1 = reader.ReadByte();
      }

      await Assert.That(b1).IsEqualTo((byte)0xFA);
   }
   
   [Test]
   public async Task StreamModeReadBytesAcquiresSpanFromStream()
   {
      byte[] result;
      
      {
         using var ms = new MemoryStream([1, 2, 3, 4, 5]);
         var reader = new ByteReader(ms);
         
         // Assuming StreamReaderSlim correctly handles AcquireSpan
         var span = reader.ReadBytes(3);
         result = span.ToArray();
      }

      byte[] shouldBe = [1, 2, 3];
      await Assert.That(result).IsEquivalentTo(shouldBe);
   }
}
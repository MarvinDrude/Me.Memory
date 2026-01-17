using System.Text;
using Me.Memory.Buffers;

namespace Me.Memory.Tests.Buffers;

public class ByteWriterTests
{
   [Test]
   public async Task ConstructorBufferStartsEmpty()
   {
      int pos;
      int length;

      {
         var writer = new ByteWriter(stackalloc byte[10]);
         pos = writer.Position;
         length = writer.WrittenSpan.Length;
         writer.Dispose();
      }

      await Assert.That(pos).IsEqualTo(0);
      await Assert.That(length).IsEqualTo(0);
   }

   [Test]
   public async Task WriteByteBufferAppendsCorrectly()
   {
      byte[] result;

      {
         var writer = new ByteWriter(stackalloc byte[10]);
         writer.WriteByte(0xAA);
         writer.WriteByte(0xBB);
         
         result = writer.WrittenSpan.ToArray();
         writer.Dispose();
      }

      byte[] shouldBe = [0xAA, 0xBB];
      await Assert.That(result).IsEquivalentTo(shouldBe);
   }

   [Test]
   public async Task WriteBytesBufferWritesSpan()
   {
      byte[] result;

      {
         var writer = new ByteWriter(stackalloc byte[10]);
         writer.WriteBytes([1, 2, 3]);
         
         result = writer.WrittenSpan.ToArray();
         writer.Dispose();
      }
      
      byte[] shouldBe = [1, 2, 3];
      await Assert.That(result).IsEquivalentTo(shouldBe);
   }

   [Test]
   public async Task WriteLittleEndianBufferWritesCorrectOrder()
   {
      byte[] result;
      int writtenBytes;

      {
         var writer = new ByteWriter(stackalloc byte[10]);
         // 0x01020304 -> Little Endian: 04 03 02 01
         writtenBytes = writer.WriteLittleEndian(0x01020304);
         
         result = writer.WrittenSpan.ToArray();
         writer.Dispose();
      }

      await Assert.That(writtenBytes).IsEqualTo(4);
      
      byte[] shouldBe = [0x04, 0x03, 0x02, 0x01];
      await Assert.That(result).IsEquivalentTo(shouldBe);
   }

   [Test]
   public async Task WriteBigEndianBufferWritesCorrectOrder()
   {
      byte[] result;
      int writtenBytes;

      {
         var writer = new ByteWriter(stackalloc byte[10]);
         // 0x01020304 -> Big Endian: 01 02 03 04
         writtenBytes = writer.WriteBigEndian(0x01020304);
         
         result = writer.WrittenSpan.ToArray();
         writer.Dispose();
      }

      await Assert.That(writtenBytes).IsEqualTo(4);
      
      byte[] shouldBe = [0x01, 0x02, 0x03, 0x04];
      await Assert.That(result).IsEquivalentTo(shouldBe);
   }

   [Test]
   public async Task WriteStringBufferWritesUtf8ByDefault()
   {
      string resultString;
      int writtenBytes;

      {
         var writer = new ByteWriter(stackalloc byte[20]);
         writtenBytes = writer.WriteString("Hello World");
         
         var span = writer.WrittenSpan;
         resultString = Encoding.UTF8.GetString(span);
         writer.Dispose();
      }

      await Assert.That(resultString).IsEqualTo("Hello World");
      await Assert.That(writtenBytes).IsEqualTo(11);
   }

   [Test]
   public async Task WriteStringRawBufferWritesDirectMemoryBytes()
   {
      byte[] result;

      {
         var writer = new ByteWriter(stackalloc byte[20]);
         // "Hi" -> 'H' (0x48, 0x00), 'i' (0x69, 0x00)
         writer.WriteStringRaw("Hi");
         
         result = writer.WrittenSpan.ToArray();
         writer.Dispose();
      }

      // 2 chars * 2 bytes = 4 bytes
      var shouldBe = "H\0i\0"u8.ToArray();
      await Assert.That(result).IsEquivalentTo(shouldBe);
   }

   [Test]
   public async Task FillBufferFillsCapacity()
   {
      byte[] result;

      {
         var writer = new ByteWriter(stackalloc byte[4]);
         writer.Fill(0xFF);
         
         writer.Position = 4; 
         result = writer.WrittenSpan.ToArray();
         writer.Dispose();
      }

      byte[] shouldBe = [0xFF, 0xFF, 0xFF, 0xFF];
      await Assert.That(result).IsEquivalentTo(shouldBe);
   }

   [Test]
   public async Task PositionBufferGetSetWorks()
   {
      int pos;
      
      {
         var writer = new ByteWriter(stackalloc byte[10]);
         writer.WriteByte(1);
         writer.Position = 5;
         
         pos = writer.Position;
         writer.Dispose();
      }

      await Assert.That(pos).IsEqualTo(5);
   }

   [Test]
   public async Task PropertiesStreamModeThrowExceptions()
   {
      await Assert.That(() =>
      {
         using var ms = new MemoryStream();
         var writer = new ByteWriter(ms);
         var p = writer.Position;
      }).Throws<ArgumentOutOfRangeException>();

      await Assert.That(() =>
      {
         using var ms = new MemoryStream();
         var writer = new ByteWriter(ms);
         writer.Position = 0;
      }).Throws<ArgumentOutOfRangeException>();

      await Assert.That(() =>
      {
         using var ms = new MemoryStream();
         var writer = new ByteWriter(ms);
         var s = writer.WrittenSpan;
      }).Throws<InvalidOperationException>();
      
      await Assert.That(() =>
      {
         using var ms = new MemoryStream();
         var writer = new ByteWriter(ms);
         writer.Fill(0);
      }).Throws<ArgumentOutOfRangeException>();
   }

   [Test]
   public async Task WriteByteStreamModeWritesToStream()
   {
      using var ms = new MemoryStream();
      
      {
         var writer = new ByteWriter(ms);
         writer.WriteByte(100);
         writer.Flush(); 
         writer.Dispose();
      }

      byte[] shouldBe = [100];
      await Assert.That(ms.ToArray()).IsEquivalentTo(shouldBe);
   }

   [Test]
   public async Task WriteBytesStreamModeWritesToStream()
   {
      using var ms = new MemoryStream();

      {
         var writer = new ByteWriter(ms);
         writer.WriteBytes([1, 2, 3]);
         writer.Flush();
         writer.Dispose();
      }

      byte[] shouldBe = [1, 2, 3];
      await Assert.That(ms.ToArray()).IsEquivalentTo(shouldBe);
   }

   [Test]
   public async Task WriteStringStreamModeWritesToStream()
   {
      using var ms = new MemoryStream();

      {
         var writer = new ByteWriter(ms);
         writer.WriteString("Test");
         writer.Flush();
         writer.Dispose();
      }

      var text = Encoding.UTF8.GetString(ms.ToArray());
      await Assert.That(text).IsEqualTo("Test");
   }
}
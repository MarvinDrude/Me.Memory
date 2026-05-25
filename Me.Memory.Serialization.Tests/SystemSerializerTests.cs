#pragma warning disable CS8600, CS8619

using System.Buffers;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Formatters.System;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Tests;

public class SystemSerializerTests
{
   private static async Task TestSerializer<T, TSerializer>(T value, int expectedSize)
      where TSerializer : ISerializer<T>, new()
   {
      var serializer = new TSerializer();
      
      // 1. Calculate length
      var length = serializer.CalculateByteLength(in value);
      await Assert.That(length).IsEqualTo(expectedSize);
      
      // 2. Write and contiguous read
      byte[] buffer = new byte[expectedSize];
      
      var (written, readSuccess, readValue, remaining) = WriteAndReadContiguous<T, TSerializer>(serializer, buffer, value);
      
      await Assert.That(written).IsEqualTo(expectedSize);
      await Assert.That(readSuccess).IsTrue();
      await Assert.That(readValue).IsEqualTo(value);
      await Assert.That(remaining).IsEqualTo(0);
      
      // 3. Multi-segment read (split the written bytes into 1-byte segments)
      if (expectedSize > 1)
      {
         var (multiReadSuccess, multiReadValue, multiRemaining) = ReadMultiSegment<T, TSerializer>(serializer, buffer, expectedSize);
         
         await Assert.That(multiReadSuccess).IsTrue();
         await Assert.That(multiReadValue).IsEqualTo(value);
         await Assert.That(multiRemaining).IsEqualTo(0);
      }
   }

   private static (int written, bool readSuccess, T readValue, long remaining) WriteAndReadContiguous<T, TSerializer>(TSerializer serializer, byte[] buffer, T value)
      where TSerializer : ISerializer<T>
   {
      var writer = new BufferWriter<byte>(buffer);
      var written = serializer.Write(ref writer, in value);
      
      var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(buffer));
      var readSuccess = serializer.TryRead(ref reader, out T readValue);
      
      return (written, readSuccess, readValue, reader.Remaining);
   }

   private static (bool readSuccess, T readValue, long remaining) ReadMultiSegment<T, TSerializer>(TSerializer serializer, byte[] buffer, int expectedSize)
      where TSerializer : ISerializer<T>
   {
      BufferSegment<byte> firstSegment = new(new byte[] { buffer[0] });
      BufferSegment<byte> currentSegment = firstSegment;
      for (int i = 1; i < expectedSize; i++)
      {
         currentSegment = currentSegment.Append(new byte[] { buffer[i] });
      }
      
      var multiSegmentSequence = new ReadOnlySequence<byte>(firstSegment, 0, currentSegment, 1);
      var multiSegmentReader = new SequenceReader<byte>(multiSegmentSequence);
      
      var multiReadSuccess = serializer.TryRead(ref multiSegmentReader, out T multiReadValue);
      return (multiReadSuccess, multiReadValue, multiSegmentReader.Remaining);
   }

   [Test]
   public async Task TestTimeSpanSerializer()
   {
      await TestSerializer<TimeSpan, TimeSpanSerializer>(TimeSpan.Zero, sizeof(long));
      await TestSerializer<TimeSpan, TimeSpanSerializer>(TimeSpan.FromHours(5.5), sizeof(long));
      await TestSerializer<TimeSpan, TimeSpanSerializer>(TimeSpan.FromSeconds(-42), sizeof(long));
   }

   [Test]
   public async Task TestDateTimeSerializer()
   {
      await TestSerializer<DateTime, DateTimeSerializer>(DateTime.MinValue, sizeof(long));
      await TestSerializer<DateTime, DateTimeSerializer>(DateTime.MaxValue, sizeof(long));
      await TestSerializer<DateTime, DateTimeSerializer>(DateTime.UtcNow, sizeof(long));
      await TestSerializer<DateTime, DateTimeSerializer>(DateTime.Now, sizeof(long));
   }

   [Test]
   public async Task TestDateTimeOffsetSerializer()
   {
      await TestSerializer<DateTimeOffset, DateTimeOffsetSerializer>(DateTimeOffset.MinValue, sizeof(long) * 2);
      await TestSerializer<DateTimeOffset, DateTimeOffsetSerializer>(DateTimeOffset.MaxValue, sizeof(long) * 2);
      await TestSerializer<DateTimeOffset, DateTimeOffsetSerializer>(DateTimeOffset.UtcNow, sizeof(long) * 2);
      await TestSerializer<DateTimeOffset, DateTimeOffsetSerializer>(
         new DateTimeOffset(2026, 5, 25, 12, 30, 0, TimeSpan.FromHours(-5)), sizeof(long) * 2);
   }

   [Test]
   public async Task TestGuidSerializer()
   {
      await TestSerializer<Guid, GuidSerializer>(Guid.Empty, 16);
      await TestSerializer<Guid, GuidSerializer>(Guid.NewGuid(), 16);
   }

   [Test]
   public async Task TestUriSerializer()
   {
      var absoluteUri = new Uri("https://marvindrude.com/api/test?param=1");
      var relativeUri = new Uri("/local/relative/path", UriKind.Relative);

      await TestSerializer<Uri, UriSerializer>(absoluteUri, sizeof(int) + System.Text.Encoding.UTF8.GetByteCount(absoluteUri.OriginalString));
      await TestSerializer<Uri, UriSerializer>(relativeUri, sizeof(int) + System.Text.Encoding.UTF8.GetByteCount(relativeUri.OriginalString));
   }

   [Test]
   public async Task TestTimeOnlySerializer()
   {
      await TestSerializer<TimeOnly, TimeOnlySerializer>(TimeOnly.MinValue, sizeof(long));
      await TestSerializer<TimeOnly, TimeOnlySerializer>(TimeOnly.MaxValue, sizeof(long));
      await TestSerializer<TimeOnly, TimeOnlySerializer>(new TimeOnly(14, 30, 15), sizeof(long));
   }

   [Test]
   public async Task TestDateOnlySerializer()
   {
      await TestSerializer<DateOnly, DateOnlySerializer>(DateOnly.MinValue, sizeof(int));
      await TestSerializer<DateOnly, DateOnlySerializer>(DateOnly.MaxValue, sizeof(int));
      await TestSerializer<DateOnly, DateOnlySerializer>(new DateOnly(2026, 5, 25), sizeof(int));
   }
}

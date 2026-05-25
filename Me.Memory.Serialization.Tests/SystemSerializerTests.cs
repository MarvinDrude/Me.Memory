#pragma warning disable CS8600, CS8619

using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Numerics;
using System.Text;
using Me.Memory.Buffers;
using Me.Memory.Serialization;
using Me.Memory.Serialization.Formatters.Collections;
using Me.Memory.Serialization.Formatters.System;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Tests;

public class SystemSerializerTests
{
   private static async Task TestSerializer<T, TSerializer>(T value, int expectedSize)
      where TSerializer : ISerializer<T>
   {
      // 1. Calculate length
      var length = TSerializer.CalculateByteLength(in value);
      await Assert.That(length).IsEqualTo(expectedSize);
      
      // 2. Write and contiguous read
      byte[] buffer = new byte[expectedSize];
      
      var (written, readSuccess, readValue, remaining) = WriteAndReadContiguous<T, TSerializer>(buffer, value);
      
      await Assert.That(written).IsEqualTo(expectedSize);
      await Assert.That(readSuccess).IsTrue();
      await Assert.That(readValue).IsEqualTo(value);
      await Assert.That(remaining).IsEqualTo(0);
      
      // 3. Multi-segment read (split the written bytes into 1-byte segments)
      if (expectedSize > 1)
      {
         var (multiReadSuccess, multiReadValue, multiRemaining) = ReadMultiSegment<T, TSerializer>(buffer, expectedSize);
         
         await Assert.That(multiReadSuccess).IsTrue();
         await Assert.That(multiReadValue).IsEqualTo(value);
         await Assert.That(multiRemaining).IsEqualTo(0);
      }
   }

   private static (int written, bool readSuccess, T readValue, long remaining) WriteAndReadContiguous<T, TSerializer>(byte[] buffer, T value)
      where TSerializer : ISerializer<T>
   {
      var writer = new BufferWriter<byte>(buffer);
      var written = TSerializer.Write(ref writer, in value);
      
      var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(buffer));
      var readSuccess = TSerializer.TryRead(ref reader, out T readValue);
      
      return (written, readSuccess, readValue, reader.Remaining);
   }

   private static (bool readSuccess, T readValue, long remaining) ReadMultiSegment<T, TSerializer>(byte[] buffer, int expectedSize)
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
      
      var multiReadSuccess = TSerializer.TryRead(ref multiSegmentReader, out T multiReadValue);
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

    [Test]
    public async Task TestSerializerRegistry()
    {
       await Assert.That(SerializerRegistry<int>.Write).IsNotNull();
       await Assert.That(SerializerRegistry<int>.TryRead).IsNotNull();
       await Assert.That(SerializerRegistry<int>.CalculateByteLength).IsNotNull();

       await Assert.That(SerializerRegistry<bool>.Write).IsNotNull();
       await Assert.That(SerializerRegistry<string?>.Write).IsNotNull();
       await Assert.That(SerializerRegistry<Guid>.Write).IsNotNull();
       await Assert.That(SerializerRegistry<DateTime>.Write).IsNotNull();
       await Assert.That(SerializerRegistry<Uri>.Write).IsNotNull();

       // Dynamically resolved nullables and generics
       await Assert.That(SerializerRegistry<int?>.Write).IsNotNull();
       await Assert.That(SerializerRegistry<int?>.TryRead).IsNotNull();
       await Assert.That(SerializerRegistry<int?>.CalculateByteLength).IsNotNull();

       await Assert.That(SerializerRegistry<List<int>?>.Write).IsNotNull();
       await Assert.That(SerializerRegistry<List<int>?>.TryRead).IsNotNull();
       await Assert.That(SerializerRegistry<List<int>?>.CalculateByteLength).IsNotNull();

       await Assert.That(SerializerRegistry<string[]?>.Write).IsNotNull();
       await Assert.That(SerializerRegistry<Dictionary<string, int>?>.Write).IsNotNull();
       await Assert.That(SerializerRegistry<ConcurrentQueue<double>?>.Write).IsNotNull();
       await Assert.That(SerializerRegistry<ImmutableList<Guid>?>.Write).IsNotNull();
    }

    [Test]
    public async Task TestArraySerializer()
    {
       // 1. Test regular array
       int[] intValues = [10, 20, 30, 40, 50];
       int expectedSize = sizeof(int) + (sizeof(int) * intValues.Length); // 4 + 4*5 = 24
       await TestArray(intValues, expectedSize);

       // 2. Test null array
       int[]? nullArray = null;
       int expectedNullSize = sizeof(int); // 4 bytes for -1
       await TestArray(nullArray, expectedNullSize);

       // 3. Test empty array
       int[] emptyArray = [];
       int expectedEmptySize = sizeof(int); // 4 bytes for 0
       await TestArray(emptyArray, expectedEmptySize);
    }

    private static async Task TestArray(int[]? value, int expectedSize)
    {
       // 1. Calculate length
       var length = ArraySerializer<int>.CalculateByteLength(in value);
       await Assert.That(length).IsEqualTo(expectedSize);
       
       // 2. Write and contiguous read
       byte[] buffer = new byte[expectedSize];
       
       var writer = new BufferWriter<byte>(buffer);
       var written = ArraySerializer<int>.Write(ref writer, in value);
       
       var (readSuccess, readValue, remaining) = ReadContiguous(buffer);
       
       await Assert.That(written).IsEqualTo(expectedSize);
       await Assert.That(readSuccess).IsTrue();
       await Assert.That(remaining).IsEqualTo(0);

       if (value is null)
       {
          await Assert.That(readValue).IsNull();
       }
       else
       {
          await Assert.That(readValue).IsNotNull();
          if (readValue is not null)
          {
             await Assert.That(readValue.Length).IsEqualTo(value.Length);
             for (int i = 0; i < value.Length; i++)
             {
                await Assert.That(readValue[i]).IsEqualTo(value[i]);
             }
          }
       }
       
       // 3. Multi-segment read
       if (expectedSize > 1)
       {
          BufferSegment<byte> firstSegment = new(new byte[] { buffer[0] });
          BufferSegment<byte> currentSegment = firstSegment;
          for (int i = 1; i < expectedSize; i++)
          {
             currentSegment = currentSegment.Append(new byte[] { buffer[i] });
          }
          
          var multiSegmentSequence = new ReadOnlySequence<byte>(firstSegment, 0, currentSegment, 1);
          var (multiReadSuccess, multiReadValue, multiRemaining) = ReadMulti(multiSegmentSequence);
          
          await Assert.That(multiReadSuccess).IsTrue();
          await Assert.That(multiRemaining).IsEqualTo(0);

          if (value is null)
          {
             await Assert.That(multiReadValue).IsNull();
          }
          else
          {
             await Assert.That(multiReadValue).IsNotNull();
             if (multiReadValue is not null)
             {
                await Assert.That(multiReadValue.Length).IsEqualTo(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                   await Assert.That(multiReadValue[i]).IsEqualTo(value[i]);
                }
             }
          }
       }
    }

    private static (bool success, int[]? readValue, long remaining) ReadContiguous(byte[] buffer)
    {
       var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(buffer));
       var success = ArraySerializer<int>.TryRead(ref reader, out int[]? readValue);
       return (success, readValue, reader.Remaining);
    }

    private static (bool success, int[]? readValue, long remaining) ReadMulti(ReadOnlySequence<byte> sequence)
    {
       var reader = new SequenceReader<byte>(sequence);
       var success = ArraySerializer<int>.TryRead(ref reader, out int[]? readValue);
       return (success, readValue, reader.Remaining);
    }

     [Test]
     public async Task TestNullableSerializers()
     {
        // Test nullable int
        int? valInt = 42;
        int? nullInt = null;
        await TestSerializer<int?, NullableSerializer<int>>(valInt, sizeof(bool) + sizeof(int));
        await TestSerializer<int?, NullableSerializer<int>>(nullInt, sizeof(bool));

        // Test nullable Guid
        Guid? valGuid = Guid.NewGuid();
        Guid? nullGuid = null;
        await TestSerializer<Guid?, NullableSerializer<Guid>>(valGuid, sizeof(bool) + 16);
        await TestSerializer<Guid?, NullableSerializer<Guid>>(nullGuid, sizeof(bool));
     }

     [Test]
     public async Task TestLazySerializer()
     {
        // Test lazy int
        Lazy<int>? valLazy = new Lazy<int>(() => 42);
        Lazy<int>? nullLazy = null;
        
        // Test active lazy
        int expectedSize = sizeof(bool) + sizeof(int);
        var length = LazySerializer<int>.CalculateByteLength(in valLazy);
        await Assert.That(length).IsEqualTo(expectedSize);

        byte[] buffer = new byte[expectedSize];
        var writer = new BufferWriter<byte>(buffer);
        var written = LazySerializer<int>.Write(ref writer, in valLazy);
        await Assert.That(written).IsEqualTo(expectedSize);

        var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(buffer));
        var success = LazySerializer<int>.TryRead(ref reader, out var readValue);
        await Assert.That(success).IsTrue();
        await Assert.That(readValue).IsNotNull();
        await Assert.That(readValue!.Value).IsEqualTo(42);

        // Test null lazy
        expectedSize = sizeof(bool);
        length = LazySerializer<int>.CalculateByteLength(in nullLazy);
        await Assert.That(length).IsEqualTo(expectedSize);

        buffer = new byte[expectedSize];
        writer = new BufferWriter<byte>(buffer);
        written = LazySerializer<int>.Write(ref writer, in nullLazy);
        await Assert.That(written).IsEqualTo(expectedSize);

        reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(buffer));
        success = LazySerializer<int>.TryRead(ref reader, out readValue);
        await Assert.That(success).IsTrue();
        await Assert.That(readValue).IsNull();
     }

     [Test]
     public async Task TestBigIntegerSerializer()
     {
        var valZero = BigInteger.Zero;
        var valSmall = new BigInteger(42);
        var valLarge = BigInteger.Parse("123456789012345678901234567890");

        await TestSerializer<BigInteger, BigIntegerSerializer>(valZero, sizeof(int) + valZero.GetByteCount());
        await TestSerializer<BigInteger, BigIntegerSerializer>(valSmall, sizeof(int) + valSmall.GetByteCount());
        await TestSerializer<BigInteger, BigIntegerSerializer>(valLarge, sizeof(int) + valLarge.GetByteCount());
     }

     [Test]
     public async Task TestComplexSerializer()
     {
        var valZero = Complex.Zero;
        var valOne = Complex.One;
        var valCustom = new Complex(3.14, -2.71);

        await TestSerializer<Complex, ComplexSerializer>(valZero, sizeof(double) * 2);
        await TestSerializer<Complex, ComplexSerializer>(valOne, sizeof(double) * 2);
        await TestSerializer<Complex, ComplexSerializer>(valCustom, sizeof(double) * 2);
     }

     public enum TestByteEnum : byte
     {
        A = 1,
        B = 2,
        C = 3
     }

     public enum TestIntEnum : int
     {
        X = 10,
        Y = 20,
        Z = 30
     }

     [Test]
     public async Task TestEnumSerializer()
     {
        await TestSerializer<TestByteEnum, EnumSerializer<TestByteEnum>>(TestByteEnum.B, sizeof(byte));
        await TestSerializer<TestIntEnum, EnumSerializer<TestIntEnum>>(TestIntEnum.Y, sizeof(int));
     }

     [Test]
     public async Task TestStringBuilderSerializer()
     {
        StringBuilder? valSb = new StringBuilder("Hello world! C# 13 is awesome.");
        StringBuilder? nullSb = null;
        StringBuilder? emptySb = new StringBuilder();

        // Test non-null
        int expectedSize = sizeof(int) + System.Text.Encoding.UTF8.GetByteCount(valSb.ToString());
        var length = StringBuilderSerializer.CalculateByteLength(in valSb);
        await Assert.That(length).IsEqualTo(expectedSize);

        byte[] buffer = new byte[expectedSize];
        var writer = new BufferWriter<byte>(buffer);
        var written = StringBuilderSerializer.Write(ref writer, in valSb);
        await Assert.That(written).IsEqualTo(expectedSize);

        var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(buffer));
        var success = StringBuilderSerializer.TryRead(ref reader, out var readValue);
        await Assert.That(success).IsTrue();
        await Assert.That(readValue).IsNotNull();
        await Assert.That(readValue!.ToString()).IsEqualTo(valSb.ToString());

        // Test null
        expectedSize = sizeof(int);
        length = StringBuilderSerializer.CalculateByteLength(in nullSb);
        await Assert.That(length).IsEqualTo(expectedSize);

        buffer = new byte[expectedSize];
        writer = new BufferWriter<byte>(buffer);
        written = StringBuilderSerializer.Write(ref writer, in nullSb);
        await Assert.That(written).IsEqualTo(expectedSize);

        reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(buffer));
        success = StringBuilderSerializer.TryRead(ref reader, out readValue);
        await Assert.That(success).IsTrue();
        await Assert.That(readValue).IsNull();

        // Test empty
        expectedSize = sizeof(int);
        length = StringBuilderSerializer.CalculateByteLength(in emptySb);
        await Assert.That(length).IsEqualTo(expectedSize);

        buffer = new byte[expectedSize];
        writer = new BufferWriter<byte>(buffer);
        written = StringBuilderSerializer.Write(ref writer, in emptySb);
        await Assert.That(written).IsEqualTo(expectedSize);

        reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(buffer));
        success = StringBuilderSerializer.TryRead(ref reader, out readValue);
        await Assert.That(success).IsTrue();
        await Assert.That(readValue).IsNotNull();
        await Assert.That(readValue!.ToString()).IsEmpty();
     }
}

using System.Buffers;
using System.Runtime.CompilerServices;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Formatters.System;
using Me.Memory.Serialization.Formatters.Unmanaged;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Tests;

internal class BufferSegment<T> : ReadOnlySequenceSegment<T>
{
   public BufferSegment(ReadOnlyMemory<T> memory)
   {
      Memory = memory;
   }

   public BufferSegment<T> Append(ReadOnlyMemory<T> memory)
   {
      var nextSegment = new BufferSegment<T>(memory)
      {
         RunningIndex = RunningIndex + Memory.Length
      };
      Next = nextSegment;
      return nextSegment;
   }
}

public class UnmanagedSerializerTests
{
    private static async Task TestSerializer<T, TSerializer>(T value, int expectedSize)
       where T : unmanaged
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
       where T : unmanaged
       where TSerializer : ISerializer<T>
    {
       var writer = new BufferWriter<byte>(buffer);
       var written = TSerializer.Write(ref writer, in value);
       
       var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(buffer));
       var readSuccess = TSerializer.TryRead(ref reader, out T readValue);
       
       return (written, readSuccess, readValue, reader.Remaining);
    }

    private static (bool readSuccess, T readValue, long remaining) ReadMultiSegment<T, TSerializer>(byte[] buffer, int expectedSize)
       where T : unmanaged
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
   public async Task TestBooleanSerializer()
   {
      await TestSerializer<bool, BooleanSerializer>(true, sizeof(bool));
      await TestSerializer<bool, BooleanSerializer>(false, sizeof(bool));
   }

   [Test]
   public async Task TestByteSerializer()
   {
      await TestSerializer<byte, ByteSerializer>(0xAB, sizeof(byte));
      await TestSerializer<byte, ByteSerializer>(0x00, sizeof(byte));
   }

   [Test]
   public async Task TestSByteSerializer()
   {
      await TestSerializer<sbyte, SByteSerializer>(42, sizeof(sbyte));
      await TestSerializer<sbyte, SByteSerializer>(-42, sizeof(sbyte));
   }

   [Test]
   public async Task TestInt16Serializer()
   {
      await TestSerializer<short, Int16Serializer>(12345, sizeof(short));
      await TestSerializer<short, Int16Serializer>(-12345, sizeof(short));
   }

   [Test]
   public async Task TestUInt16Serializer()
   {
      await TestSerializer<ushort, UInt16Serializer>(54321, sizeof(ushort));
      await TestSerializer<ushort, UInt16Serializer>(0, sizeof(ushort));
   }

   [Test]
   public async Task TestInt32Serializer()
   {
      await TestSerializer<int, Int32Serializer>(123456789, sizeof(int));
      await TestSerializer<int, Int32Serializer>(-123456789, sizeof(int));
   }

   [Test]
   public async Task TestUInt32Serializer()
   {
      await TestSerializer<uint, UInt32Serializer>(3000000000, sizeof(uint));
      await TestSerializer<uint, UInt32Serializer>(0, sizeof(uint));
   }

   [Test]
   public async Task TestInt64Serializer()
   {
      await TestSerializer<long, Int64Serializer>(1234567890123456789L, sizeof(long));
      await TestSerializer<long, Int64Serializer>(-1234567890123456789L, sizeof(long));
   }

   [Test]
   public async Task TestUInt64Serializer()
   {
      await TestSerializer<ulong, UInt64Serializer>(18000000000000000000UL, sizeof(ulong));
      await TestSerializer<ulong, UInt64Serializer>(0UL, sizeof(ulong));
   }

   [Test]
   public async Task TestCharSerializer()
   {
      await TestSerializer<char, CharSerializer>('A', sizeof(char));
      await TestSerializer<char, CharSerializer>('\u263A', sizeof(char)); // Smiley face
   }

   [Test]
   public async Task TestSingleSerializer()
   {
      await TestSerializer<float, SingleSerializer>(3.14159f, sizeof(float));
      await TestSerializer<float, SingleSerializer>(-0.00123f, sizeof(float));
   }

   [Test]
   public async Task TestDoubleSerializer()
   {
      await TestSerializer<double, DoubleSerializer>(2.718281828459, sizeof(double));
      await TestSerializer<double, DoubleSerializer>(-1234567.890123, sizeof(double));
   }

   [Test]
   public async Task TestDecimalSerializer()
   {
      await TestSerializer<decimal, DecimalSerializer>(123456.789m, sizeof(decimal));
      await TestSerializer<decimal, DecimalSerializer>(-987654.321m, sizeof(decimal));
   }

   [Test]
   public async Task TestHalfSerializer()
   {
      await TestSerializer<Half, HalfSerializer>((Half)1.5f, Unsafe.SizeOf<Half>());
      await TestSerializer<Half, HalfSerializer>((Half)(-3.14f), Unsafe.SizeOf<Half>());
   }

   [Test]
   public async Task TestStringSerializer()
   {
      await TestString("Hello World", sizeof(int) + 11);
      await TestString("", sizeof(int) + 0);
      await TestString(null!, sizeof(int));
      await TestString("Smiley: \u263A", sizeof(int) + 11);
   }

    private static async Task TestString(string value, int expectedSize)
    {
       // 1. Calculate length
       var length = StringSerializer.CalculateByteLength(in value);
       await Assert.That(length).IsEqualTo(expectedSize);
       
       // 2. Write and contiguous read
       byte[] buffer = new byte[expectedSize];
       
       var (written, readSuccess, readValue, remaining) = WriteStringAndReadContiguous(buffer, value);
       
       await Assert.That(written).IsEqualTo(expectedSize);
       await Assert.That(readSuccess).IsTrue();
       await Assert.That(readValue).IsEqualTo(value);
       await Assert.That(remaining).IsEqualTo(0);
       
       // 3. Multi-segment read (split the written bytes into 1-byte segments)
       if (expectedSize > 1)
       {
          var (multiReadSuccess, multiReadValue, multiRemaining) = ReadStringMultiSegment(buffer, expectedSize);
          await Assert.That(multiReadSuccess).IsTrue();
          await Assert.That(multiReadValue).IsEqualTo(value);
          await Assert.That(multiRemaining).IsEqualTo(0);
       }
    }

    private static (int written, bool readSuccess, string? readValue, long remaining) WriteStringAndReadContiguous(byte[] buffer, string value)
    {
       var writer = new BufferWriter<byte>(buffer);
       var written = StringSerializer.Write(ref writer, in value);
       
       var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(buffer));
       var readSuccess = StringSerializer.TryRead(ref reader, out string? readValue);
       
       return (written, readSuccess, readValue, reader.Remaining);
    }

    private static (bool readSuccess, string? readValue, long remaining) ReadStringMultiSegment(byte[] buffer, int expectedSize)
    {
       BufferSegment<byte> firstSegment = new(new byte[] { buffer[0] });
       BufferSegment<byte> currentSegment = firstSegment;
       for (int i = 1; i < expectedSize; i++)
       {
          currentSegment = currentSegment.Append(new byte[] { buffer[i] });
       }
       
       var multiSegmentSequence = new ReadOnlySequence<byte>(firstSegment, 0, currentSegment, 1);
       var multiSegmentReader = new SequenceReader<byte>(multiSegmentSequence);
       
       var multiReadSuccess = StringSerializer.TryRead(ref multiSegmentReader, out string? multiReadValue);
       return (multiReadSuccess, multiReadValue, multiSegmentReader.Remaining);
    }
}


#pragma warning disable CS8600, CS8604, CS8619

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Me.Memory.Buffers;
using Me.Memory.Serialization;
using Me.Memory.Serialization.Attributes;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Tests;

[GenerateSerializer]
public class SimpleTestClass
{
   [SerializerPosition(0)]
   public int Id { get; set; }

   [SerializerPosition(1)]
   public string? Name { get; set; }
}

[GenerateSerializer]
public class IgnoreTestClass
{
   [SerializerPosition(0)]
   public int Id { get; set; }

   [SerializerPosition(1)]
   [SerializerIgnore]
   public string? IgnoredProperty { get; set; }

   [SerializerPosition(2)]
   public bool Flag { get; set; }
}

public abstract class ReverseStringSerializer : ISerializer<string?>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in string? value)
   {
      if (value is null)
      {
         return Me.Memory.Serialization.Formatters.System.StringSerializer.Write(ref writer, null);
      }
      char[] charArray = value.ToCharArray();
      Array.Reverse(charArray);
      return Me.Memory.Serialization.Formatters.System.StringSerializer.Write(ref writer, new string(charArray));
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out string? value)
   {
      if (!Me.Memory.Serialization.Formatters.System.StringSerializer.TryRead(ref reader, out var raw))
      {
         value = null;
         return false;
      }
      if (raw is null)
      {
         value = null;
         return true;
      }
      char[] charArray = raw.ToCharArray();
      Array.Reverse(charArray);
      value = new string(charArray);
      return true;
   }

   public static int CalculateByteLength(scoped in string? value)
   {
      return Me.Memory.Serialization.Formatters.System.StringSerializer.CalculateByteLength(value);
   }
}

[GenerateSerializer]
public class CustomSerializerTestClass
{
   [SerializerPosition(0)]
   public int Id { get; set; }

   [SerializerPosition(1)]
   [UseSerializer(typeof(ReverseStringSerializer))]
   public string? ReversedName { get; set; }
}

[GenerateSerializer]
public struct SimpleTestStruct
{
   [SerializerPosition(0)]
   public double Value { get; set; }

   [SerializerPosition(1)]
   public bool Flag { get; set; }
}

[GenerateSerializer]
[SerializerUnion(1, typeof(DerivedA))]
[SerializerUnion(2, typeof(DerivedB))]
public abstract class BaseTestClass
{
   [SerializerPosition(0)]
   public string? Code { get; set; }
}

[GenerateSerializer]
public class DerivedA : BaseTestClass
{
   [SerializerPosition(1)]
   public int ValueA { get; set; }
}

[GenerateSerializer]
public class DerivedB : BaseTestClass
{
   [SerializerPosition(1)]
   public string? ValueB { get; set; }
}

public class PolymorphicSerializerTests
{
   private static async Task TestSerializerDirect<T, TSerializer>(T value, int expectedSize, Func<T, T, Task> assertEqual)
      where TSerializer : ISerializer<T>
   {
      // 1. Calculate length
      var length = TSerializer.CalculateByteLength(in value);
      await Assert.That(length).IsEqualTo(expectedSize);
      
      // 2. Write and contiguous read
      byte[] buffer = new byte[expectedSize];
      var writer = new BufferWriter<byte>(buffer);
      var written = TSerializer.Write(ref writer, in value);
      await Assert.That(written).IsEqualTo(expectedSize);
      
      var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(buffer));
      var readSuccess = TSerializer.TryRead(ref reader, out T readValue);
      await Assert.That(readSuccess).IsTrue();
      await assertEqual(readValue, value);
   }

   [Test]
   public async Task TestSimpleClass()
   {
      var obj = new SimpleTestClass { Id = 42, Name = "Test" };
      // size = presence(1) + Id(4) + string(presence(4) + utf8(4)) = 13
      int size = 1 + sizeof(int) + (sizeof(int) + 4);
      await TestSerializerDirect<SimpleTestClass?, SimpleTestClassSerializer>(obj, size, async (read, expected) =>
      {
         await Assert.That(read).IsNotNull();
         await Assert.That(read!.Id).IsEqualTo(expected!.Id);
         await Assert.That(read.Name).IsEqualTo(expected.Name);
      });

      // Null case
      int nullSize = 1; // presence(1)
      await TestSerializerDirect<SimpleTestClass?, SimpleTestClassSerializer>(null, nullSize, async (read, expected) =>
      {
         await Assert.That(read).IsNull();
      });
   }

   [Test]
   public async Task TestSimpleStruct()
   {
      var obj = new SimpleTestStruct { Value = 3.14, Flag = true };
      // size = Value(8) + Flag(1) = 9
      int size = sizeof(double) + sizeof(bool);
      await TestSerializerDirect<SimpleTestStruct, SimpleTestStructSerializer>(obj, size, async (read, expected) =>
      {
         await Assert.That(read.Value).IsEqualTo(expected.Value);
         await Assert.That(read.Flag).IsEqualTo(expected.Flag);
      });
   }

   [Test]
   public async Task TestPolymorphism()
   {
      // 1. DerivedA instance
      BaseTestClass objA = new DerivedA { Code = "A", ValueA = 100 };
      // size = unionTag(4) + ( presence(1) + DerivedCode(presence(4) + utf8(1)) + ValueA(4) )
      // Wait: DerivedA has Code (position 0) and ValueA (position 1)
      // Presence for DerivedA itself is sizeof(bool) = 1 (simple reference type presence prefix)
      // Then Code (sizeof(int) + 1 = 5)
      // Then ValueA (sizeof(int) = 4)
      // So total size = 4 (tag) + ( 1 (presence) + 5 (Code) + 4 (ValueA) ) = 14
      int sizeA = sizeof(int) + (sizeof(bool) + (sizeof(int) + 1) + sizeof(int));
      await TestSerializerDirect<BaseTestClass?, BaseTestClassSerializer>(objA, sizeA, async (read, expected) =>
      {
         await Assert.That(read).IsNotNull();
         await Assert.That(read is DerivedA).IsTrue();
         var readA = (DerivedA)read!;
         await Assert.That(readA.Code).IsEqualTo(expected!.Code);
         await Assert.That(readA.ValueA).IsEqualTo(((DerivedA)expected).ValueA);
      });

      // 2. DerivedB instance
      BaseTestClass objB = new DerivedB { Code = "B", ValueB = "Test" };
      // size = unionTag(4) + ( presence(1) + Code(5) + ValueB(8) )
      // So total size = 4 (tag) + ( 1 (presence) + 5 (Code) + 8 (ValueB) ) = 18
      int sizeB = sizeof(int) + (sizeof(bool) + (sizeof(int) + 1) + (sizeof(int) + 4));
      await TestSerializerDirect<BaseTestClass?, BaseTestClassSerializer>(objB, sizeB, async (read, expected) =>
      {
         await Assert.That(read).IsNotNull();
         await Assert.That(read is DerivedB).IsTrue();
         var readB = (DerivedB)read!;
         await Assert.That(readB.Code).IsEqualTo(expected!.Code);
         await Assert.That(readB.ValueB).IsEqualTo(((DerivedB)expected).ValueB);
      });

      // 3. Null base test class
      int nullSize = sizeof(int); // tag = -1
      await TestSerializerDirect<BaseTestClass?, BaseTestClassSerializer>(null, nullSize, async (read, expected) =>
      {
         await Assert.That(read).IsNull();
      });
   }

   [Test]
   public async Task TestHighLevelSerializerUtility()
   {
      var obj = new SimpleTestClass { Id = 123, Name = "HighLevel" };
      
      // Serialize using high-level utility
      byte[] bytes = MeSerializer.Serialize<SimpleTestClass?>(obj);
      
      // Verify size matches expected SimpleTestClass size
      // presence(1) + Id(4) + string(presence(4) + utf8(9)) = 18
      int expectedSize = 1 + sizeof(int) + (sizeof(int) + 9);
      await Assert.That(bytes.Length).IsEqualTo(expectedSize);
      
      // Deserialize using high-level utility
      var success = MeSerializer.TryDeserialize<SimpleTestClass?>(bytes, out var result);
      
      await Assert.That(success).IsTrue();
      await Assert.That(result).IsNotNull();
      await Assert.That(result!.Id).IsEqualTo(obj.Id);
      await Assert.That(result.Name).IsEqualTo(obj.Name);
   }

   [Test]
   public async Task TestSerializerIgnore()
   {
      var obj = new IgnoreTestClass
      {
         Id = 42,
         IgnoredProperty = "Should be ignored",
         Flag = true
      };

      // Since IgnoredProperty has [SerializerIgnore], only Id and Flag should be serialized.
      // Expected size:
      // presence(1) + Id(4) + Flag(1) = 6 bytes
      int expectedSize = 1 + sizeof(int) + sizeof(bool);
      
      await TestSerializerDirect<IgnoreTestClass?, IgnoreTestClassSerializer>(obj, expectedSize, async (read, expected) =>
      {
         await Assert.That(read).IsNotNull();
         await Assert.That(read!.Id).IsEqualTo(expected!.Id);
         await Assert.That(read.Flag).IsEqualTo(expected.Flag);
         // IgnoredProperty should be null in the deserialized object because it wasn't serialized
         await Assert.That(read.IgnoredProperty).IsNull();
      });
   }

   [Test]
   public async Task TestCustomSerializer()
   {
      var obj = new CustomSerializerTestClass
      {
         Id = 42,
         ReversedName = "Hello"
      };

      // Since ReversedName uses ReverseStringSerializer, "Hello" should be reversed to "olleH" on serialization.
      // Expected size:
      // presence(1) + Id(4) + string(presence(4) + utf8(5)) = 14 bytes
      int expectedSize = 1 + sizeof(int) + (sizeof(int) + 5);
      
      await TestSerializerDirect<CustomSerializerTestClass?, CustomSerializerTestClassSerializer>(obj, expectedSize, async (read, expected) =>
      {
         await Assert.That(read).IsNotNull();
         await Assert.That(read!.Id).IsEqualTo(expected!.Id);
         await Assert.That(read.ReversedName).IsEqualTo(expected.ReversedName); // Deserializes and reverses "olleH" back to "Hello"
      });

      // Directly verify that the raw bytes in the buffer actually contain "olleH" reversed!
      byte[] buffer = new byte[expectedSize];
      var writer = new BufferWriter<byte>(buffer);
      var written = CustomSerializerTestClassSerializer.Write(ref writer, obj);
      await Assert.That(written).IsEqualTo(expectedSize);
      
      // Index 0: presence(1)
      // Index 1-4: Id (42 as little endian int) -> 42, 0, 0, 0
      // Index 5-8: string presence length (5 as little endian int) -> 5, 0, 0, 0
      // Index 9-13: "olleH" bytes
      var stringBytes = buffer[9..];
      var rawString = System.Text.Encoding.UTF8.GetString(stringBytes);
      await Assert.That(rawString).IsEqualTo("olleH");
   }
}

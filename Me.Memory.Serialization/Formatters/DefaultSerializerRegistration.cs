#pragma warning disable CA2255

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Me.Memory.Serialization.Formatters.System;
using Me.Memory.Serialization.Formatters.Unmanaged;

namespace Me.Memory.Serialization.Formatters;

internal static class DefaultSerializerRegistration
{
   [ModuleInitializer]
   internal static void RegisterDefaults()
   {
      // Boolean
      SerializerRegistry<bool>.Write = BooleanSerializer.Write;
      SerializerRegistry<bool>.TryRead = BooleanSerializer.TryRead;
      SerializerRegistry<bool>.CalculateByteLength = BooleanSerializer.CalculateByteLength;

      // Byte
      SerializerRegistry<byte>.Write = ByteSerializer.Write;
      SerializerRegistry<byte>.TryRead = ByteSerializer.TryRead;
      SerializerRegistry<byte>.CalculateByteLength = ByteSerializer.CalculateByteLength;

      // Char
      SerializerRegistry<char>.Write = CharSerializer.Write;
      SerializerRegistry<char>.TryRead = CharSerializer.TryRead;
      SerializerRegistry<char>.CalculateByteLength = CharSerializer.CalculateByteLength;

      // Decimal
      SerializerRegistry<decimal>.Write = DecimalSerializer.Write;
      SerializerRegistry<decimal>.TryRead = DecimalSerializer.TryRead;
      SerializerRegistry<decimal>.CalculateByteLength = DecimalSerializer.CalculateByteLength;

      // Double
      SerializerRegistry<double>.Write = DoubleSerializer.Write;
      SerializerRegistry<double>.TryRead = DoubleSerializer.TryRead;
      SerializerRegistry<double>.CalculateByteLength = DoubleSerializer.CalculateByteLength;

      // Half
      SerializerRegistry<Half>.Write = HalfSerializer.Write;
      SerializerRegistry<Half>.TryRead = HalfSerializer.TryRead;
      SerializerRegistry<Half>.CalculateByteLength = HalfSerializer.CalculateByteLength;

      // Int16 (short)
      SerializerRegistry<short>.Write = Int16Serializer.Write;
      SerializerRegistry<short>.TryRead = Int16Serializer.TryRead;
      SerializerRegistry<short>.CalculateByteLength = Int16Serializer.CalculateByteLength;

      // Int32 (int)
      SerializerRegistry<int>.Write = Int32Serializer.Write;
      SerializerRegistry<int>.TryRead = Int32Serializer.TryRead;
      SerializerRegistry<int>.CalculateByteLength = Int32Serializer.CalculateByteLength;

      // Int64 (long)
      SerializerRegistry<long>.Write = Int64Serializer.Write;
      SerializerRegistry<long>.TryRead = Int64Serializer.TryRead;
      SerializerRegistry<long>.CalculateByteLength = Int64Serializer.CalculateByteLength;

      // SByte (sbyte)
      SerializerRegistry<sbyte>.Write = SByteSerializer.Write;
      SerializerRegistry<sbyte>.TryRead = SByteSerializer.TryRead;
      SerializerRegistry<sbyte>.CalculateByteLength = SByteSerializer.CalculateByteLength;

      // Single (float)
      SerializerRegistry<float>.Write = SingleSerializer.Write;
      SerializerRegistry<float>.TryRead = SingleSerializer.TryRead;
      SerializerRegistry<float>.CalculateByteLength = SingleSerializer.CalculateByteLength;

      // UInt16 (ushort)
      SerializerRegistry<ushort>.Write = UInt16Serializer.Write;
      SerializerRegistry<ushort>.TryRead = UInt16Serializer.TryRead;
      SerializerRegistry<ushort>.CalculateByteLength = UInt16Serializer.CalculateByteLength;

      // UInt32 (uint)
      SerializerRegistry<uint>.Write = UInt32Serializer.Write;
      SerializerRegistry<uint>.TryRead = UInt32Serializer.TryRead;
      SerializerRegistry<uint>.CalculateByteLength = UInt32Serializer.CalculateByteLength;

      // UInt64 (ulong)
      SerializerRegistry<ulong>.Write = UInt64Serializer.Write;
      SerializerRegistry<ulong>.TryRead = UInt64Serializer.TryRead;
      SerializerRegistry<ulong>.CalculateByteLength = UInt64Serializer.CalculateByteLength;

      // String (string?)
      SerializerRegistry<string?>.Write = StringSerializer.Write;
      SerializerRegistry<string?>.TryRead = StringSerializer.TryRead;
      SerializerRegistry<string?>.CalculateByteLength = StringSerializer.CalculateByteLength;

      // TimeSpan
      SerializerRegistry<TimeSpan>.Write = TimeSpanSerializer.Write;
      SerializerRegistry<TimeSpan>.TryRead = TimeSpanSerializer.TryRead;
      SerializerRegistry<TimeSpan>.CalculateByteLength = TimeSpanSerializer.CalculateByteLength;

      // DateTime
      SerializerRegistry<DateTime>.Write = DateTimeSerializer.Write;
      SerializerRegistry<DateTime>.TryRead = DateTimeSerializer.TryRead;
      SerializerRegistry<DateTime>.CalculateByteLength = DateTimeSerializer.CalculateByteLength;

      // DateTimeOffset
      SerializerRegistry<DateTimeOffset>.Write = DateTimeOffsetSerializer.Write;
      SerializerRegistry<DateTimeOffset>.TryRead = DateTimeOffsetSerializer.TryRead;
      SerializerRegistry<DateTimeOffset>.CalculateByteLength = DateTimeOffsetSerializer.CalculateByteLength;

      // Guid
      SerializerRegistry<Guid>.Write = GuidSerializer.Write;
      SerializerRegistry<Guid>.TryRead = GuidSerializer.TryRead;
      SerializerRegistry<Guid>.CalculateByteLength = GuidSerializer.CalculateByteLength;

      // Uri
      SerializerRegistry<Uri>.Write = UriSerializer.Write;
      SerializerRegistry<Uri>.TryRead = UriSerializer.TryRead;
      SerializerRegistry<Uri>.CalculateByteLength = UriSerializer.CalculateByteLength;

      // TimeOnly
      SerializerRegistry<TimeOnly>.Write = TimeOnlySerializer.Write;
      SerializerRegistry<TimeOnly>.TryRead = TimeOnlySerializer.TryRead;
      SerializerRegistry<TimeOnly>.CalculateByteLength = TimeOnlySerializer.CalculateByteLength;

      // DateOnly
      SerializerRegistry<DateOnly>.Write = DateOnlySerializer.Write;
      SerializerRegistry<DateOnly>.TryRead = DateOnlySerializer.TryRead;
      SerializerRegistry<DateOnly>.CalculateByteLength = DateOnlySerializer.CalculateByteLength;

      // BigInteger
      SerializerRegistry<BigInteger>.Write = BigIntegerSerializer.Write;
      SerializerRegistry<BigInteger>.TryRead = BigIntegerSerializer.TryRead;
      SerializerRegistry<BigInteger>.CalculateByteLength = BigIntegerSerializer.CalculateByteLength;

      // Complex
      SerializerRegistry<Complex>.Write = ComplexSerializer.Write;
      SerializerRegistry<Complex>.TryRead = ComplexSerializer.TryRead;
      SerializerRegistry<Complex>.CalculateByteLength = ComplexSerializer.CalculateByteLength;

      // StringBuilder
      SerializerRegistry<StringBuilder?>.Write = StringBuilderSerializer.Write;
      SerializerRegistry<StringBuilder?>.TryRead = StringBuilderSerializer.TryRead;
      SerializerRegistry<StringBuilder?>.CalculateByteLength = StringBuilderSerializer.CalculateByteLength;
   }
}

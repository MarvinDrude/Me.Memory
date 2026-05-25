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
      // Unmanaged
      Register<bool>(BooleanSerializer.Write, BooleanSerializer.TryRead, BooleanSerializer.CalculateByteLength);
      Register<byte>(ByteSerializer.Write, ByteSerializer.TryRead, ByteSerializer.CalculateByteLength);
      Register<char>(CharSerializer.Write, CharSerializer.TryRead, CharSerializer.CalculateByteLength);
      Register<decimal>(DecimalSerializer.Write, DecimalSerializer.TryRead, DecimalSerializer.CalculateByteLength);
      Register<double>(DoubleSerializer.Write, DoubleSerializer.TryRead, DoubleSerializer.CalculateByteLength);
      Register<Half>(HalfSerializer.Write, HalfSerializer.TryRead, HalfSerializer.CalculateByteLength);
      Register<short>(Int16Serializer.Write, Int16Serializer.TryRead, Int16Serializer.CalculateByteLength);
      Register<int>(Int32Serializer.Write, Int32Serializer.TryRead, Int32Serializer.CalculateByteLength);
      Register<long>(Int64Serializer.Write, Int64Serializer.TryRead, Int64Serializer.CalculateByteLength);
      Register<sbyte>(SByteSerializer.Write, SByteSerializer.TryRead, SByteSerializer.CalculateByteLength);
      Register<float>(SingleSerializer.Write, SingleSerializer.TryRead, SingleSerializer.CalculateByteLength);
      Register<ushort>(UInt16Serializer.Write, UInt16Serializer.TryRead, UInt16Serializer.CalculateByteLength);
      Register<uint>(UInt32Serializer.Write, UInt32Serializer.TryRead, UInt32Serializer.CalculateByteLength);
      Register<ulong>(UInt64Serializer.Write, UInt64Serializer.TryRead, UInt64Serializer.CalculateByteLength);

      // System
      Register<string?>(StringSerializer.Write, StringSerializer.TryRead, StringSerializer.CalculateByteLength);
      Register<TimeSpan>(TimeSpanSerializer.Write, TimeSpanSerializer.TryRead, TimeSpanSerializer.CalculateByteLength);
      Register<DateTime>(DateTimeSerializer.Write, DateTimeSerializer.TryRead, DateTimeSerializer.CalculateByteLength);
      Register<DateTimeOffset>(DateTimeOffsetSerializer.Write, DateTimeOffsetSerializer.TryRead, DateTimeOffsetSerializer.CalculateByteLength);
      Register<Guid>(GuidSerializer.Write, GuidSerializer.TryRead, GuidSerializer.CalculateByteLength);
      Register<Uri>(UriSerializer.Write, UriSerializer.TryRead, UriSerializer.CalculateByteLength);
      Register<TimeOnly>(TimeOnlySerializer.Write, TimeOnlySerializer.TryRead, TimeOnlySerializer.CalculateByteLength);
      Register<DateOnly>(DateOnlySerializer.Write, DateOnlySerializer.TryRead, DateOnlySerializer.CalculateByteLength);
      Register<BigInteger>(BigIntegerSerializer.Write, BigIntegerSerializer.TryRead, BigIntegerSerializer.CalculateByteLength);
      Register<Complex>(ComplexSerializer.Write, ComplexSerializer.TryRead, ComplexSerializer.CalculateByteLength);
      Register<StringBuilder?>(StringBuilderSerializer.Write, StringBuilderSerializer.TryRead, StringBuilderSerializer.CalculateByteLength);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static void Register<T>(WriteDelegate<T> write, TryReadDelegate<T> tryRead, CalculateByteLengthDelegate<T> calculateByteLength)
      where T : allows ref struct
   {
      SerializerRegistry<T>.Write = write;
      SerializerRegistry<T>.TryRead = tryRead;
      SerializerRegistry<T>.CalculateByteLength = calculateByteLength;
   }
}

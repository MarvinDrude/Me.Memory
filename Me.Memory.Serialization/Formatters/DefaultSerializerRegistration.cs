#pragma warning disable CA2255

using System.Runtime.CompilerServices;
using Me.Memory.Serialization.Formatters.System;
using Me.Memory.Serialization.Formatters.Unmanaged;

namespace Me.Memory.Serialization.Formatters;

internal static class DefaultSerializerRegistration
{
   [ModuleInitializer]
   internal static void RegisterDefaults()
   {
      // Unmanaged / Primitive serializers
      SerializerRegistry<bool>.Instance = new StaticSerializerWrapper<bool, BooleanSerializer>();
      SerializerRegistry<byte>.Instance = new StaticSerializerWrapper<byte, ByteSerializer>();
      SerializerRegistry<char>.Instance = new StaticSerializerWrapper<char, CharSerializer>();
      SerializerRegistry<decimal>.Instance = new StaticSerializerWrapper<decimal, DecimalSerializer>();
      SerializerRegistry<double>.Instance = new StaticSerializerWrapper<double, DoubleSerializer>();
      SerializerRegistry<Half>.Instance = new StaticSerializerWrapper<Half, HalfSerializer>();
      SerializerRegistry<short>.Instance = new StaticSerializerWrapper<short, Int16Serializer>();
      SerializerRegistry<int>.Instance = new StaticSerializerWrapper<int, Int32Serializer>();
      SerializerRegistry<long>.Instance = new StaticSerializerWrapper<long, Int64Serializer>();
      SerializerRegistry<sbyte>.Instance = new StaticSerializerWrapper<sbyte, SByteSerializer>();
      SerializerRegistry<float>.Instance = new StaticSerializerWrapper<float, SingleSerializer>();
      SerializerRegistry<ushort>.Instance = new StaticSerializerWrapper<ushort, UInt16Serializer>();
      SerializerRegistry<uint>.Instance = new StaticSerializerWrapper<uint, UInt32Serializer>();
      SerializerRegistry<ulong>.Instance = new StaticSerializerWrapper<ulong, UInt64Serializer>();

      // System serializers
      SerializerRegistry<string?>.Instance = new StaticSerializerWrapper<string?, StringSerializer>();
      SerializerRegistry<TimeSpan>.Instance = new StaticSerializerWrapper<TimeSpan, TimeSpanSerializer>();
      SerializerRegistry<DateTime>.Instance = new StaticSerializerWrapper<DateTime, DateTimeSerializer>();
      SerializerRegistry<DateTimeOffset>.Instance = new StaticSerializerWrapper<DateTimeOffset, DateTimeOffsetSerializer>();
      SerializerRegistry<Guid>.Instance = new StaticSerializerWrapper<Guid, GuidSerializer>();
      SerializerRegistry<Uri>.Instance = new StaticSerializerWrapper<Uri, UriSerializer>();
      SerializerRegistry<TimeOnly>.Instance = new StaticSerializerWrapper<TimeOnly, TimeOnlySerializer>();
      SerializerRegistry<DateOnly>.Instance = new StaticSerializerWrapper<DateOnly, DateOnlySerializer>();
   }
}

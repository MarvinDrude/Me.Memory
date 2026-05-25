using System.Buffers;
using System.Runtime.CompilerServices;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.System;

internal interface IEnumHelper<T>
{
   int Write(ref BufferWriter<byte> writer, scoped in T value);
   
   bool TryRead(ref SequenceReader<byte> reader, out T value);
   
   int CalculateByteLength(scoped in T value);
}

internal sealed class EnumHelper<T, TUnderlying> : IEnumHelper<T>
   where T : struct, Enum
   where TUnderlying : struct
{
   public int Write(ref BufferWriter<byte> writer, scoped in T value)
   {
      var underlying = Unsafe.As<T, TUnderlying>(ref Unsafe.AsRef(in value));
      return SerializerRegistry<TUnderlying>.GetWrite()(ref writer, underlying);
   }

   public bool TryRead(ref SequenceReader<byte> reader, out T value)
   {
      var success = SerializerRegistry<TUnderlying>.GetTryRead()(ref reader, out var underlying);
      value = Unsafe.As<TUnderlying, T>(ref underlying);
      
      return success;
   }

   public int CalculateByteLength(scoped in T value)
   {
      var underlying = Unsafe.As<T, TUnderlying>(ref Unsafe.AsRef(in value));
      return SerializerRegistry<TUnderlying>.GetCalculateByteLength()(underlying);
   }
}

/// <summary>
/// Serializer for Enum values.
/// </summary>
public abstract class EnumSerializer<T> : ISerializer<T>
   where T : struct, Enum
{
   private static readonly IEnumHelper<T> _helper;

   static EnumSerializer()
   {
      var underlyingType = typeof(T).GetEnumUnderlyingType();
      var helperType = typeof(EnumHelper<,>).MakeGenericType(typeof(T), underlyingType);
      
      _helper = (IEnumHelper<T>)Activator.CreateInstance(helperType)!;
   }

   public static int Write(ref BufferWriter<byte> writer, scoped in T value)
   {
      return _helper.Write(ref writer, in value);
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out T value)
   {
      return _helper.TryRead(ref reader, out value);
   }

   public static int CalculateByteLength(scoped in T value)
   {
      return _helper.CalculateByteLength(in value);
   }
}

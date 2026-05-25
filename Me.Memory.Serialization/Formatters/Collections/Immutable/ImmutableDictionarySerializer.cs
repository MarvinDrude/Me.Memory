using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.Immutable;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Collections.Immutable;

/// <summary>
/// Serializer for ImmutableDictionary values.
/// </summary>
public abstract class ImmutableDictionarySerializer<TKey, TValue> : ISerializer<ImmutableDictionary<TKey, TValue>?>
   where TKey : notnull
{
   public static int Write(ref BufferWriter<byte> writer, scoped in ImmutableDictionary<TKey, TValue>? value)
   {
      if (value is null)
      {
         var lengthSpan = writer.AcquireSpan(sizeof(int));
         BinaryPrimitives.WriteInt32LittleEndian(lengthSpan, -1);
         
         return sizeof(int);
      }

      var writeKey = SerializerRegistry<TKey>.GetWrite();
      var writeValue = SerializerRegistry<TValue>.GetWrite();

      var lengthSpanNotNil = writer.AcquireSpan(sizeof(int));
      BinaryPrimitives.WriteInt32LittleEndian(lengthSpanNotNil, value.Count);

      var written = sizeof(int);
      foreach (var pair in value)
      {
         written += writeKey(ref writer, pair.Key);
         written += writeValue(ref writer, pair.Value);
      }

      return written;
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out ImmutableDictionary<TKey, TValue>? value)
   {
      if (!reader.TryReadLittleEndian(out int length))
      {
         value = null;
         return false;
      }

      if (length < 0)
      {
         value = null;
         return true;
      }

      var tryReadKey = SerializerRegistry<TKey>.GetTryRead();
      var tryReadValue = SerializerRegistry<TValue>.GetTryRead();
      var builder = ImmutableDictionary.CreateBuilder<TKey, TValue>();

      for (var i = 0; i < length; i++)
      {
         if (!tryReadKey(ref reader, out var key))
         {
            value = null;
            return false;
         }
         
         if (!tryReadValue(ref reader, out var val))
         {
            value = null;
            return false;
         }
         
         builder.Add(key, val);
      }

      value = builder.ToImmutable();
      return true;
   }

   public static int CalculateByteLength(scoped in ImmutableDictionary<TKey, TValue>? value)
   {
      if (value is null)
      {
         return sizeof(int);
      }

      var calculateKey = SerializerRegistry<TKey>.GetCalculateByteLength();
      var calculateValue = SerializerRegistry<TValue>.GetCalculateByteLength();

      var length = sizeof(int);
      foreach (var pair in value)
      {
         length += calculateKey(pair.Key);
         length += calculateValue(pair.Value);
      }

      return length;
   }
}

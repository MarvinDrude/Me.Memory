using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.System;

/// <summary>
/// Serializer for nullable value types.
/// </summary>
public abstract class NullableSerializer<T> : ISerializer<T?>
   where T : struct
{
   public static int Write(ref BufferWriter<byte> writer, scoped in T? value)
   {
      if (!value.HasValue)
      {
         var span = writer.AcquireSpan(sizeof(bool));
         span[0] = 0; // false
         return sizeof(bool);
      }

      var writeElement = SerializerRegistry<T>.GetWrite();
      var hasValueSpan = writer.AcquireSpan(sizeof(bool));
      hasValueSpan[0] = 1; // true

      return sizeof(bool) + writeElement(ref writer, value.Value);
   }

   public static bool TryRead(ref SequenceReader<byte> reader, [MaybeNullWhen(false)] out T? value)
   {
      if (reader.Remaining < sizeof(bool))
      {
         value = null;
         return false;
      }

      var hasValue = reader.UnreadSpan[0] != 0;
      reader.Advance(sizeof(bool));

      if (!hasValue)
      {
         value = null;
         return true;
      }

      var tryReadElement = SerializerRegistry<T>.GetTryRead();
      if (!tryReadElement(ref reader, out T element))
      {
         value = null;
         return false;
      }

      value = element;
      return true;
   }

   public static int CalculateByteLength(scoped in T? value)
   {
      if (!value.HasValue)
      {
         return sizeof(bool);
      }

      var calculateElement = SerializerRegistry<T>.GetCalculateByteLength();
      return sizeof(bool) + calculateElement(value.Value);
   }
}

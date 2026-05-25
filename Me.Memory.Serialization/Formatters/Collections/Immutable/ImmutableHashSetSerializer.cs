using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Immutable;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Collections.Immutable;

/// <summary>
/// Serializer for ImmutableHashSet values.
/// </summary>
public abstract class ImmutableHashSetSerializer<T> : ISerializer<ImmutableHashSet<T>?>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in ImmutableHashSet<T>? value)
   {
      if (value is null)
      {
         var lengthSpan = writer.AcquireSpan(sizeof(int));
         BinaryPrimitives.WriteInt32LittleEndian(lengthSpan, -1);
         
         return sizeof(int);
      }

      var writeElement = SerializerRegistry<T>.GetWrite();
      var lengthSpanNotNil = writer.AcquireSpan(sizeof(int));
      BinaryPrimitives.WriteInt32LittleEndian(lengthSpanNotNil, value.Count);

      var written = sizeof(int);
      foreach (var item in value)
      {
         written += writeElement(ref writer, item);
      }

      return written;
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out ImmutableHashSet<T>? value)
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

      var tryReadElement = SerializerRegistry<T>.GetTryRead();
      var builder = ImmutableHashSet.CreateBuilder<T>();

      for (int i = 0; i < length; i++)
      {
         if (!tryReadElement(ref reader, out var element))
         {
            value = null;
            return false;
         }
         builder.Add(element);
      }

      value = builder.ToImmutable();
      return true;
   }

   public static int CalculateByteLength(scoped in ImmutableHashSet<T>? value)
   {
      if (value is null)
      {
         return sizeof(int);
      }

      var calculateElement = SerializerRegistry<T>.GetCalculateByteLength();
      var length = sizeof(int);
      foreach (var item in value)
      {
         length += calculateElement(item);
      }

      return length;
   }
}

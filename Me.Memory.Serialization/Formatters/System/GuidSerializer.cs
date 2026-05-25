using System.Buffers;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.System;

/// <summary>
/// Serializer for Guid values.
/// </summary>
public abstract class GuidSerializer : ISerializer<Guid>
{
   public static int Write(ref BufferWriter<byte> writer, scoped in Guid value)
   {
      var span = writer.AcquireSpan(16);
      value.TryWriteBytes(span);
      
      return 16;
   }

   public static bool TryRead(ref SequenceReader<byte> reader, out Guid value)
   {
      if (reader.UnreadSpan.Length >= 16)
      {
         value = new Guid(reader.UnreadSpan[..16]);
         reader.Advance(16);
         
         return true;
      }
      
      if (reader.Remaining >= 16)
      {
         Span<byte> bytes = stackalloc byte[16];
         reader.UnreadSequence.Slice(0, 16).CopyTo(bytes);
         value = new Guid(bytes);
         reader.Advance(16);
         
         return true;
      }

      value = default;
      return false;
   }

   public static int CalculateByteLength(scoped in Guid value)
   {
      return 16;
   }
}

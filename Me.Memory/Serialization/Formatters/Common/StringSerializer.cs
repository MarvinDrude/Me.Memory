using System.Diagnostics.CodeAnalysis;
using System.Text;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Common;

public sealed class StringSerializer : ISerializer<string>
{
   private static readonly Encoding _encoding = Encoding.UTF8;
   
   public void Write(ref ByteWriter writer, ref string value)
   {
      var span = value.AsSpan();
      var length = _encoding.GetByteCount(span);

      writer.WriteLittleEndian(length);
      if (length > 0)
      {
         writer.WriteString(span, _encoding);
      }
   }

   public bool TryRead(ref ByteReader reader, [MaybeNullWhen(false)] out string value)
   {
      var length = reader.ReadLittleEndian<int>();
      
      value = length > 0 
         ? reader.ReadString(length, _encoding) 
         : string.Empty;
      
      return true;
   }
}
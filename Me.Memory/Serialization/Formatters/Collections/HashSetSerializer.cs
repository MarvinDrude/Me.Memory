using System.Diagnostics.CodeAnalysis;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Collections;

public sealed class HashSetSerializer<T> : ISerializer<HashSet<T>>
{
   private static ISerializer<T> ItemSerializer => SerializerCache<T>.Instance;
   
   public void Write(ref ByteWriter writer, ref HashSet<T> value)
   {
      var itemSerializer = ItemSerializer;
      writer.WriteLittleEndian(value.Count);
      
      foreach (var current in value)
      {
         var copy = current;
         itemSerializer.Write(ref writer, ref copy);
      }
   }

   public bool TryRead(ref ByteReader reader, [MaybeNullWhen(false)] out HashSet<T> value)
   {
      var itemSerializer = ItemSerializer;
      
      var count = reader.ReadLittleEndian<int>();
      value = new HashSet<T>(count);

      for (var i = 0; i < count; i++)
      {
         if (!itemSerializer.TryRead(ref reader, out var item))
         {
            value = null;
            return false;
         }
         
         value.Add(item);
      }

      return true;
   }
}
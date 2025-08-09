using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Collections;

public sealed class ListSerializer<T> : ISerializer<List<T>>
{
   private static ISerializer<T> ItemSerializer => SerializerCache<T>.Instance;
   
   public void Write(ref ByteWriter writer, ref List<T> value)
   {
      var itemSerializer = ItemSerializer;
      writer.WriteLittleEndian(value.Count);

      foreach (ref var current in CollectionsMarshal.AsSpan(value))
      {
         itemSerializer.Write(ref writer, ref current);       
      }
   }

   public bool TryRead(ref ByteReader reader, [MaybeNullWhen(false)] out List<T> value)
   {
      var itemSerializer = ItemSerializer;
      var count = reader.ReadLittleEndian<int>();

      value = [];
      
      if (count == 0)
      {
         return true;
      }

      for (var i = 0; i < count; i++)
      {
         if (!itemSerializer.TryRead(ref reader, out var item))
         {
            return false;
         }
         value.Add(item);
      }
      
      return true;
   }
}
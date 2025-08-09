using System.Diagnostics.CodeAnalysis;
using Me.Memory.Buffers;
using Me.Memory.Buffers.Dynamic;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Collections;

public sealed class PooledSetSerializer<T> : ISerializer<PooledSet<T>>
{
   private static ISerializer<T> ItemSerializer => SerializerCache<T>.Instance;
   
   public void Write(ref ByteWriter writer, ref PooledSet<T> value)
   {
      var itemSerializer = ItemSerializer;
      writer.WriteLittleEndian(value.Count);

      foreach (ref var current in value.WrittenSpan)
      {
         itemSerializer.Write(ref writer, ref current);
      }
   }

   public bool TryRead(ref ByteReader reader, [MaybeNullWhen(false)] out PooledSet<T> value)
   {
      var itemSerializer = ItemSerializer;
      var count = reader.ReadLittleEndian<int>();

      value = new PooledSet<T>(count);
      if (count == 0)
      {
         return true;
      }

      for (var i = 0; i < count; i++)
      {
         if (!itemSerializer.TryRead(ref reader, out var item))
         {
            value.Dispose();
            value = null;
            
            return false;
         }
         
         value.Add(item);
      }

      return true;
   }
}
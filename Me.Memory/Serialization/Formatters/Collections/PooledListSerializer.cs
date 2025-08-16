using System.Diagnostics.CodeAnalysis;
using Me.Memory.Buffers;
using Me.Memory.Buffers.Dynamic;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Collections;

public sealed class PooledListSerializer<T> : ISerializer<PooledList<T>>
{
   private static ISerializer<T> ItemSerializer => SerializerCache<T>.Instance;
   
   public void Write(ref ByteWriter writer, ref PooledList<T> value)
   {
      var itemSerializer = ItemSerializer;
      writer.WriteLittleEndian(value.Count);

      foreach (ref var current in value.WrittenSpan)
      {
         itemSerializer.Write(ref writer, ref current);
      }
   }

   public bool TryRead(ref ByteReader reader, [MaybeNullWhen(false)] out PooledList<T> value)
   {
      var itemSerializer = ItemSerializer;
      var count = reader.ReadLittleEndian<int>();

      value = new PooledList<T>(count);
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
   
   public int CalculateByteLength(ref PooledList<T> value)
   {
      var length = sizeof(int);
      foreach (ref var current in value.WrittenSpan)
      {
         length += ItemSerializer.CalculateByteLength(ref current);
      }
      
      return length;
   }
}
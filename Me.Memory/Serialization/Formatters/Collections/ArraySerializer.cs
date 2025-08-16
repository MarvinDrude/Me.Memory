using System.Diagnostics.CodeAnalysis;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Collections;

public sealed class ArraySerializer<T> : ISerializer<T[]>
{
   private static ISerializer<T> ItemSerializer => SerializerCache<T>.Instance;
   
   public void Write(ref ByteWriter writer, ref T[] value)
   {
      var itemSerializer = ItemSerializer;
      writer.WriteLittleEndian(value.Length);

      var span = value.AsSpan();
      for (var index = 0; index < span.Length; index++)
      {
         ref var current = ref span[index];
         itemSerializer.Write(ref writer, ref current);
      }
   }

   public bool TryRead(ref ByteReader reader, [MaybeNullWhen(false)] out T[] value)
   {
      var itemSerializer = ItemSerializer;
      var count = reader.ReadLittleEndian<int>();
      
      value = new T[count];
      
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
         value[i] = item;
      }
      
      return true;
   }

   public int CalculateByteLength(ref T[] value)
   {
      var length = sizeof(int);
      var span = value.AsSpan();
      
      for (var index = 0; index < span.Length; index++)
      {
         ref var current = ref span[index];
         length += ItemSerializer.CalculateByteLength(ref current);
      }

      return length;
   }
}
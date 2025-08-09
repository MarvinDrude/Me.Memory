using System.Diagnostics.CodeAnalysis;
using Me.Memory.Buffers;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Formatters.Collections;

public sealed class DictionarySerializer<TKey, TValue> : ISerializer<Dictionary<TKey, TValue>>
   where TKey : notnull
{
   private static readonly ISerializer<TKey> KeySerializer = SerializerCache<TKey>.Instance;
   private static readonly ISerializer<TValue> ValueSerializer = SerializerCache<TValue>.Instance;
   
   public void Write(ref ByteWriter writer, ref Dictionary<TKey, TValue> value)
   {
      writer.WriteLittleEndian(value.Count);

      var keySerializer = KeySerializer;
      var valueSerializer = ValueSerializer;
      
      foreach (var (key, ob) in value)
      {
         var keySers = key;
         var valueSers = ob;
         
         keySerializer.Write(ref writer, ref keySers);
         valueSerializer.Write(ref writer, ref valueSers);       
      }
   }

   public bool TryRead(ref ByteReader reader, [MaybeNullWhen(false)] out Dictionary<TKey, TValue> value)
   {
      Dictionary<TKey, TValue> result = [];
      var count = reader.ReadLittleEndian<int>();

      var keySerializer = KeySerializer;
      var valueSerializer = ValueSerializer;
      
      for (var i = 0; i < count; i++)
      {
         if (!keySerializer.TryRead(ref reader, out var key)
             || !valueSerializer.TryRead(ref reader, out var valueSers))
         {
            value = null;
            return false;
         }
         
         result[key] = valueSers;
      }
      
      value = result;
      return true;
   }
}
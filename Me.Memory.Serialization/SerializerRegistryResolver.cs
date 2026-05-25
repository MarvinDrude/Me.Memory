using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Me.Memory.Serialization.Formatters.System;
using Me.Memory.Serialization.Formatters.Collections;
using Me.Memory.Serialization.Formatters.Collections.Concurrent;
using Me.Memory.Serialization.Formatters.Collections.Immutable;

namespace Me.Memory.Serialization;

public static class SerializerRegistryResolver
{
   public static void RegisterIfGenericOrSpecial<T>()
      where T : allows ref struct
   {
      var type = typeof(T);
      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
      {
         var underlyingType = Nullable.GetUnderlyingType(type)!;
         var serializerType = typeof(NullableSerializer<>).MakeGenericType(underlyingType);
         RegisterFromSerializerType<T>(serializerType);
         return;
      }

      if (type.IsArray && type.GetArrayRank() == 1)
      {
         var elementType = type.GetElementType()!;
         var serializerType = typeof(ArraySerializer<>).MakeGenericType(elementType);
         RegisterFromSerializerType<T>(serializerType);
         return;
      }

      if (type.IsArray && type.GetArrayRank() == 2)
      {
         var elementType = type.GetElementType()!;
         var serializerType = typeof(Array2DSerializer<>).MakeGenericType(elementType);
         RegisterFromSerializerType<T>(serializerType);
         return;
      }

      if (type.IsArray && type.GetArrayRank() == 4)
      {
         var elementType = type.GetElementType()!;
         var serializerType = typeof(Array4DSerializer<>).MakeGenericType(elementType);
         RegisterFromSerializerType<T>(serializerType);
         return;
      }

      if (type.IsGenericType)
      {
         var genericDef = type.GetGenericTypeDefinition();
         var genericArgs = type.GetGenericArguments();

         Type? serializerType = null;

         if (genericDef == typeof(List<>))
         {
            serializerType = typeof(ListSerializer<>).MakeGenericType(genericArgs[0]);
         }
         else if (genericDef == typeof(Dictionary<,>))
         {
            serializerType = typeof(DictionarySerializer<,>).MakeGenericType(genericArgs[0], genericArgs[1]);
         }
         else if (genericDef == typeof(HashSet<>))
         {
            serializerType = typeof(HashSetSerializer<>).MakeGenericType(genericArgs[0]);
         }
         else if (genericDef == typeof(Queue<>))
         {
            serializerType = typeof(QueueSerializer<>).MakeGenericType(genericArgs[0]);
         }
         else if (genericDef == typeof(Stack<>))
         {
            serializerType = typeof(StackSerializer<>).MakeGenericType(genericArgs[0]);
         }
         else if (genericDef == typeof(LinkedList<>))
         {
            serializerType = typeof(LinkedListSerializer<>).MakeGenericType(genericArgs[0]);
         }
         else if (genericDef == typeof(SortedDictionary<,>))
         {
            serializerType = typeof(SortedDictionarySerializer<,>).MakeGenericType(genericArgs[0], genericArgs[1]);
         }
         else if (genericDef == typeof(SortedList<,>))
         {
            serializerType = typeof(SortedListSerializer<,>).MakeGenericType(genericArgs[0], genericArgs[1]);
         }
         else if (genericDef == typeof(ArraySegment<>))
         {
            serializerType = typeof(ArraySegmentSerializer<>).MakeGenericType(genericArgs[0]);
         }
         else if (genericDef == typeof(ReadOnlyCollection<>))
         {
            serializerType = typeof(ReadOnlyCollectionSerializer<>).MakeGenericType(genericArgs[0]);
         }
         else if (genericDef == typeof(ReadOnlyDictionary<,>))
         {
            serializerType = typeof(ReadOnlyDictionarySerializer<,>).MakeGenericType(genericArgs[0], genericArgs[1]);
         }
         else if (genericDef == typeof(ConcurrentQueue<>))
         {
            serializerType = typeof(ConcurrentQueueSerializer<>).MakeGenericType(genericArgs[0]);
         }
         else if (genericDef == typeof(ConcurrentStack<>))
         {
            serializerType = typeof(ConcurrentStackSerializer<>).MakeGenericType(genericArgs[0]);
         }
         else if (genericDef == typeof(ConcurrentBag<>))
         {
            serializerType = typeof(ConcurrentBagSerializer<>).MakeGenericType(genericArgs[0]);
         }
         else if (genericDef == typeof(ConcurrentDictionary<,>))
         {
            serializerType = typeof(ConcurrentDictionarySerializer<,>).MakeGenericType(genericArgs[0], genericArgs[1]);
         }
         else if (genericDef == typeof(ImmutableArray<>))
         {
            serializerType = typeof(ImmutableArraySerializer<>).MakeGenericType(genericArgs[0]);
         }
         else if (genericDef == typeof(ImmutableList<>))
         {
            serializerType = typeof(ImmutableListSerializer<>).MakeGenericType(genericArgs[0]);
         }
         else if (genericDef == typeof(ImmutableHashSet<>))
         {
            serializerType = typeof(ImmutableHashSetSerializer<>).MakeGenericType(genericArgs[0]);
         }
         else if (genericDef == typeof(ImmutableQueue<>))
         {
            serializerType = typeof(ImmutableQueueSerializer<>).MakeGenericType(genericArgs[0]);
         }
         else if (genericDef == typeof(ImmutableStack<>))
         {
            serializerType = typeof(ImmutableStackSerializer<>).MakeGenericType(genericArgs[0]);
         }
         else if (genericDef == typeof(ImmutableSortedSet<>))
         {
            serializerType = typeof(ImmutableSortedSetSerializer<>).MakeGenericType(genericArgs[0]);
         }
         else if (genericDef == typeof(ImmutableDictionary<,>))
         {
            serializerType = typeof(ImmutableDictionarySerializer<,>).MakeGenericType(genericArgs[0], genericArgs[1]);
         }
         else if (genericDef == typeof(ImmutableSortedDictionary<,>))
         {
            serializerType = typeof(ImmutableSortedDictionarySerializer<,>).MakeGenericType(genericArgs[0], genericArgs[1]);
         }

         if (serializerType != null)
         {
            RegisterFromSerializerType<T>(serializerType);
         }
      }
   }

   private static void RegisterFromSerializerType<T>(Type serializerType)
      where T : allows ref struct
   {
      const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
      
      var writeMethod = serializerType.GetMethod("Write", flags);
      var tryReadMethod = serializerType.GetMethod("TryRead", flags);
      var calculateLengthMethod = serializerType.GetMethod("CalculateByteLength", flags);

      if (writeMethod != null && tryReadMethod != null && calculateLengthMethod != null)
      {
         SerializerRegistry<T>.Write = (WriteDelegate<T>)Delegate.CreateDelegate(typeof(WriteDelegate<T>), writeMethod);
         SerializerRegistry<T>.TryRead = (TryReadDelegate<T>)Delegate.CreateDelegate(typeof(TryReadDelegate<T>), tryReadMethod);
         SerializerRegistry<T>.CalculateByteLength = (CalculateByteLengthDelegate<T>)Delegate.CreateDelegate(typeof(CalculateByteLengthDelegate<T>), calculateLengthMethod);
      }
   }
}

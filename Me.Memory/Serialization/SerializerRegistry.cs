using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using Me.Memory.Serialization.Formatters.Collections;
using Me.Memory.Serialization.Formatters.Common;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization;

public static class SerializerRegistry
{
   private static class Cache<T>
   {
      internal static ISerializer<T>? Instance;
   }

   private static readonly ConcurrentDictionary<Type, Type> _targetToSerializerType = [];
   
   public static void Register<T>(ISerializer<T> serializer)
   {
      Cache<T>.Instance = serializer;
   }

   /// <summary>
   /// Uses ugly reflection, but since this is only called at startup, it's not a big deal.
   /// Will update this later when source generators allow modern .NET
   /// </summary>
   public static void RegisterFromAssembly(Assembly assembly)
   {
      var interfaceType = typeof(ISerializer<>);
      var interfaceTypeBase = typeof(ISerializer);
      
      foreach (var type in assembly.GetTypes()
                  .Where(t => t is { IsInterface: false } 
                              && interfaceTypeBase.IsAssignableFrom(t)))
      {
         if (type.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType)
             is not { } interFace)
         {
            continue;
         }
         
         var serializedType = interFace.GetGenericArguments().First();
         
         if (type.IsGenericType)
         {
            var serializedDefinition = serializedType.IsGenericType 
               ? serializedType.GetGenericTypeDefinition() 
               : serializedType;
            _targetToSerializerType[serializedDefinition] = type;
            
            continue;
         }

         var instance = Activator.CreateInstance(type)
            ?? throw new InvalidOperationException($"Failed to create {type}");
         RegisterMethod.MakeGenericMethod(serializedType).Invoke(null, [instance]);
      }
   }

   public static ISerializer<T> For<T>()
   {
      if (Cache<T>.Instance is { } serializer)
      {
         return serializer;
      }
      
      var type = typeof(T);
      if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
      {
         var unmanagedType = typeof(UnmanagedSerializer<>).MakeGenericType(type);
         var unmanagedSerializer = (Activator.CreateInstance(unmanagedType) as ISerializer<T>)
            ?? throw new InvalidOperationException($"No serializer registered for type {typeof(T)}");
         
         return Cache<T>.Instance ??= unmanagedSerializer;
      }

      if (type.IsArray)
      {
         var arrayType = typeof(ArraySerializer<>).MakeGenericType(
            type.GetElementType() ?? throw new InvalidOperationException());
         var arraySerializer = (Activator.CreateInstance(arrayType) as ISerializer<T>)
            ?? throw new InvalidOperationException($"No serializer registered for type {typeof(T)}");
         
         return Cache<T>.Instance ??= arraySerializer;
      }
      
      if (type.IsGenericType)
      {
         var definition = type.GetGenericTypeDefinition();
         if (_targetToSerializerType.TryGetValue(definition, out var serializerType))
         {
            var args = type.GetGenericArguments();
            var closed = serializerType.MakeGenericType(args);
            var instance = Activator.CreateInstance(closed)
               ?? throw new InvalidOperationException($"Failed to create {closed}");
            
            return Cache<T>.Instance ??= (ISerializer<T>)instance;
         }
      }
      
      throw new InvalidOperationException($"No serializer registered for type {typeof(T)}");
   }
   
   [ModuleInitializer]
   internal static void ModuleInit()
   {
      RegisterFromAssembly(typeof(SerializerRegistry).Assembly);
      
      // pre-heat some common unmanaged serializers
      Register(new UnmanagedSerializer<short>());
      Register(new UnmanagedSerializer<ushort>());
      Register(new UnmanagedSerializer<int>());
      Register(new UnmanagedSerializer<uint>());
      Register(new UnmanagedSerializer<long>());
      Register(new UnmanagedSerializer<ulong>());
      Register(new UnmanagedSerializer<nint>());
      Register(new UnmanagedSerializer<nuint>());
      Register(new UnmanagedSerializer<float>());
      Register(new UnmanagedSerializer<double>());
      Register(new UnmanagedSerializer<decimal>());
      Register(new UnmanagedSerializer<bool>());
   }
   
   private static readonly MethodInfo RegisterMethod 
      = typeof(SerializerRegistry).GetMethod(nameof(Register), BindingFlags.Public | BindingFlags.Static) 
        ?? throw new InvalidOperationException();
}
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization;

/// <summary>
/// A zero-overhead, reflection-free static registry for serializers.
/// </summary>
/// <typeparam name="T">The type of the values being serialized.</typeparam>
public static class SerializerRegistry<T>
   where T : allows ref struct
{
   /// <summary>
   /// Gets or sets the static serializer instance for type <typeparamref name="T"/>.
   /// Access is resolved by the JIT compiler as a direct pointer read, yielding maximum performance.
   /// </summary>
   public static IInstanceSerializer<T>? Instance { get; set; }
}

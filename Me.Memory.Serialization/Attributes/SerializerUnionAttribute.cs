using System;

namespace Me.Memory.Serialization.Attributes;

/// <summary>
/// Declares a known derived type for polymorphic inheritance-based serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public sealed class SerializerUnionAttribute(int tag, Type type) : Attribute
{
   public int Tag { get; } = tag;
   
   public Type Type { get; } = type;
}

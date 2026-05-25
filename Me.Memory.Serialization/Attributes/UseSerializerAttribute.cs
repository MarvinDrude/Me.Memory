using System;

namespace Me.Memory.Serialization.Attributes;

/// <summary>
/// Specifies a custom serializer to be used for this property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class UseSerializerAttribute(Type serializerType) : Attribute
{
   public Type SerializerType { get; } = serializerType;
}

using System;

namespace Me.Memory.Serialization.Attributes;

/// <summary>
/// Specifies the serialization position of a public property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SerializerPositionAttribute(int position) : Attribute
{
   public int Position { get; } = position;
}

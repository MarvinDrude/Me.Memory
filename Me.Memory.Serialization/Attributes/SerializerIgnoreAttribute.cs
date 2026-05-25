using System;

namespace Me.Memory.Serialization.Attributes;

/// <summary>
/// Specifies that a property should be ignored during serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SerializerIgnoreAttribute : Attribute;

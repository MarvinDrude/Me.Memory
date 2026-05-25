using System;

namespace Me.Memory.Serialization.Attributes;

/// <summary>
/// Marks a class or struct for automatic serializer generation.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class GenerateSerializerAttribute : Attribute;

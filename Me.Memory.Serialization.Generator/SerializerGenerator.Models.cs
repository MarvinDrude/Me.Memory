using Me.Memory.Collections;

namespace Me.Memory.Serialization.Generator;

public sealed record PropertyInfo(string Name, string TypeFullyQualifiedName, string? CustomSerializerFullyQualifiedName = null);

public sealed record UnionInfo(int Tag, string TypeFullyQualifiedName);

public sealed record SerializerGenerationData(
   string Namespace,
   string TypeName,
   bool IsReferenceType,
   bool IsAbstract,
   string FullyQualifiedName,
   string TypeNameWithNullability,
   SequenceArray<PropertyInfo> Properties,
   SequenceArray<UnionInfo> Unions
);

namespace Me.Memory.Serialization.Generator;

public sealed partial class SerializerGenerator
{
   private const string GenerateSerializerAttributeFullName = "Me.Memory.Serialization.Attributes.GenerateSerializerAttribute";
   private const string SerializerPositionAttributeFullName = "Me.Memory.Serialization.Attributes.SerializerPositionAttribute";
   private const string SerializerUnionAttributeFullName = "Me.Memory.Serialization.Attributes.SerializerUnionAttribute";

   private const string DiagnosticId = "MEMGEN001";
   private const string DiagnosticTitle = "Failed to generate serializer";
   private const string DiagnosticMessageFormat = "An exception occurred while generating serializer for {0}: {1}";
   private const string DiagnosticCategory = "SerializerGenerator";
}

using Me.Memory.Collections;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Me.Memory.Serialization.Generator;

public sealed partial class SerializerGenerator
{
   private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
   {
      return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 } 
         or StructDeclarationSyntax { AttributeLists.Count: > 0 } 
         or RecordDeclarationSyntax { AttributeLists.Count: > 0 };
   }

   private static SerializerGenerationData? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
   {
      var typeDecl = (TypeDeclarationSyntax)context.Node;
      var isTarget = false;
      
      foreach (var attributeList in typeDecl.AttributeLists)
      {
         foreach (var attribute in attributeList.Attributes)
         {
            if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attribSymbol) 
               continue;
            
            var attribContainingType = attribSymbol.ContainingType.ToDisplayString();
            if (attribContainingType == GenerateSerializerAttributeFullName)
            {
               isTarget = true;
               break;
            }
         }
         if (isTarget) break;
      }
      
      if (!isTarget) return null;

      if (context.SemanticModel.GetDeclaredSymbol(typeDecl) is not INamedTypeSymbol typeSymbol)
      {
         return null;
      }

      var ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
         ? ""
         : typeSymbol.ContainingNamespace.ToDisplayString();
      var typeName = typeSymbol.Name;
      var isReferenceType = typeSymbol.IsReferenceType;
      var isAbstract = typeSymbol.IsAbstract;
      var fullyQualifiedName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
      var typeNameWithNullability = isReferenceType ? $"{fullyQualifiedName}?" : fullyQualifiedName;

      var sortedProperties = CollectPositionedProperties(typeSymbol);
      var unions = CollectUnions(typeSymbol);

      var properties = new SequenceArray<PropertyInfo>(sortedProperties.Select(prop => {
         var customSerializerAttr = prop.GetAttributes().FirstOrDefault(a => 
            a.AttributeClass?.ToDisplayString() == UseSerializerAttributeFullName);
         
         string? customSerializerName = null;
         if (customSerializerAttr is not null && customSerializerAttr.ConstructorArguments.Length > 0 
            && customSerializerAttr.ConstructorArguments[0].Value is ITypeSymbol serializerTypeSymbol)
         {
            customSerializerName = serializerTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
         }

         return new PropertyInfo(
            prop.Name,
            prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            customSerializerName
         );
      }).ToArray());

      var unionInfos = new SequenceArray<UnionInfo>(unions.Select(u => new UnionInfo(
         u.Tag,
         u.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
      )).ToArray());

      return new SerializerGenerationData(
         ns,
         typeName,
         isReferenceType,
         isAbstract,
         fullyQualifiedName,
         typeNameWithNullability,
         properties,
         unionInfos
      );
   }

   private static List<IPropertySymbol> CollectPositionedProperties(INamedTypeSymbol typeSymbol)
   {
      var properties = new List<IPropertySymbol>();
      var currentType = typeSymbol;
      
      while (currentType is not null && currentType.SpecialType == SpecialType.None)
      {
         foreach (var member in currentType.GetMembers())
         {
            if (member is IPropertySymbol { IsReadOnly: false, DeclaredAccessibility: not Accessibility.Private } property)
            {
               properties.Add(property);
            }
         }
         
         currentType = currentType.BaseType;
      }

      var positionedProperties = new List<(int Position, IPropertySymbol Property)>();
      foreach (var prop in properties)
      {
         var hasIgnore = prop.GetAttributes().Any(a => 
            a.AttributeClass?.ToDisplayString() == SerializerIgnoreAttributeFullName);
         if (hasIgnore)
         {
            continue;
         }

         var attr = prop.GetAttributes().FirstOrDefault(a => 
            a.AttributeClass?.ToDisplayString() == SerializerPositionAttributeFullName);
         
         if (attr is not null && attr.ConstructorArguments.Length > 0 
            && attr.ConstructorArguments[0].Value is int pos)
         {
            positionedProperties.Add((pos, prop));
         }
      }

      return positionedProperties
         .OrderBy(p => p.Position)
         .Select(p => p.Property)
         .ToList();
   }

   private static List<(int Tag, ITypeSymbol Type)> CollectUnions(INamedTypeSymbol typeSymbol)
   {
      var unions = new List<(int Tag, ITypeSymbol Type)>();
      var unionAttrs = typeSymbol.GetAttributes().Where(a => 
         a.AttributeClass?.ToDisplayString() == SerializerUnionAttributeFullName);
      
      foreach (var attr in unionAttrs)
      {
         if (attr.ConstructorArguments is [{ Value: int tag }, { Value: ITypeSymbol unionType }])
         {
            unions.Add((tag, unionType));
         }
      }
      return unions;
   }
}

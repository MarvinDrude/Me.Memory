using System;
using System.Collections.Generic;
using System.Linq;
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

   private static TypeDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
   {
      var typeDecl = (TypeDeclarationSyntax)context.Node;
      
      foreach (var attributeList in typeDecl.AttributeLists)
      {
         foreach (var attribute in attributeList.Attributes)
         {
            if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attribSymbol) 
               continue;
            
            var attribContainingType = attribSymbol.ContainingType.ToDisplayString();
            if (attribContainingType == GenerateSerializerAttributeFullName)
            {
               return typeDecl;
            }
         }
      }
      
      return null;
   }

   private static List<IPropertySymbol> CollectPositionedProperties(INamedTypeSymbol typeSymbol)
   {
      var properties = new List<IPropertySymbol>();
      var currentType = typeSymbol;
      
      while (currentType is not null && currentType.SpecialType == SpecialType.None)
      {
         foreach (var member in currentType.GetMembers())
         {
            if (member is IPropertySymbol property)
            {
               properties.Add(property);
            }
         }
         currentType = currentType.BaseType;
      }

      var positionedProperties = new List<(int Position, IPropertySymbol Property)>();
      foreach (var prop in properties)
      {
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

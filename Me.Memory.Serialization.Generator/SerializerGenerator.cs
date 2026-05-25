using System;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Me.Memory.Serialization.Generator;

[Generator]
public sealed partial class SerializerGenerator : IIncrementalGenerator
{
   public void Initialize(IncrementalGeneratorInitializationContext context)
   {
      IncrementalValuesProvider<TypeDeclarationSyntax> typeDeclarations = context.SyntaxProvider
         .CreateSyntaxProvider(
            predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
            transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
         .Where(static m => m is not null)!;

      IncrementalValueProvider<(Compilation, ImmutableArray<TypeDeclarationSyntax>)> compilationAndTypes =
         context.CompilationProvider.Combine(typeDeclarations.Collect());

      context.RegisterSourceOutput(compilationAndTypes,
         static (spc, source) => Execute(source.Item1, source.Item2, spc));
   }

   private static void Execute(Compilation compilation, ImmutableArray<TypeDeclarationSyntax> types, SourceProductionContext context)
   {
      if (types.IsDefaultOrEmpty)
      {
         return;
      }

      var distinctTypes = types.Distinct();
      foreach (var typeDecl in distinctTypes)
      {
         var semanticModel = compilation.GetSemanticModel(typeDecl.SyntaxTree);
         if (semanticModel.GetDeclaredSymbol(typeDecl) is not INamedTypeSymbol typeSymbol)
         {
            continue;
         }

         try
         {
            var generatedSource = GenerateSerializerSource(typeSymbol);
            var hintName = $"{typeSymbol.ContainingNamespace.ToDisplayString().Replace(".", "_")}_{typeSymbol.Name}Serializer.g.cs";
            
            context.AddSource(hintName, SourceText.From(generatedSource, Encoding.UTF8));
         }
         catch (Exception ex)
         {
            ReportGenerationFailure(context, typeDecl, typeSymbol, ex);
         }
      }
   }
}

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Me.Memory.Serialization.Generator;

public sealed partial class SerializerGenerator
{
   private static void ReportGenerationFailure(SourceProductionContext context, TypeDeclarationSyntax typeDecl, INamedTypeSymbol typeSymbol, Exception ex)
   {
      var descriptor = new DiagnosticDescriptor(
         id: DiagnosticId,
         title: DiagnosticTitle,
         messageFormat: DiagnosticMessageFormat,
         category: DiagnosticCategory,
         defaultSeverity: DiagnosticSeverity.Error,
         isEnabledByDefault: true);

      context.ReportDiagnostic(Diagnostic.Create(descriptor, typeDecl.GetLocation(), typeSymbol.Name, ex.ToString()));
   }
}

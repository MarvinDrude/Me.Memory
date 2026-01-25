using Microsoft.CodeAnalysis;
using Me.Memory.Embedded;

namespace Me.Memory.Embedded.Generator;

[Generator]
public sealed partial class SharedSourceGenerator : IIncrementalGenerator
{
   public void Initialize(IncrementalGeneratorInitializationContext context)
   {
      context.RegisterPostInitializationOutput(static (ctx) =>
      {
         EmbeddedPayload.Add(ctx);
      });
   }
}
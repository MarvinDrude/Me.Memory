using System.Text;
using Microsoft.CodeAnalysis;

namespace Me.Memory.Embedded.Packer;

[Generator]
public sealed class SourcePacker : IIncrementalGenerator
{
   public void Initialize(IncrementalGeneratorInitializationContext context)
   {
      var files = context.AdditionalTextsProvider
         .Where(a => a.Path.EndsWith(".cs"));
      var collected = files.Collect();

      context.RegisterSourceOutput(collected, (spc, sourceFiles) =>
      {
         StringBuilder sb = new();
         HashSet<string> already = [];

         sb.AppendLine("using Microsoft.CodeAnalysis;");
         sb.AppendLine("");
         sb.AppendLine("namespace Me.Memory.Embedded;");
         
         sb.AppendLine("internal static class EmbeddedPayload");
         sb.AppendLine("{");

         StringBuilder addMethod = new();
         addMethod.AppendLine("   internal static void Add(IncrementalGeneratorPostInitializationContext context)");
         addMethod.AppendLine("   {");

         foreach (var file in sourceFiles)
         {
            var content = file.GetText()?.ToString();
            if (content == null) continue;
            if (!content.Contains("namespace Me.Memory")) continue;
            
            var quotes = 3;
            while (content.Contains(new string('"', quotes))) quotes++;
            var quoteStr = new string('"', quotes);

            var safeName = Path.GetFileNameWithoutExtension(file.Path).Replace(".", "_");
            if (!already.Add(safeName)) continue;
            
            sb.AppendLine($"   public const string {safeName} = {quoteStr}");
            sb.AppendLine("#nullable enable");
            sb.AppendLine(content);
            sb.AppendLine($"{quoteStr};");
            sb.AppendLine();

            addMethod.AppendLine($"      context.AddSource(\"{safeName}.g.cs\", {safeName});");
         }
         
         
         addMethod.AppendLine("   }");
         sb.Append(addMethod);

         sb.AppendLine("}");
         spc.AddSource("Payload.g.cs", sb.ToString());
      });
   }
}
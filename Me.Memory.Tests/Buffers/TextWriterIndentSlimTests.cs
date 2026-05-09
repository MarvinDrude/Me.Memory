using Me.Memory.Buffers;

namespace Me.Memory.Tests.Buffers;

public sealed class TextWriterIndentSlimTests
{
   [Test]
   public async Task WriteEncodedProperlyEscapesBigFiveCharacters()
   {
      string result;
      const string input = "<tag attribute='val' & \"quote\">";
      const string expected = "&lt;tag attribute=&apos;val&apos; &amp; &quot;quote&quot;&gt;";

      {
         using var writer = new TextWriterIndentSlim(
            stackalloc char[128], 
            stackalloc char[32]);

         writer.WriteHtmlEncoded(input.AsSpan());
         result = writer.ToString();
      }

      await Assert.That(result).IsEqualTo(expected);
   }

   [Test]
   public async Task WriteEncodedFastPathWorksForPlainText()
   {
      string result;
      const string input = "Hello World 123";

      {
         using var writer = new TextWriterIndentSlim(
            stackalloc char[64], 
            stackalloc char[32]);

         writer.WriteHtmlEncoded(input.AsSpan());
         result = writer.ToString();
      }

      await Assert.That(result).IsEqualTo(input);
   }

   [Test]
   public async Task WriteEncodedMaintainsIndentationOnNewLines()
   {
      string result;
      const string text = "<b>Bold</b>";

      {
         using var writer = new TextWriterIndentSlim(
            stackalloc char[128], 
            stackalloc char[32],
            indentSize: 2);

         writer.UpIndent();
         writer.WriteHtmlEncoded(text.AsSpan());
         writer.WriteLine();
         writer.WriteHtmlEncoded(text.AsSpan());
         
         result = writer.ToString();
      }

      var expected = $"  &lt;b&gt;Bold&lt;/b&gt;\n  &lt;b&gt;Bold&lt;/b&gt;";
      await Assert.That(result).IsEqualTo(expected);
   }

   [Test]
   public async Task WriteEncodedMultiLineSplitsAndIndentsCorrectly()
   {
      string result;
      const string multiLineInput = "Line 1 <\n    Line 2 >";

      {
         using var writer = new TextWriterIndentSlim(
            stackalloc char[128], 
            stackalloc char[32],
            indentSize: 4);

         writer.UpIndent();
         writer.WriteHtmlEncoded(multiLineInput.AsSpan(), true);
         
         result = writer.ToString();
      }

      var expected = $"    Line 1 &lt;\n    Line 2 &gt;";
      await Assert.That(result).IsEqualTo(expected);
   }

   [Test]
   public async Task WriteEncodedHandlesSequentialSpecialCharacters()
   {
      string result;
      const string input = "<<<";
      const string expected = "&lt;&lt;&lt;";

      {
         using var writer = new TextWriterIndentSlim(
            stackalloc char[64], 
            stackalloc char[32]);

         writer.WriteHtmlEncoded(input.AsSpan());
         result = writer.ToString();
      }

      await Assert.That(result).IsEqualTo(expected);
   }

   [Test]
   public async Task WriteEncodedHandlesLargeInputWithResizing()
   {
      string result;
      
      var input = new string('<', 500); 
      var expected = string.Concat(Enumerable.Repeat("&lt;", 500));

      {
         using var writer = new TextWriterIndentSlim(
            stackalloc char[10], 
            stackalloc char[10],
            initialMinGrowCapacity: 1024);

         writer.WriteHtmlEncoded(input.AsSpan());
         result = writer.ToString();
      }

      await Assert.That(result.Length).IsEqualTo(expected.Length);
      await Assert.That(result).IsEqualTo(expected);
   }
}
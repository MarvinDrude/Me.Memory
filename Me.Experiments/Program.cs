
using Me.Memory.Buffers;

Console.WriteLine("Hello World!");
var x = 20;

var writer = new TextWriterIndentSlim(stackalloc char[256], stackalloc char[128]);
writer.UpIndent();
writer.UpIndent();
writer.WriteInterpolated($"{x} - {x}");

Console.WriteLine(writer.ToString());
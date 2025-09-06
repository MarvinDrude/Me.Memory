
using Me.Memory.Buffers;
using Me.Memory.Collections;

Console.WriteLine("Hello World!");
var x = 20;

var writer = new TextWriterIndentSlim(stackalloc char[256], stackalloc char[128]);
writer.UpIndent();
writer.UpIndent();
writer.WriteInterpolated($"{x} - {x}");

Console.WriteLine(writer.ToString());

SequenceArray<string> test = ["a", "b", "c"];

foreach (ref var item in test)
{
   Console.WriteLine(item);
}

var test1 = new SequenceArray<string>(new string[] { "a", "b", "c", "d" });

Console.WriteLine(test == test1);
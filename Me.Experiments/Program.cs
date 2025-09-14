
using Me.Memory.Buffers;
using Me.Memory.Buffers.Spans;
using Me.Memory.Collections;

Console.WriteLine("Hello World!");
var x = 20;

var writer = new TextWriterIndentSlim(stackalloc char[256], stackalloc char[128]);
writer.UpIndent();
writer.UpIndent();
writer.WriteInterpolated($"{x} - {x}");

Console.WriteLine(writer.ToString());

using var circ = new CircularBuffer<string>(4);

circ.Add("1");
circ.Add("2");
circ.Add("3");

foreach (ref var item in circ)
{
   Console.WriteLine(item);
}
Console.WriteLine("==");

circ.Add("4");
foreach (ref var item in circ)
{
   Console.WriteLine(item);
}
Console.WriteLine("==");
circ.Add("5");
circ.Add("6");

foreach (ref var item in circ)
{
   Console.WriteLine(item);
}
Console.WriteLine(circ[3]);
Console.WriteLine("==");

Console.WriteLine(circ.WrittenTwoSpan.Length);
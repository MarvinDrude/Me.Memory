
using Me.Memory.Buffers;
using Me.Memory.Buffers.Spans;
using Me.Memory.Collections;
using Me.Memory.Pools;
using Me.Memory.Services;
using Microsoft.Extensions.DependencyInjection;

var coll = new ServiceCollection();
coll.AddSingleton<A>();
coll.AddSingleton<B>();
var provider = coll.BuildServiceProvider();

if (provider.TryGetServices<A, B>(out var a, out var b))
{
   Console.WriteLine(a.Name);
   Console.WriteLine(b.Name);
}

Console.WriteLine("test");


public class A
{
   public string Name => "a";
}

public class B
{
   public string Name => "b";
}
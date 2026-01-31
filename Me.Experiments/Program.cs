
using System.Runtime.InteropServices;
using Me.Memory.Buffers;
using Me.Memory.Buffers.Spans;
using Me.Memory.Collections;
using Me.Memory.Extensions;
using Me.Memory.Pools;
using Me.Memory.Services;
using Me.Memory.Utils;
using Microsoft.Extensions.DependencyInjection;

var result = new AsyncTimerResult();
using (var timer = new AsyncTimer(result))
{
   await Task.Delay(200);
}

Console.WriteLine("Hello World! " + result.Elapsed.TotalMilliseconds);

// var coll = new ServiceCollection();
// coll.AddSingleton<A>();
// coll.AddSingleton<B>();
// var provider = coll.BuildServiceProvider();
//
// if (provider.TryGetServices<A, B>(out var a, out var b))
// {
//    Console.WriteLine(a.Name);
//    Console.WriteLine(b.Name);
// }
//
//
//
// public class A
// {
//    public string Name => "a";
// }
//
// public class B
// {
//    public string Name => "b";
// }
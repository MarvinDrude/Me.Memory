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
using (new AsyncTimer(result))
{
   await Task.Delay(200);
}

var time = TimeSpan.Zero;
using (new StackTimer(ref time))
{
   Thread.Sleep(300);
}

var times = 0L;
using (new StackTimer(ref times))
{
   Thread.Sleep(400);
}


Console.WriteLine("Hello World! " + result.Elapsed.TotalMilliseconds);
Console.WriteLine("Hello World! " + time.TotalMilliseconds);
Console.WriteLine("Hello World! " + new TimeSpan(times).TotalMilliseconds);
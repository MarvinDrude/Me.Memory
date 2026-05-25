using System.Runtime.InteropServices;
using Me.Memory.Buffers;
using Me.Memory.Buffers.Spans;
using Me.Memory.Collections;
using Me.Memory.Extensions;
using Me.Memory.Pools;
using Me.Memory.Serialization;
using Me.Memory.Services;
using Me.Memory.Utils;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello World!");

var bytes = MeSerializer.SerializeWithoutPrecalculation((uint?)200, 256);
var deserialized = MeSerializer.Deserialize<int>(bytes);

Console.WriteLine(deserialized);
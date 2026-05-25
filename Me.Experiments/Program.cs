using System.Runtime.InteropServices;
using Me.Memory.Buffers;
using Me.Memory.Buffers.Spans;
using Me.Memory.Collections;
using Me.Memory.Extensions;
using Me.Memory.Pools;
using Me.Memory.Serialization;
using Me.Memory.Serialization.Attributes;
using Me.Memory.Services;
using Me.Memory.Utils;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello World!");

var bytes = MeSerializer.SerializeWithoutPrecalculation(new TestStruct()
{
   Value = 200,
   Next = new TestStruct2()
   {
      Value2 = 100
   }
}, 256);
var deserialized = MeSerializer.Deserialize<TestStruct>(bytes);

Console.WriteLine(deserialized);

return;

[GenerateSerializer]
public struct TestStruct
{
   [SerializerPosition(0)]
   public int Value { get; set; }
   
   [SerializerPosition(1)]
   public int ValueIn { get; set; }
      
   [SerializerPosition(2)]
   public TestStruct2 Next { get; set; }
}
   
[GenerateSerializer]
public struct TestStruct2
{
   [SerializerPosition(0)]
   public int Value2 { get; set; }
}

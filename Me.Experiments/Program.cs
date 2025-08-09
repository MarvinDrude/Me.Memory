
using Me.Memory.Extensions;
using Me.Memory.Serialization;

int[] test = [1, 2, 3, 4, 5];



var memory = test.SerializeMemory();
var next = SerializerCache<int[]>.DeserializeMemory(memory.Span);

Console.WriteLine(next);
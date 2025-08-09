

using Me.Memory.Serialization;

var test = SerializerCache<string>.Instance;
var test2 = SerializerCache<List<string>>.Instance;
var test23 = SerializerCache<List<List<string>>>.Instance;

Console.WriteLine(test);
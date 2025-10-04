
using Me.Memory.Buffers;
using Me.Memory.Buffers.Spans;
using Me.Memory.Collections;
using Me.Memory.Pools;


var pool = new ObjectPool<Test>(new ObjectPoolOptions<Test>()
{
   FactoryFunc = static () =>
   {
      Console.WriteLine("CREATE");
      return new Test()
      {
         Name = "Empty"
      };
   },
   ReturnFunc = static (_) => true,
   MaxSize = 10,
   InitialSize = 5
});

Console.WriteLine("AAA");

for (var i = 0; i < 10; i++)
{
   var tt = pool.Get();
   pool.Return(tt);
}

public class Test
{
   public string? Name { get; set; }
}
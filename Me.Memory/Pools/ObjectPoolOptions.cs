namespace Me.Memory.Pools;

public sealed class ObjectPoolOptions<T>
{
   public int MaxSize { get; init; } = 1024 * 8;

   public int InitialSize { get; init; } = 512;
   
   public required Func<T> FactoryFunc { get; init; }

   public Func<T, bool> ReturnFunc { get; init; } = static (_) => true;
}
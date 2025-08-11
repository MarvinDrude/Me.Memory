using System.Threading.Channels;

namespace Me.Memory.Threading;

public sealed class WorkPoolOptions
{
   public int MaxDegreeOfParallelism { get; init; } = Environment.ProcessorCount;

   public int Capacity { get; init; } = 10_000;

   public BoundedChannelFullMode FullMode { get; init; } = BoundedChannelFullMode.Wait;
   
   public bool SingleReader { get; init; } = false;
}
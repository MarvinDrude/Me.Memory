using System.Diagnostics;

namespace Me.Memory.Utils;

/// <summary>
/// Works for async methods to get timing. If Method is not
/// ran in parallel / concurrent u can reuse one allocation of
/// TimerResult
/// </summary>
public readonly struct AsyncTimer(AsyncTimerResult result) : IDisposable
{
   private readonly long _startTicks = Stopwatch.GetTimestamp();
   private readonly AsyncTimerResult _result = result;

   public void Dispose()
   {
      _result.Elapsed = Stopwatch.GetElapsedTime(_startTicks);
   }
}

public sealed class AsyncTimerResult
{
   public TimeSpan Elapsed { get; internal set; }
}
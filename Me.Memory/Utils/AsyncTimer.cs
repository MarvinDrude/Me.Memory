using System.Diagnostics;

namespace Me.Memory.Utils;

/// <summary>
/// Works for async methods to get timing. If Method is not
/// ran in parallel / concurrent u can reuse one allocation of
/// TimerResult
/// </summary>
public readonly struct AsyncTimer : IDisposable
{
   private readonly long _startTicks;
   private readonly AsyncTimerResult _result;

   public AsyncTimer(AsyncTimerResult result)
   {
      _result = result;
      _startTicks = Stopwatch.GetTimestamp();
   }
   
   public void Dispose()
   {
      var delta = Stopwatch.GetTimestamp() - _startTicks;
      _result.Elapsed = new TimeSpan(delta);
   }
}

public sealed class AsyncTimerResult
{
   public TimeSpan Elapsed { get; internal set; }
}
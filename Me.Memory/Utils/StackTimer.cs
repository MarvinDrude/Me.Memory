using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Me.Memory.Utils;

/// <summary>
/// Stack allocated timing helper, works with using block
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly ref struct StackTimer
{
   private readonly long _startTicks;
   
   private readonly ref long _resultTicks;
   private readonly ref TimeSpan _resultSpan;

   private readonly bool _isSpan;
   
   public StackTimer(ref long resultTicks)
   {
      _resultTicks = ref resultTicks;
      _resultSpan = ref Unsafe.NullRef<TimeSpan>();
      
      _startTicks = GetTimestamp();
      _isSpan = false;
   }

   public StackTimer(ref TimeSpan resultSpan)
   {
      _resultSpan = ref resultSpan;
      _resultTicks = ref Unsafe.NullRef<long>();
      
      _startTicks = GetTimestamp();
      _isSpan = true;
   }

   public void Dispose()
   {
      var delta = GetTimestamp() - _startTicks;

      if (_isSpan)
      {
         _resultSpan = new TimeSpan(delta);
         return;
      }
      _resultTicks = delta;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static long GetTimestamp()
   {
      return Stopwatch.GetTimestamp();
   }
}
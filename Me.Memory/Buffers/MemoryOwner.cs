using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Me.Memory.Buffers;

[StructLayout(LayoutKind.Auto)]
public readonly struct MemoryOwner<T> : IDisposable
{
   public int Capacity
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Buffer.Length;
   }

   public int Length
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get;
   }

   public Span<T> Span
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Buffer.AsSpan(0, Length);
   }

   public Memory<T> Memory
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Buffer.AsMemory(0, Length);
   }
   
   public T[] Buffer
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get;
   }

   public MemoryOwner(int minSize)
   {
      Buffer = ArrayPool<T>.Shared.Rent(minSize);
      Length = minSize;
   }
   
   public void Dispose()
   {
      ArrayPool<T>.Shared.Return(Buffer);
   }
}
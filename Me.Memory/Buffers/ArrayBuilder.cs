using System.Buffers;
using System.Runtime.CompilerServices;

namespace Me.Memory.Buffers;

public sealed class ArrayBuilder<T> : IDisposable
{
   private static readonly T[] EmptyPlaceholder = [];

   public int Count
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _position;
   }

   public Span<T> WrittenSpan
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _buffer.AsSpan(0, _position);
   }

   public T[] UnderlyingArray
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _buffer;
   }
   
   private readonly ArrayPool<T> _pool = ArrayPool<T>.Shared;
   private readonly int _minCapacity;
   private bool _disposed;

   private T[] _buffer;
   private int _position;

   public ArrayBuilder(int minCapacity = 16)
   {
      _minCapacity = minCapacity;
      _buffer = EmptyPlaceholder;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Add(scoped in T value)
   {
      if (_position == _buffer.Length)
      {
         GrowCapacity(_buffer.Length * 2);
      }
      
      _buffer[_position++] = value;
   }

   public void Add(ReadOnlySpan<T> span)
   {
      var requiredLength = _position + span.Length;
      if (_buffer.Length < requiredLength)
      {
         var newCapacity = Math.Max(_buffer.Length * 2, _minCapacity);
         while (newCapacity < requiredLength) newCapacity *= 2;
         
         GrowCapacity(newCapacity);
      }
      
      span.CopyTo(_buffer.AsSpan(_position));
      _position += span.Length;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Set(int index, scoped in T value)
   {
      _buffer[index] = value;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Clear()
   {
      ReturnBuffer();
      
      _buffer = EmptyPlaceholder;
      _position = 0;
   }

   private void GrowCapacity(int requestedSize)
   {
      var newCapacity = Math.Max(requestedSize, _minCapacity);
      var newBuffer = _pool.Rent(newCapacity);

      if (_position > 0)
      {
         _buffer.AsSpan(0, _position).CopyTo(newBuffer);
      }

      ReturnBuffer();
      _buffer = newBuffer;
   }
   
   private void ReturnBuffer()
   {
      if (ReferenceEquals(EmptyPlaceholder, _buffer)) return;
      
      Array.Clear(_buffer, 0, _position);
      _pool.Return(_buffer);
   }

   public void Dispose()
   {
      if (_disposed) return;
      _disposed = true;

      Clear();
   }
}
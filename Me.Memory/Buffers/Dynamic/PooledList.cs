using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Me.Memory.Buffers.Dynamic;

[CollectionBuilder(typeof(PooledListCollectionBuilder), nameof(PooledListCollectionBuilder.Create))]
public sealed class PooledList<T> : IDisposable
{
   public Span<T> WrittenSpan
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _owner.Span[.._index];
   }

   public int Count
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _index;
   }
   
   public int Capacity
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _owner.Length;
   }

   public T this[int index]
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => WrittenSpan[index];
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => WrittenSpan[index] = value;
   }
   
   private MemoryOwner<T> _owner;
   private int _index;
   
   private bool _isDisposed;

   public PooledList(int initialCapacity)
   {
      _owner = new MemoryOwner<T>(initialCapacity);
      _index = 0;
   }

   public PooledList(ReadOnlySpan<T> values)
   {
      _owner = new MemoryOwner<T>(values.Length);
      _index = values.Length;
      
      values.CopyTo(_owner.Span);
   }

   public void AddRange(scoped in ReadOnlySpan<T> span)
   {
      if (span.IsEmpty) return;

      var needed = _index + span.Length;
      EnsureSize(needed);
      
      span.CopyTo(_owner.Span[_index..]);
      _index += span.Length;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Add(scoped in T item)
   {
      if (_owner.Length == _index)
      {
         EnsureSize(
            (int)Math.Min(
               (long)Math.Max(_owner.Length, 1) * 2, 
               int.MaxValue - 1));
      }

      ref var reference = ref MemoryMarshal.GetReference(_owner.Span);
      Unsafe.Add(ref reference, _index++) = item;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public int IndexOf(T item, IEqualityComparer<T>? cmp = null)
   {
      return WrittenSpan.IndexOf(item, cmp);
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public ref T GetByReference(int index)
   {
      ref var reference = ref MemoryMarshal.GetReference(_owner.Span);
      return ref Unsafe.Add(ref reference, index);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public bool Contains(T item, IEqualityComparer<T>? cmp = null)
   {
      return WrittenSpan.Contains(item, cmp);
   }
   
   /// <summary>
   /// Only resizes if the new capacity is greater than the current capacity.
   /// </summary>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Resize(int newCapacity)
   {
      EnsureSize(newCapacity);
   }
   
   private void EnsureSize(int size)
   {
      if (size <= _owner.Length 
          || _owner.TryResize(size))
      {
         return;
      }

      var lastOwner = _owner;
      _owner = new MemoryOwner<T>(size);
      
      lastOwner.Span[.._index].CopyTo(_owner.Span);
      lastOwner.Dispose();
   }
   
   public void Trim()
   {
      if (_index >= _owner.Length)
      {
         return;
      }

      var lastOwner = _owner;
      _owner = new MemoryOwner<T>(_index);
      
      lastOwner.Span[.._index].CopyTo(_owner.Span);
      lastOwner.Dispose();
   }
   
   public Span<T>.Enumerator GetEnumerator() => WrittenSpan.GetEnumerator();
   
   public void Dispose()
   {
      if (_isDisposed) return;
      _isDisposed = true;
      
      _owner.Dispose();

      _index = 0;
      _owner = default;
   }
}

public static class PooledListCollectionBuilder
{
   public static PooledList<T> Create<T>(ReadOnlySpan<T> values) => new(values);
}
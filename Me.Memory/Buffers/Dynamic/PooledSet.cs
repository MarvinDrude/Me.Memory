using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Me.Memory.Buffers.Dynamic;

[CollectionBuilder(typeof(PooledSetCollectionBuilder), nameof(PooledSetCollectionBuilder.Create))]
public sealed class PooledSet<T> : IDisposable
{
   public Span<T> WrittenSpan
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _list.WrittenSpan;
   }

   public int Count
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _list.Count;
   }
   
   public T this[int index]
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _list[index];
   }
   
   public T[] Array
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _list.Array;
   }
   
   private readonly PooledList<T> _list;
   
   public PooledSet(int initialCapacity)
   {
      _list = new PooledList<T>(initialCapacity);
   }
   
   public PooledSet(ReadOnlySpan<T> values)
   {
      _list = new PooledList<T>(values.Length);
      AddSpanRange(values);
   }
   
   public bool Add(scoped in T item)
   {
      if (_list.IndexOf(item) >= 0)
      {
         return false;
      }

      _list.Add(item);
      return true;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void AddRange(params T[] items)
   {
      AddSpanRange(items);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void AddRange(scoped in PooledSet<T> items)
   {
      AddSpanRange(items.WrittenSpan);
   }
   
   public void AddSpanRange(scoped in ReadOnlySpan<T> span)
   {
      foreach (var item in span)
      {
         Add(item);
      }
   }
   
   public bool ContainsAny(ICollection<T> collection)
   {
      foreach (ref var item in WrittenSpan)
      {
         if (collection.Contains(item))
         {
            return true;
         }
      }

      return false;
   }
   
   public ref T GetByReference(int index)
   {
      return ref _list.GetByReference(index);
   }
   
   public Span<T>.Enumerator GetEnumerator() => _list.WrittenSpan.GetEnumerator();
   
   public void Dispose()
   {
      _list.Dispose();
   } 
}

public static class PooledSetCollectionBuilder
{
   public static PooledSet<T> Create<T>(ReadOnlySpan<T> values) 
      where T : IEquatable<T> => new(values);
}
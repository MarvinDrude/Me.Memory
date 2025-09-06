using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Me.Memory.Collections;

[CollectionBuilder(typeof(SequenceArrayCollectionBuilder), nameof(SequenceArrayCollectionBuilder.Create))]
public readonly struct SequenceArray<T>
   : IEquatable<SequenceArray<T>>
{
   [field: AllowNull, MaybeNull]
   public T[] Array => field ?? throw new InvalidOperationException("Default struct invocation.");
   
   public Span<T> Span => Array.AsSpan();
   public Memory<T> Memory => Array.AsMemory();

   public SequenceArray(T[] buffer)
   {
      Array = buffer;
   }

   public SequenceArray(ReadOnlySpan<T> span)
   {
      Array = span.ToArray();
   }
   
   public bool Equals(SequenceArray<T> other)
   {
      return Array.AsSpan().SequenceEqual(other.Array.AsSpan());
   }

   public Span<T>.Enumerator GetEnumerator()
   {
      return Span.GetEnumerator();
   }

   public override bool Equals(object? obj)
   {
      return obj is SequenceArray<T> array && Equals(array);
   }

   public override int GetHashCode()
   {
      var hash = new HashCode();

      foreach (ref var item in Span)
      {
         hash.Add(item);
      }
      
      return hash.ToHashCode();
   }
   
   public static bool operator ==(SequenceArray<T> left, SequenceArray<T> right)
   {
      return left.Equals(right);
   }

   public static bool operator !=(SequenceArray<T> left, SequenceArray<T> right)
   {
      return !(left == right);
   }
}

public static class SequenceArrayCollectionBuilder
{
   public static SequenceArray<T> Create<T>(ReadOnlySpan<T> values) => new(values);
}
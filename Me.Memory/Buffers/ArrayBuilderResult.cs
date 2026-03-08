using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Me.Memory.Buffers;

public readonly struct ArrayBuilderResult<T>(ArrayBuilder<T>? builder) : IDisposable
{
   [MemberNotNullWhen(true, nameof(_builder))]
   public bool HasValue => _builder is not null;
   
   private readonly ArrayBuilder<T>? _builder = builder;

   public ReadOnlySpan<T> WrittenSpan
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get =>
         _builder is null 
            ? ReadOnlySpan<T>.Empty : _builder.WrittenSpan;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Dispose()
   {
      _builder?.Dispose();
   }
   
   public static ArrayBuilderResult<T> Empty => new(null);
   
   public static implicit operator ArrayBuilderResult<T>(ArrayBuilder<T>? builder) => new(builder);
}
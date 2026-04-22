using System.Diagnostics.CodeAnalysis;

namespace Me.Memory.Results;

public readonly struct VoidResult<TError>
{
   public readonly bool IsSuccess;
   
   [MemberNotNullWhen(false, nameof(Error))]
   public bool HasValue => IsSuccess;
   
   [MemberNotNullWhen(true, nameof(Error))]
   public bool Failed => !IsSuccess;
   
   public readonly TError? Error;

   public VoidResult()
   {
      IsSuccess = true;
   }

   public VoidResult(TError error)
   {
      IsSuccess = false;
      Error = error;
   }

   public override string ToString()
   {
      return HasValue ? $"SUCCESS" : $"ERROR: {Error.ToString()}";
   }
   
   public static implicit operator VoidResult<TError>(bool success) => new();
   public static implicit operator VoidResult<TError>(TError error) => new(error);
}
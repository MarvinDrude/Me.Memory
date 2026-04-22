using Me.Memory.Results;
using Me.Memory.Results.Errors;

namespace Me.Memory.Extensions;

public static class ResultExtensions
{
   extension<TResult>(Result<TResult, StringError> res)
   {
      public string ErrorMessage => res.Error.Detail;
   }
}
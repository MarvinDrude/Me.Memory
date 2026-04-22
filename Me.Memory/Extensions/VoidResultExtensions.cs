using Me.Memory.Results;
using Me.Memory.Results.Errors;

namespace Me.Memory.Extensions;

public static class VoidResultExtensions
{
   extension(VoidResult<StringError> res)
   {
      public string ErrorMessage => res.Error.Detail;
   }
}
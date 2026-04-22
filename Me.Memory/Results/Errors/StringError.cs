namespace Me.Memory.Results.Errors;

public readonly struct StringError(string detail)
{
   public readonly string Detail = detail;
}
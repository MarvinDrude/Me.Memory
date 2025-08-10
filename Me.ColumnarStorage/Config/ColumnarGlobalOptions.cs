namespace Me.ColumnarStorage.Config;

public sealed class ColumnarGlobalOptions
{
   public string BasePath { get; set; } = string.Empty;

   public string RelativePathSegments { get; set; } = @"\segments\";
}
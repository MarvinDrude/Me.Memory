namespace Me.Memory.Models.Structs;

public class StructLayoutInfo
{
   public required string Name { get; set; }
   
   public required int ByteSize { get; set; }

   public required StructFieldInfo[] Fields { get; set; }
   
   public class StructFieldInfo
   {
      public required string Name { get; set; }
      
      public bool IsPadding { get; set; }
      
      public int Offset { get; set; }
      
      public int ByteSize { get; set; }
   }
}
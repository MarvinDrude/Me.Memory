using System.Reflection;
using System.Runtime.InteropServices;
using Me.Memory.Buffers;
using Me.Memory.Models.Structs;

namespace Me.Memory.Extensions;

public static class StructExtensions
{
   extension<T>(T str)
      where T : struct
   {
      public static StructLayoutInfo GetStructLayout()
      {
         return StructLayoutCache<T>.Instance;
      }
      
      public static string GetStructLayoutString()
      {
         return StructLayoutCache<T>.InstanceString;
      }
   }
   
   private static class StructLayoutCache<T>
   {
      internal static readonly StructLayoutInfo Instance = GetLayoutInfo();
      internal static readonly string InstanceString = GetLayoutInfoString();

      private static StructLayoutInfo GetLayoutInfo()
      {
         var type = typeof(T);
         var totalSize = Marshal.SizeOf<T>();

         var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
         Array.Sort(fields, (a, b) => Marshal.OffsetOf(type, a.Name).ToInt32().CompareTo(Marshal.OffsetOf(type, b.Name).ToInt32()));
         
         using var builder = new ArrayBuilder<StructLayoutInfo.StructFieldInfo>(fields.Length + 10);
         var currentOffset = 0;

         foreach (var field in fields)
         {
            var fieldOffset = Marshal.OffsetOf(type, field.Name).ToInt32();
            var fieldSize = Marshal.SizeOf(field.FieldType);

            if (fieldOffset > currentOffset)
            {
               builder.Add(new StructLayoutInfo.StructFieldInfo()
               {
                  Name = "PADDING",
                  IsPadding = true,
                  Offset = currentOffset,
                  ByteSize = fieldOffset - currentOffset
               });
            }
            
            builder.Add(new StructLayoutInfo.StructFieldInfo()
            {
               Name = field.Name,
               Offset = fieldOffset,
               ByteSize = fieldSize
            });
            
            currentOffset = fieldOffset + fieldSize;
         }
         
         if (currentOffset < totalSize)
         {
            builder.Add(new StructLayoutInfo.StructFieldInfo()
            {
               Name = "PADDING",
               IsPadding = true,
               Offset = currentOffset,
               ByteSize = totalSize - currentOffset
            });
         }

         return new StructLayoutInfo()
         {
            Name = type.Name,
            ByteSize = totalSize,
            Fields = builder.WrittenSpan.ToArray()
         };
      }

      private static string GetLayoutInfoString()
      {
         var instance = Instance;
         var writer = new TextWriterIndentSlim(
            stackalloc char[512], stackalloc char[64]);

         try
         {
            writer.WriteLineInterpolated($"Struct: {instance.Name}, Size: {instance.ByteSize} bytes");
            writer.WriteLine("------");

            foreach (var field in instance.Fields)
            {
               writer.WriteInterpolated($"[{field.Offset:D3}...{(field.Offset + field.ByteSize - 1):D3}] ");
               writer.WriteLineInterpolated($"{field.Name}: Offset: {field.Offset}, Size: {field.ByteSize} bytes");
            }
            
            writer.WriteLine("------");
            return writer.ToString();
         }
         finally
         {
            writer.Dispose();
         }
      }
   }
}
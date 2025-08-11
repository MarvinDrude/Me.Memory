using System.Runtime.ExceptionServices;

namespace Me.Memory.Extensions;

public static class TaskExtensions
{
   extension(Task task)
   {
      public async Task WithAggregateException()
      {
         try
         {
            await task;
         }
         catch (Exception er)
         {
            if (task.Exception is not null) ExceptionDispatchInfo.Capture(er).Throw();
            throw;
         }
      }
   }
}
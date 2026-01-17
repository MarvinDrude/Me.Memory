using System.Threading.Channels;
using Me.Memory.Threading;

namespace Me.Memory.Tests.Threading;

public class WorkPoolTests
{
   [Test]
   public async Task EnqueueExecutesSyncFunctionAndReturnsResult()
   {
      int result;
      
      {
         await using var pool = new WorkPool(new WorkPoolOptions { MaxDegreeOfParallelism = 1 });
         // Act
         result = await pool.Enqueue(() => 42);
      }

      await Assert.That(result).IsEqualTo(42);
   }

   [Test]
   public async Task EnqueueExecutesAsyncFunctionAndReturnsResult()
   {
      string result;

      {
         await using var pool = new WorkPool(new WorkPoolOptions { MaxDegreeOfParallelism = 1 });

         // Act
         result = await pool.Enqueue(async ct =>
         {
            await Task.Delay(10, ct);
            return "Async Hi";
         });
      }

      await Assert.That(result).IsEqualTo("Async Hi");
   }

   [Test]
   public async Task ExceptionsPropagateToEnqueueTask()
   {
      await using var pool = new WorkPool(new WorkPoolOptions());
      
      await Assert.That(() =>
      {
         pool.Enqueue<int>(() => throw new InvalidOperationException("Boom"))
            .GetAwaiter().GetResult();
      }).Throws<InvalidOperationException>();
   }
   
   [Test]
   public async Task ExceptionsPropagateToEnqueueTaskAsync()
   {
      await using var pool = new WorkPool(new WorkPoolOptions());
      
      await Assert.That(() =>
      {
         pool.Enqueue((ct) =>
            {
               throw new InvalidOperationException("Boom");
               return Task.FromResult<int>(0);
            })
            .GetAwaiter().GetResult();
      }).Throws<InvalidOperationException>();
   }

   [Test]
   public async Task MultipleWorkersExecuteInParallel()
   {
      // Setup: 2 workers
      await using var pool = new WorkPool(new WorkPoolOptions { MaxDegreeOfParallelism = 2 });

      var tcs1 = new TaskCompletionSource();
      var tcs2 = new TaskCompletionSource();

      // Act: Enqueue two tasks that wait for each other (deadlock if single threaded) 
      // or simply block until we signal them.
      
      var task1 = pool.Enqueue(async ct =>
      {
         tcs1.SetResult(); // Signal I started
         await tcs2.Task;  // Wait for task2
         return 1;
      });

      var task2 = pool.Enqueue(async ct =>
      {
         tcs2.SetResult(); // Signal I started
         await tcs1.Task;  // Wait for task1
         return 2;
      });

      // Wait for both tasks to complete
      var results = await Task.WhenAll(task1, task2);

      await Assert.That(results).IsEquivalentTo([1, 2]);
   }

   [Test]
   public async Task CancellationAtEnqueueTimeCancelsTask()
   {
      await using var pool = new WorkPool(new WorkPoolOptions());
      using var cts = new CancellationTokenSource();
      
      await cts.CancelAsync();

      // Act & Assert
      // The task returned by Enqueue should be canceled immediately
      var task = pool.Enqueue(() => 1, cts.Token);

      await Assert.That(task.IsCanceled).IsTrue();
   }

   [Test]
   public async Task CompleteWaitsForPendingItems()
   {
      // Setup: 1 worker
      var pool = new WorkPool(new WorkPoolOptions { MaxDegreeOfParallelism = 1 });
      
      var tcs = new TaskCompletionSource<int>();
      
      // Enqueue an item that blocks
      var task = pool.Enqueue(async ct =>
      {
         await Task.Delay(50, ct); // Simulate work
         return 100;
      });

      // Act: Complete the pool (should signal writer complete but allow reader to drain)
      await pool.Complete();

      // Assert: The task should still finish successfully
      var result = await task;
      await Assert.That(result).IsEqualTo(100);
      
      // Dispose to clean up workers
      await pool.DisposeAsync();
   }

   [Test]
   public async Task EnqueueThrowsIfPoolNotAccepting()
   {
      var pool = new WorkPool(new WorkPoolOptions());
      await pool.Complete();

      // Act & Assert
      Assert.Throws<InvalidOperationException>(() =>
      {
         // We don't await the result, the call to Enqueue itself throws sync
         _ = pool.Enqueue(() => 1);
      });
      
      await pool.DisposeAsync();
   }

   [Test]
   public async Task DisposeAsyncCancelsRunningWork()
   {
      // Setup: 1 worker
      var pool = new WorkPool(new WorkPoolOptions { MaxDegreeOfParallelism = 1 });
      var tcs = new TaskCompletionSource<int>();

      // Enqueue a task that runs "forever" until canceled
      var task = pool.Enqueue(async ct =>
      {
         tcs.SetResult(0); // Signal we started
         await Task.Delay(-1, ct); // Wait indefinitely until CT triggers
         return 1;
      });

      await tcs.Task; // Ensure it's running

      // Act: Dispose should trigger the internal CancellationTokenSource
      await pool.DisposeAsync();

      // Assert: The user task should be canceled
      await Assert.That(async () => await task).Throws<TaskCanceledException>();
   }

   [Test]
   public async Task WorkPoolHandlesHighVolume()
   {
      // Smoke test for deadlocks/race conditions
      await using var pool = new WorkPool(new WorkPoolOptions { MaxDegreeOfParallelism = 4 });
      
      int count = 1000;
      var tasks = new Task<int>[count];

      for (int i = 0; i < count; i++)
      {
         int val = i;
         tasks[i] = pool.Enqueue(() => val);
      }

      var results = await Task.WhenAll(tasks);
      
      // Basic verification: sum of 0..999
      var expectedSum = count * (count - 1) / 2;
      await Assert.That(results.Sum()).IsEqualTo(expectedSum);
   }

   [Test]
   public async Task EnqueueWaitsIfChannelFull()
   {
      // Setup: Capacity 1
      var options = new WorkPoolOptions 
      { 
         Capacity = 1, 
         FullMode = BoundedChannelFullMode.Wait,
         MaxDegreeOfParallelism = 1
      };
      
      await using var pool = new WorkPool(options);
      
      var tcsBlocker = new TaskCompletionSource<int>();
      
      // 1. Occupy the worker
      _ = pool.Enqueue(async ct => await tcsBlocker.Task);
      
      // 2. Fill the channel (Capacity 1)
      _ = pool.Enqueue(() => 1);
      
      // 3. Next enqueue should block/wait because worker is busy and channel is full
      var enqueueTask = pool.Enqueue(() => 2);
      
      await Assert.That(enqueueTask.IsCompleted).IsFalse();

      // Unblock
      tcsBlocker.SetResult(1);
      
      var result = await enqueueTask;
      await Assert.That(result).IsEqualTo(2);
   }
}
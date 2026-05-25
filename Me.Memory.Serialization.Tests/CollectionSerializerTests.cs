#pragma warning disable CS8600, CS8604, CS8619

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Me.Memory.Buffers;
using Me.Memory.Serialization;
using Me.Memory.Serialization.Formatters.Collections;
using Me.Memory.Serialization.Formatters.Collections.Concurrent;
using Me.Memory.Serialization.Formatters.Collections.Immutable;
using Me.Memory.Serialization.Interfaces;

namespace Me.Memory.Serialization.Tests;

public class CollectionSerializerTests
{
   private static async Task TestSerializerDirect<T, TSerializer>(T value, int expectedSize, Func<T, T, Task> assertEqual)
      where TSerializer : ISerializer<T>
   {
      // 1. Calculate length
      var length = TSerializer.CalculateByteLength(in value);
      await Assert.That(length).IsEqualTo(expectedSize);
      
      // 2. Write and contiguous read
      byte[] buffer = new byte[expectedSize];
      var writer = new BufferWriter<byte>(buffer);
      var written = TSerializer.Write(ref writer, in value);
      await Assert.That(written).IsEqualTo(expectedSize);
      
      var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(buffer));
      var readSuccess = TSerializer.TryRead(ref reader, out T readValue);
      await Assert.That(readSuccess).IsTrue();
      await assertEqual(readValue, value);
   }

   [Test]
   public async Task TestArray2DSerializer()
   {
      int[,] arr = { { 1, 2 }, { 3, 4 } };
      int size = (sizeof(int) * 2) + (sizeof(int) * arr.Length);
      await TestSerializerDirect<int[,]?, Array2DSerializer<int>>(arr, size, async (read, expected) =>
      {
         await Assert.That(read!.GetLength(0)).IsEqualTo(expected!.GetLength(0));
         await Assert.That(read.GetLength(1)).IsEqualTo(expected.GetLength(1));
         await Assert.That(read[0, 0]).IsEqualTo(expected[0, 0]);
         await Assert.That(read[1, 1]).IsEqualTo(expected[1, 1]);
      });
   }

   [Test]
   public async Task TestArray4DSerializer()
   {
      int[,,,] arr = new int[2, 2, 2, 2];
      arr[0, 1, 0, 1] = 42;
      int size = (sizeof(int) * 4) + (sizeof(int) * arr.Length);
      await TestSerializerDirect<int[,,,]?, Array4DSerializer<int>>(arr, size, async (read, expected) =>
      {
         await Assert.That(read!.GetLength(0)).IsEqualTo(expected!.GetLength(0));
         await Assert.That(read[0, 1, 0, 1]).IsEqualTo(expected[0, 1, 0, 1]);
      });
   }

   [Test]
   public async Task TestArraySegmentSerializer()
   {
      int[] arr = { 1, 2, 3, 4, 5 };
      var segment = new ArraySegment<int>(arr, 1, 3);
      int size = sizeof(int) + (sizeof(int) * segment.Count);
      await TestSerializerDirect<ArraySegment<int>, ArraySegmentSerializer<int>>(segment, size, async (read, expected) =>
      {
         await Assert.That(read.Count).IsEqualTo(expected.Count);
         await Assert.That(read[0]).IsEqualTo(expected[0]);
         await Assert.That(read[2]).IsEqualTo(expected[2]);
      });
   }

   [Test]
   public async Task TestBitArraySerializer()
   {
      var bitArr = new BitArray(new[] { true, false, true, true, false });
      int size = sizeof(int) + 1; // 5 bits fits in 1 byte
      await TestSerializerDirect<BitArray?, BitArraySerializer>(bitArr, size, async (read, expected) =>
      {
         await Assert.That(read!.Length).IsEqualTo(expected!.Length);
         await Assert.That(read[0]).IsTrue();
         await Assert.That(read[1]).IsFalse();
      });
   }

   [Test]
   public async Task TestListSerializer()
   {
      var list = new List<int> { 10, 20, 30 };
      int size = sizeof(int) + (sizeof(int) * list.Count);
      await TestSerializerDirect<List<int>?, ListSerializer<int>>(list, size, async (read, expected) =>
      {
         await Assert.That(read!.Count).IsEqualTo(expected!.Count);
         await Assert.That(read[0]).IsEqualTo(expected[0]);
      });
   }

   [Test]
   public async Task TestLinkedListSerializer()
   {
      var list = new LinkedList<int>();
      list.AddLast(1);
      list.AddLast(2);
      int size = sizeof(int) + (sizeof(int) * list.Count);
      await TestSerializerDirect<LinkedList<int>?, LinkedListSerializer<int>>(list, size, async (read, expected) =>
      {
         await Assert.That(read!.Count).IsEqualTo(expected!.Count);
         await Assert.That(read.First!.Value).IsEqualTo(expected.First!.Value);
      });
   }

   [Test]
   public async Task TestQueueSerializer()
   {
      var q = new Queue<int>();
      q.Enqueue(100);
      q.Enqueue(200);
      int size = sizeof(int) + (sizeof(int) * q.Count);
      await TestSerializerDirect<Queue<int>?, QueueSerializer<int>>(q, size, async (read, expected) =>
      {
         await Assert.That(read!.Count).IsEqualTo(expected!.Count);
         await Assert.That(new Queue<int>(read).Dequeue()).IsEqualTo(100);
      });
   }

   [Test]
   public async Task TestStackSerializer()
   {
      var s = new Stack<int>();
      s.Push(10);
      s.Push(20);
      int size = sizeof(int) + (sizeof(int) * s.Count);
      await TestSerializerDirect<Stack<int>?, StackSerializer<int>>(s, size, async (read, expected) =>
      {
         await Assert.That(read!.Count).IsEqualTo(expected!.Count);
         var copy = new Stack<int>(new Stack<int>(read)); // reverse to preserve pop order for testing
         await Assert.That(copy.Pop()).IsEqualTo(20);
         await Assert.That(copy.Pop()).IsEqualTo(10);
      });
   }

   [Test]
   public async Task TestHashSetSerializer()
   {
      var set = new HashSet<int> { 1, 2, 3 };
      int size = sizeof(int) + (sizeof(int) * set.Count);
      await TestSerializerDirect<HashSet<int>?, HashSetSerializer<int>>(set, size, async (read, expected) =>
      {
         await Assert.That(read!.Count).IsEqualTo(expected!.Count);
         await Assert.That(read.Contains(2)).IsTrue();
      });
   }

   [Test]
   public async Task TestReadOnlyCollectionSerializer()
   {
      var list = new List<int> { 1, 2, 3 };
      var ro = new ReadOnlyCollection<int>(list);
      int size = sizeof(int) + (sizeof(int) * ro.Count);
      await TestSerializerDirect<ReadOnlyCollection<int>?, ReadOnlyCollectionSerializer<int>>(ro, size, async (read, expected) =>
      {
         await Assert.That(read!.Count).IsEqualTo(expected!.Count);
         await Assert.That(read[1]).IsEqualTo(expected[1]);
      });
   }

   [Test]
   public async Task TestSortedListSerializer()
   {
      var list = new SortedList<string, int> { { "b", 2 }, { "a", 1 } };
      int size = sizeof(int) 
         + (sizeof(int) + System.Text.Encoding.UTF8.GetByteCount("a")) + sizeof(int)
         + (sizeof(int) + System.Text.Encoding.UTF8.GetByteCount("b")) + sizeof(int);
      await TestSerializerDirect<SortedList<string, int>?, SortedListSerializer<string, int>>(list, size, async (read, expected) =>
      {
         await Assert.That(read!.Count).IsEqualTo(expected!.Count);
         await Assert.That(read.Keys[0]).IsEqualTo("a");
      });
   }

   [Test]
   public async Task TestDictionarySerializer()
   {
      var dict = new Dictionary<string, int> { { "hello", 1 } };
      int size = sizeof(int) + (sizeof(int) + System.Text.Encoding.UTF8.GetByteCount("hello")) + sizeof(int);
      await TestSerializerDirect<Dictionary<string, int>?, DictionarySerializer<string, int>>(dict, size, async (read, expected) =>
      {
         await Assert.That(read!["hello"]).IsEqualTo(expected!["hello"]);
      });
   }

   [Test]
   public async Task TestSortedDictionarySerializer()
   {
      var dict = new SortedDictionary<string, int> { { "hello", 1 } };
      int size = sizeof(int) + (sizeof(int) + System.Text.Encoding.UTF8.GetByteCount("hello")) + sizeof(int);
      await TestSerializerDirect<SortedDictionary<string, int>?, SortedDictionarySerializer<string, int>>(dict, size, async (read, expected) =>
      {
         await Assert.That(read!["hello"]).IsEqualTo(expected!["hello"]);
      });
   }

   [Test]
   public async Task TestReadOnlyDictionarySerializer()
   {
      var dict = new Dictionary<string, int> { { "hello", 1 } };
      var ro = new ReadOnlyDictionary<string, int>(dict);
      int size = sizeof(int) + (sizeof(int) + System.Text.Encoding.UTF8.GetByteCount("hello")) + sizeof(int);
      await TestSerializerDirect<ReadOnlyDictionary<string, int>?, ReadOnlyDictionarySerializer<string, int>>(ro, size, async (read, expected) =>
      {
         await Assert.That(read!["hello"]).IsEqualTo(expected!["hello"]);
      });
   }

   [Test]
   public async Task TestImmutableArraySerializer()
   {
      var arr = ImmutableArray.Create(1, 2, 3);
      int size = sizeof(int) + (sizeof(int) * arr.Length);
      await TestSerializerDirect<ImmutableArray<int>, ImmutableArraySerializer<int>>(arr, size, async (read, expected) =>
      {
         await Assert.That(read.Length).IsEqualTo(expected.Length);
         await Assert.That(read[0]).IsEqualTo(expected[0]);
      });
   }

   [Test]
   public async Task TestImmutableListSerializer()
   {
      var list = ImmutableList.Create(1, 2, 3);
      int size = sizeof(int) + (sizeof(int) * list.Count);
      await TestSerializerDirect<ImmutableList<int>?, ImmutableListSerializer<int>>(list, size, async (read, expected) =>
      {
         await Assert.That(read!.Count).IsEqualTo(expected!.Count);
         await Assert.That(read[0]).IsEqualTo(expected[0]);
      });
   }

   [Test]
   public async Task TestImmutableQueueSerializer()
   {
      var q = ImmutableQueue.Create(1, 2, 3);
      int size = sizeof(int) + (sizeof(int) * 3);
      await TestSerializerDirect<ImmutableQueue<int>?, ImmutableQueueSerializer<int>>(q, size, async (read, expected) =>
      {
         await Assert.That(read!.Peek()).IsEqualTo(expected!.Peek());
      });
   }

   [Test]
   public async Task TestImmutableStackSerializer()
   {
      var s = ImmutableStack.Create(10, 20);
      int size = sizeof(int) + (sizeof(int) * 2);
      await TestSerializerDirect<ImmutableStack<int>?, ImmutableStackSerializer<int>>(s, size, async (read, expected) =>
      {
         await Assert.That(read!.Peek()).IsEqualTo(expected!.Peek());
      });
   }

   [Test]
   public async Task TestImmutableHashSetSerializer()
   {
      var set = ImmutableHashSet.Create(1, 2, 3);
      int size = sizeof(int) + (sizeof(int) * set.Count);
      await TestSerializerDirect<ImmutableHashSet<int>?, ImmutableHashSetSerializer<int>>(set, size, async (read, expected) =>
      {
         await Assert.That(read!.Count).IsEqualTo(expected!.Count);
         await Assert.That(read.Contains(2)).IsTrue();
      });
   }

   [Test]
   public async Task TestImmutableSortedSetSerializer()
   {
      var set = ImmutableSortedSet.Create(1, 2, 3);
      int size = sizeof(int) + (sizeof(int) * set.Count);
      await TestSerializerDirect<ImmutableSortedSet<int>?, ImmutableSortedSetSerializer<int>>(set, size, async (read, expected) =>
      {
         await Assert.That(read!.Count).IsEqualTo(expected!.Count);
         await Assert.That(read.Contains(2)).IsTrue();
      });
   }

   [Test]
   public async Task TestImmutableDictionarySerializer()
   {
      var dict = ImmutableDictionary.CreateRange(new[] { KeyValuePair.Create("hello", 1) });
      int size = sizeof(int) + (sizeof(int) + System.Text.Encoding.UTF8.GetByteCount("hello")) + sizeof(int);
      await TestSerializerDirect<ImmutableDictionary<string, int>?, ImmutableDictionarySerializer<string, int>>(dict, size, async (read, expected) =>
      {
         await Assert.That(read!["hello"]).IsEqualTo(expected!["hello"]);
      });
   }

   [Test]
   public async Task TestImmutableSortedDictionarySerializer()
   {
      var dict = ImmutableSortedDictionary.CreateRange(new[] { KeyValuePair.Create("hello", 1) });
      int size = sizeof(int) + (sizeof(int) + System.Text.Encoding.UTF8.GetByteCount("hello")) + sizeof(int);
      await TestSerializerDirect<ImmutableSortedDictionary<string, int>?, ImmutableSortedDictionarySerializer<string, int>>(dict, size, async (read, expected) =>
      {
         await Assert.That(read!["hello"]).IsEqualTo(expected!["hello"]);
      });
   }

   [Test]
   public async Task TestConcurrentBagSerializer()
   {
      var bag = new ConcurrentBag<int> { 1, 2, 3 };
      int size = sizeof(int) + (sizeof(int) * bag.Count);
      await TestSerializerDirect<ConcurrentBag<int>?, ConcurrentBagSerializer<int>>(bag, size, async (read, expected) =>
      {
         await Assert.That(read!.Count).IsEqualTo(expected!.Count);
      });
   }

   [Test]
   public async Task TestConcurrentQueueSerializer()
   {
      var q = new ConcurrentQueue<int>();
      q.Enqueue(1);
      q.Enqueue(2);
      int size = sizeof(int) + (sizeof(int) * q.Count);
      await TestSerializerDirect<ConcurrentQueue<int>?, ConcurrentQueueSerializer<int>>(q, size, async (read, expected) =>
      {
         await Assert.That(read!.Count).IsEqualTo(expected!.Count);
      });
   }

   [Test]
   public async Task TestConcurrentStackSerializer()
   {
      var s = new ConcurrentStack<int>();
      s.Push(10);
      s.Push(20);
      int size = sizeof(int) + (sizeof(int) * s.Count);
      await TestSerializerDirect<ConcurrentStack<int>?, ConcurrentStackSerializer<int>>(s, size, async (read, expected) =>
      {
         await Assert.That(read!.Count).IsEqualTo(expected!.Count);
         await Assert.That(read.TryPop(out var val) && val == 20).IsTrue();
      });
   }

   [Test]
   public async Task TestConcurrentDictionarySerializer()
   {
      var dict = new ConcurrentDictionary<string, int>();
      dict.TryAdd("hello", 1);
      int size = sizeof(int) + (sizeof(int) + System.Text.Encoding.UTF8.GetByteCount("hello")) + sizeof(int);
      await TestSerializerDirect<ConcurrentDictionary<string, int>?, ConcurrentDictionarySerializer<string, int>>(dict, size, async (read, expected) =>
      {
         await Assert.That(read!["hello"]).IsEqualTo(expected!["hello"]);
      });
   }
}

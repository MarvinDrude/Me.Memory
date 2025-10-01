using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Me.Memory.Buffers;

namespace Me.Memory.Benchmarks.Buffers;

[SimpleJob(RunStrategy.Throughput, iterationCount: 1)]
[MinColumn, MaxColumn, MeanColumn, MedianColumn, MemoryDiagnoser]
public class BufferWriterBench
{
   private const byte _smallCallsCount = 255;
   private const int _smallCallsAllocCount = 16_000;
   
   [Benchmark]
   public byte[] LotsOfSmallCallsBufferWriter()
   {
      using var writer = new BufferWriter<byte>(stackalloc byte[_smallCallsCount]);

      for (byte e = 0; e < _smallCallsCount; e++)
      {
         writer.Add(e);
      }
      
      return writer.WrittenSpan.ToArray();
   }

   [Benchmark]
   public byte[] LotsOfSmallCallsMemoryStream()
   {
      using var memoryStream = new MemoryStream();
      
      for (byte e = 0; e < _smallCallsCount; e++)
      {
         memoryStream.WriteByte(e);
      }
      
      return memoryStream.ToArray();
   }
   
   [Benchmark]
   public byte[] LotsOfSmallCallsList()
   {
      List<byte> bytes = [];
      
      for (byte e = 0; e < _smallCallsCount; e++)
      {
         bytes.Add(e);
      }
      
      return bytes.ToArray();
   }
   
   [Benchmark]
   public byte[] LotsOfSmallCallsAllocBufferWriter()
   {
      using var writer = new BufferWriter<byte>(stackalloc byte[1024], 1024 * 5);

      for (var e = 0; e < _smallCallsAllocCount; e++)
      {
         writer.Add((byte)(e % byte.MaxValue));
      }
      
      return writer.WrittenSpan.ToArray();
   }
   
   [Benchmark]
   public byte[] LotsOfSmallCallsAllocMemoryStream()
   {
      using var memoryStream = new MemoryStream();
      
      for (var e = 0; e < _smallCallsAllocCount; e++)
      {
         memoryStream.WriteByte((byte)(e % byte.MaxValue));
      }
      
      return memoryStream.ToArray();
   }
   
   [Benchmark]
   public byte[] LotsOfSmallCallsAllocList()
   {
      List<byte> bytes = [];
      
      for (var e = 0; e < _smallCallsAllocCount; e++)
      {
         bytes.Add((byte)(e % byte.MaxValue));
      }
      
      return bytes.ToArray();
   }
}
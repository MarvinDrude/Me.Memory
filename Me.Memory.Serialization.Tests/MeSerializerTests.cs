using System;
using System.Buffers;
using System.Threading.Tasks;
using Me.Memory.Buffers;
using Me.Memory.Serialization;

namespace Me.Memory.Serialization.Tests;

public class MeSerializerTests
{
   private struct SpanTestResult
   {
      public bool Success;
      public int BytesWritten;
      public byte[] Data;
      public Exception? Exception;
   }

   private static SpanTestResult TestSerializeSpanHelper(string? value, int spanSize, bool trySerialize)
   {
      Span<byte> destination = new byte[spanSize];
      try
      {
         if (trySerialize)
         {
            var success = MeSerializer.TrySerialize<string?>(value, destination, out var written);
            return new SpanTestResult 
            { 
               Success = success, 
               BytesWritten = written, 
               Data = destination[..written].ToArray() 
            };
         }
         else
         {
            var written = MeSerializer.Serialize<string?>(value, destination);
            return new SpanTestResult 
            { 
               Success = true, 
               BytesWritten = written, 
               Data = destination[..written].ToArray() 
            };
         }
      }
      catch (Exception ex)
      {
         return new SpanTestResult { Exception = ex };
      }
   }

   private static SpanTestResult TestSerializeWithoutPrecalcSpanHelper(string? value, int spanSize, bool trySerialize)
   {
      Span<byte> destination = new byte[spanSize];
      try
      {
         if (trySerialize)
         {
            var success = MeSerializer.TrySerializeWithoutPrecalculation<string?>(value, destination, out var written);
            return new SpanTestResult 
            { 
               Success = success, 
               BytesWritten = written, 
               Data = destination[..written].ToArray() 
            };
         }
         else
         {
            var written = MeSerializer.SerializeWithoutPrecalculation<string?>(value, destination);
            return new SpanTestResult 
            { 
               Success = true, 
               BytesWritten = written, 
               Data = destination[..written].ToArray() 
            };
         }
      }
      catch (Exception ex)
      {
         return new SpanTestResult { Exception = ex };
      }
   }

   [Test]
   public async Task TestCalculateByteLength()
   {
      var testValue = "Hello, World!";
      var expectedSize = sizeof(int) + System.Text.Encoding.UTF8.GetByteCount(testValue);
      var calculated = MeSerializer.CalculateByteLength<string?>(testValue);
      await Assert.That(calculated).IsEqualTo(expectedSize);
   }

   [Test]
   public async Task TestSerializeIntoSpan()
   {
      var testValue = "SpanSerialization";
      var expectedSize = sizeof(int) + System.Text.Encoding.UTF8.GetByteCount(testValue);
      
      // Fits exactly
      var result = TestSerializeSpanHelper(testValue, expectedSize, trySerialize: false);
      await Assert.That(result.Success).IsTrue();
      await Assert.That(result.BytesWritten).IsEqualTo(expectedSize);
      await Assert.That(result.Exception).IsNull();
      
      // Deserialize to verify
      var success = MeSerializer.TryDeserialize<string?>(result.Data, out var deserialized);
      await Assert.That(success).IsTrue();
      await Assert.That(deserialized).IsEqualTo(testValue);

      // Too small span throws
      var resultFailed = TestSerializeSpanHelper(testValue, expectedSize - 1, trySerialize: false);
      await Assert.That(resultFailed.Exception is ArgumentException).IsTrue();
   }

   [Test]
   public async Task TestTrySerializeIntoSpan()
   {
      var testValue = "TrySpanSerialization";
      var expectedSize = sizeof(int) + System.Text.Encoding.UTF8.GetByteCount(testValue);
      
      // Fits exactly
      var result = TestSerializeSpanHelper(testValue, expectedSize, trySerialize: true);
      await Assert.That(result.Success).IsTrue();
      await Assert.That(result.BytesWritten).IsEqualTo(expectedSize);
      await Assert.That(result.Exception).IsNull();
      
      // Too small span returns false
      var resultFailed = TestSerializeSpanHelper(testValue, expectedSize - 1, trySerialize: true);
      await Assert.That(resultFailed.Success).IsFalse();
      await Assert.That(resultFailed.BytesWritten).IsEqualTo(0);
      await Assert.That(resultFailed.Exception).IsNull();
   }

   [Test]
   public async Task TestSerializeWithoutPrecalculationSpan()
   {
      var testValue = "NoPrecalcSpan";
      var expectedSize = sizeof(int) + System.Text.Encoding.UTF8.GetByteCount(testValue);

      // Fits exactly
      var result = TestSerializeWithoutPrecalcSpanHelper(testValue, expectedSize, trySerialize: false);
      await Assert.That(result.Success).IsTrue();
      await Assert.That(result.BytesWritten).IsEqualTo(expectedSize);
      await Assert.That(result.Exception).IsNull();
      
      // Deserialize to verify
      var deserialized = MeSerializer.Deserialize<string?>(result.Data);
      await Assert.That(deserialized).IsEqualTo(testValue);

      // Too small span throws
      var resultFailed = TestSerializeWithoutPrecalcSpanHelper(testValue, expectedSize - 1, trySerialize: false);
      await Assert.That(resultFailed.Exception is ArgumentException).IsTrue();
   }

   [Test]
   public async Task TestTrySerializeWithoutPrecalculationSpan()
   {
      var testValue = "TryNoPrecalcSpan";
      var expectedSize = sizeof(int) + System.Text.Encoding.UTF8.GetByteCount(testValue);

      // Fits exactly
      var result = TestSerializeWithoutPrecalcSpanHelper(testValue, expectedSize, trySerialize: true);
      await Assert.That(result.Success).IsTrue();
      await Assert.That(result.BytesWritten).IsEqualTo(expectedSize);
      await Assert.That(result.Exception).IsNull();

      // Too small span returns false
      var resultFailed = TestSerializeWithoutPrecalcSpanHelper(testValue, expectedSize - 1, trySerialize: true);
      await Assert.That(resultFailed.Success).IsFalse();
      await Assert.That(resultFailed.BytesWritten).IsEqualTo(0);
      await Assert.That(resultFailed.Exception).IsNull();
   }

   [Test]
   public async Task TestSerializeWithoutPrecalculationArray()
   {
      var testValue = "NoPrecalcArray";
      var expectedSize = sizeof(int) + System.Text.Encoding.UTF8.GetByteCount(testValue);

      // Uses default initial capacity
      var bytesDefault = MeSerializer.SerializeWithoutPrecalculation<string?>(testValue);
      await Assert.That(bytesDefault.Length).IsEqualTo(expectedSize);
      var deserializedDefault = MeSerializer.Deserialize<string?>(bytesDefault);
      await Assert.That(deserializedDefault).IsEqualTo(testValue);

      // Uses custom initial capacity
      var bytesCustom = MeSerializer.SerializeWithoutPrecalculation<string?>(testValue, 4);
      await Assert.That(bytesCustom.Length).IsEqualTo(expectedSize);
      var deserializedCustom = MeSerializer.Deserialize<string?>(bytesCustom);
      await Assert.That(deserializedCustom).IsEqualTo(testValue);
   }

   [Test]
   public async Task TestDeserializeTryAndThrowing()
   {
      var testValue = "DeserializationTesting";
      var bytes = MeSerializer.Serialize<string?>(testValue);
      var sequence = new ReadOnlySequence<byte>(bytes);

      // 1. TryDeserialize with ReadOnlySequence<byte>
      var successSeq = MeSerializer.TryDeserialize<string?>(sequence, out var valSeq);
      await Assert.That(successSeq).IsTrue();
      await Assert.That(valSeq).IsEqualTo(testValue);

      // 2. TryDeserialize with byte[]
      var successArr = MeSerializer.TryDeserialize<string?>(bytes, out var valArr);
      await Assert.That(successArr).IsTrue();
      await Assert.That(valArr).IsEqualTo(testValue);

      // 3. Deserialize throwing with ReadOnlyMemory<byte>
      var valMem = MeSerializer.Deserialize<string?>(new ReadOnlyMemory<byte>(bytes));
      await Assert.That(valMem).IsEqualTo(testValue);

      // 4. Deserialize throwing with byte[]
      var valArrThrowing = MeSerializer.Deserialize<string?>(bytes);
      await Assert.That(valArrThrowing).IsEqualTo(testValue);

      // 5. Deserialize throwing with ReadOnlySequence<byte>
      var valSeqThrowing = MeSerializer.Deserialize<string?>(sequence);
      await Assert.That(valSeqThrowing).IsEqualTo(testValue);

      // 6. Failures
      byte[] invalidBytes = [255, 255, 255]; // Bad presence/length flags (too small for standard string reading)
      
      var tryFail = MeSerializer.TryDeserialize<string?>(invalidBytes, out _);
      await Assert.That(tryFail).IsFalse();

      await Assert.That(() => MeSerializer.Deserialize<string?>(invalidBytes))
         .Throws<InvalidOperationException>();
   }
}

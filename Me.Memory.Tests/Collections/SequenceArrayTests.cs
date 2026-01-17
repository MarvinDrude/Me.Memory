using Me.Memory.Collections;

namespace Me.Memory.Tests.Collections;

public class SequenceArrayTests
{
   [Test]
   public async Task ConstructorStoresArrayCorrectly()
   {
      int[] result;

      {
         var input = new[] { 1, 2, 3 };
         var seq = new SequenceArray<int>(input);
         result = seq.Array;
      }

      int[] expected = [1, 2, 3];
      // We check if the backing array content matches
      await Assert.That(result).IsEquivalentTo(expected);
   }

   [Test]
   public async Task ConstructorFromSpanCopiesData()
   {
      int[] result;

      {
         ReadOnlySpan<int> span = [10, 20];
         var seq = new SequenceArray<int>(span);
         result = seq.Array;
      }

      int[] expected = [10, 20];
      await Assert.That(result).IsEquivalentTo(expected);
   }

   [Test]
   public async Task DefaultStructThrowsOnAccess()
   {
      await Assert.That(() =>
      {
         SequenceArray<int> def = default;
         var arr = def.Array;
      }).Throws<InvalidOperationException>();
   }

   [Test]
   public async Task SequenceEqualityReturnsTrueForSameContent()
   {
      bool isEqual;
      bool isOperatorEqual;

      {
         var seq1 = new SequenceArray<int>([1, 2, 3]);
         var seq2 = new SequenceArray<int>([1, 2, 3]);

         isEqual = seq1.Equals(seq2);
         isOperatorEqual = seq1 == seq2;
      }

      await Assert.That(isEqual).IsTrue();
      await Assert.That(isOperatorEqual).IsTrue();
   }

   [Test]
   public async Task SequenceEqualityReturnsFalseForDifferentContent()
   {
      bool isEqual;
      bool isOperatorEqual;

      {
         var seq1 = new SequenceArray<int>([1, 2, 3]);
         var seq2 = new SequenceArray<int>([1, 2, 4]); // Different last element

         isEqual = seq1.Equals(seq2);
         isOperatorEqual = seq1 == seq2;
      }

      await Assert.That(isEqual).IsFalse();
      await Assert.That(isOperatorEqual).IsFalse();
   }

   [Test]
   public async Task GetHashCodeReturnsSameValueForSameContent()
   {
      int hash1;
      int hash2;

      {
         var seq1 = new SequenceArray<byte>([0xAA, 0xBB]);
         var seq2 = new SequenceArray<byte>([0xAA, 0xBB]);

         hash1 = seq1.GetHashCode();
         hash2 = seq2.GetHashCode();
      }

      await Assert.That(hash1).IsEqualTo(hash2);
   }

   [Test]
   public async Task EnumeratorIteratesCorrectly()
   {
      List<int> resultList = [];

      {
         var seq = new SequenceArray<int>([1, 2, 3]);
         foreach (var item in seq)
         {
            resultList.Add(item);
         }
      }

      var result = resultList.ToArray();
      int[] expected = [1, 2, 3];
      
      await Assert.That(result).IsEquivalentTo(expected);
   }

   [Test]
   public async Task CollectionExpressionCreatesSequenceArray()
   {
      SequenceArray<int> seq;

      {
         // Testing the CollectionBuilder attribute
         seq = [1, 2, 3];
      }

      int[] expected = [1, 2, 3];
      // Verify contents directly via Span/Array
      await Assert.That(seq.Array).IsEquivalentTo(expected);
   }

   [Test]
   public async Task ByteContentComparisonWorks()
   {
      // Specific test for byte arrays as requested in previous steps
      byte[] result;

      {
         SequenceArray<byte> seq = [0x01, 0x02];
         result = seq.Array;
      }

      byte[] expected = [0x01, 0x02];
      await Assert.That(result).IsEquivalentTo(expected);
   }
}
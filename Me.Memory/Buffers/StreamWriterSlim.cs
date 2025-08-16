using System.Runtime.InteropServices;

namespace Me.Memory.Buffers;

/// <summary>
/// A lightweight ref struct for writing data to a <see cref="Stream"/> in an efficient manner,
/// optimized for scenarios where memory allocation needs to be minimized.
/// </summary>
/// <remarks>
/// This struct utilizes a buffering mechanism internally to reduce the number of writes to
/// the underlying stream. It is designed for high-performance scenarios such as file IO or
/// network communication, where frequent writes to the stream may introduce a significant
/// overhead. The buffer size is configurable and defaults to 1MB, but may be adjusted depending
/// on the use case.
/// </remarks>
/// <threadsafety>
/// This struct is not thread-safe and is intended to be used within a single thread context
/// due to its ref struct nature and lack of synchronization mechanisms.
/// </threadsafety>
/// <disposal>
/// Proper disposal of this struct is essential to ensure any buffered data is flushed to
/// the underlying stream and unmanaged resources are released. It is recommended to use this
/// struct within a `using` block or explicitly call <see cref="Dispose"/> to enforce cleanup.
/// </disposal>
/// <performance>
/// For optimal performance, ensure that the chunk size is appropriately configured based on
/// the expected size of incoming data. Very small chunk sizes may lead to frequent flushing,
/// while excessively large chunk sizes may consume unnecessary memory.
/// </performance>
/// <example>
/// Example usage scenarios include:
/// - Writing large files piece by piece with minimal allocations.
/// Note that usage examples are not provided in this documentation by design.
/// </example>
/// <seealso cref="MemoryOwner{T}"/>
/// <seealso cref="BufferWriter{T}"/>
[StructLayout(LayoutKind.Auto)]
public ref struct StreamWriterSlim : IDisposable
{
   private readonly Stream _stream;
   private readonly int _chunkSize;

   private MemoryOwner<byte> _owner;
   private BufferWriter<byte> _writer;

   private bool _isDisposed = false;
   
   /// <summary>
   /// For FileStreams, use file buffer size of 1 and FileOptions.None
   /// </summary>
   public StreamWriterSlim(
      Stream stream,
      int chunkSize = 1024 * 1024)
   {
      _stream = stream;
      _chunkSize = chunkSize;
      
      _owner = new MemoryOwner<byte>(chunkSize);
      _writer = new BufferWriter<byte>(_owner.Span);
   }
   
   public void Write(scoped ReadOnlySpan<byte> span)
   {
      if (NeedsFlushing(span.Length))
      {
         Flush();
      }
      
      if (span.Length > _chunkSize)
      {
         _stream.Write(span);
         return;
      }
      
      _writer.Write(span);
   }
   
   public void Add(byte value)
   {
      if (NeedsFlushing(1))
      {
         Flush();
      }
      
      _writer.Add(value);
   }
   
   public Span<byte> AcquireSpan(int length)
   {
      if (NeedsFlushing(length))
      {
         Flush();
         
         if (NeedsFlushing(length)) 
            throw new InvalidOperationException("Required span size exceeds chunk size");
      }
      
      return _writer.AcquireSpan(length, true);
   }

   public void Flush()
   {
      if (_writer.Position <= 0)
      {
         return;
      }

      _stream.Write(_writer.WrittenSpan);
      _writer.Position = 0;
      
      _stream.Flush();
   }
   
   public bool NeedsFlushing(int newAddition)
   {
      return newAddition + _writer.Position >= _chunkSize;
   }
   
   public void Dispose()
   {
      if (_isDisposed) return;
      _isDisposed = true;

      Flush();
      
      _writer.Dispose();
      _owner.Dispose();
   }
}
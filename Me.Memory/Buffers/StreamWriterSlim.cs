using System.Runtime.InteropServices;

namespace Me.Memory.Buffers;

[StructLayout(LayoutKind.Auto)]
public ref struct StreamWriterSlim : IDisposable
{
   private readonly Stream _stream;
   private readonly int _chunkSize;

   private readonly MemoryOwner<byte> _owner;
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Me.Memory.Buffers;

[StructLayout(LayoutKind.Auto)]
public ref struct StreamReaderSlim : IDisposable
{
   public int BytesAvailable
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _end - _start;
   }
   
   private readonly Stream _stream;
   private readonly MemoryOwner<byte> _owner;

   private readonly Span<byte> _buffer;

   private int _start;
   private int _end;

   private bool _isDisposed = false;

   /// <summary>
   /// For FileStreams, use file buffer size of 1 and FileOptions.SequentialScan
   /// </summary>
   public StreamReaderSlim(
      Stream stream,
      int chunkSize = 1024 * 1024)
   {
      _stream = stream;
      
      _owner = new MemoryOwner<byte>(chunkSize);
      _buffer = _owner.Span;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public ReadOnlySpan<byte> AcquireSpan(int length)
   {
      Ensure(length);
      
      var span = _buffer.Slice(_start, length);
      _start += length;
      
      return span;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public byte ReadByte()
   {
      Ensure(1);
      
      var value = _buffer[_start];
      _start++;

      return value;
   }
   
   public void Ensure(int needed)
   {
      if (needed <= BytesAvailable) return;
      ArgumentOutOfRangeException.ThrowIfGreaterThan(needed, _buffer.Length);

      var additionalNeeded = needed - BytesAvailable;
      if (_buffer.Length - _end < additionalNeeded)
      {
         if (BytesAvailable > 0)
         {
            _buffer.Slice(_start, BytesAvailable).CopyTo(_buffer);
            
            _end = BytesAvailable;
            _start = 0;
         }
         else
         {
            _start = _end = 0;
         }
      }
      
      while (BytesAvailable < needed)
      {
         var read = _stream.Read(_buffer[_end..]);
         if (read <= 0) throw new EndOfStreamException();
         _end += read;
      }
   }
   
   public void Dispose()
   {
      if (_isDisposed) return;
      _isDisposed = true;
      
      _owner.Dispose();
   }
}
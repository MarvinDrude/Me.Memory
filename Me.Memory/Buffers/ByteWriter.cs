using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Me.Memory.Extensions;

namespace Me.Memory.Buffers;

[StructLayout(LayoutKind.Auto)]
public ref struct ByteWriter : IDisposable
{
   public ReadOnlySpan<byte> WrittenSpan
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => !_isStream ? _writer.WrittenSpan : throw new InvalidOperationException("This ByteWriter is stream based.");
   }

   public int Position
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get
      {
         ArgumentOutOfRangeException.ThrowIfEqual(_isStream, true, nameof(_writer));
         return _writer.Position;
      }
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set
      {
         ArgumentOutOfRangeException.ThrowIfEqual(_isStream, true, nameof(_writer));
         _writer.Position = value;
      }
   }
   
   private BufferWriter<byte> _writer;
   private StreamWriterSlim _streamWriter;

   private readonly bool _isStream;
   
   public ByteWriter(
      Span<byte> buffer,
      int initialMinGrowCapacity = 512)
   {
      _writer = new BufferWriter<byte>(buffer, initialMinGrowCapacity);
      _isStream = false;
   }

   public ByteWriter(
      Stream stream, 
      int chunkSize = 1024 * 1024)
   {
      _streamWriter = new StreamWriterSlim(stream, chunkSize);
      _isStream = true;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public int WriteBigEndian<T>(T value)
      where T : unmanaged
   {
      var size = Unsafe.SizeOf<T>();
      var span = AcquireSpan(size);

      value.WriteBigEndian(span);
      return size;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public int WriteLittleEndian<T>(T value)
      where T : unmanaged
   {
      var size = Unsafe.SizeOf<T>();
      var span = AcquireSpan(size);

      value.WriteLittleEndian(span);
      return size;
   }
   
   /// <summary>
   /// Only meant for intra process / memory to memory since it's just a recast.
   /// Same endianness required too.
   /// </summary>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public int WriteStringRaw(scoped ReadOnlySpan<char> text)
   {
      var rawBytes = MemoryMarshal.AsBytes(text);

      if (!_isStream)
      {
         _writer.Write(rawBytes);
      }
      else
      {
         _streamWriter.Write(rawBytes);
      }
      
      return rawBytes.Length;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public int WriteString(scoped ReadOnlySpan<char> text)
   {
      return WriteString(text, Encoding.UTF8);
   }
   
   public int WriteString(scoped ReadOnlySpan<char> text, Encoding encoding)
   {
      var size = encoding.GetByteCount(text);
      var span = AcquireSpan(size);

      var written = encoding.GetBytes(text, span);
      ArgumentOutOfRangeException.ThrowIfNotEqual(size, written);
      
      return written;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void WriteByte(byte value)
   {
      if (!_isStream)
      {
         _writer.Add(value);
      }
      else
      {
         _streamWriter.Add(value);
      }
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void WriteBytes(ReadOnlySpan<byte> buffer)
   {
      if (!_isStream)
      {
         _writer.Write(buffer);
      }
      else
      {
         _streamWriter.Write(buffer);
      }
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void WriteBytes(Span<byte> buffer)
   {
      if (!_isStream)
      {
         _writer.Write(buffer);
      }
      else
      {
         _streamWriter.Write(buffer);
      }
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Fill(byte value)
   {
      ArgumentOutOfRangeException.ThrowIfEqual(_isStream, true, nameof(_writer));
      _writer.Fill(value);
   }
   
   public void Flush()
   {
      if (_isStream)
      {
         _streamWriter.Flush();
      }
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private Span<byte> AcquireSpan(int length)
   {
      return _isStream 
         ? _streamWriter.AcquireSpan(length) 
         : _writer.AcquireSpan(length);
   }
   
   public void Dispose()
   {
      if (_isStream)
      {
         _streamWriter.Dispose();
      }
      else
      {
         _writer.Dispose();
      }
   }
}
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Me.Memory.Extensions;

namespace Me.Memory.Buffers;

[StructLayout(LayoutKind.Auto)]
public ref struct ByteReader
{
   public int BytesRemaining
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get
      {
         ArgumentOutOfRangeException.ThrowIfEqual(_isStream, true, nameof(_buffer));
         return _buffer.Length - (int)_position;
      }
   }

   public int Position
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get
      {
         ArgumentOutOfRangeException.ThrowIfEqual(_isStream, true, nameof(_buffer));
         return (int)_position;
      }
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set
      {
         ArgumentOutOfRangeException.ThrowIfEqual(_isStream, true, nameof(_buffer));
         _position = value;
      }
   }
   
   private readonly ReadOnlySpan<byte> _buffer;
   private StreamReaderSlim _streamReader;

   private readonly bool _isStream;
   private long _position;
   
   public ByteReader(ReadOnlySpan<byte> buffer)
   {
      _buffer = buffer;
      _position = 0;
      
      _isStream = false;
   }

   public ByteReader(
      Stream stream, 
      int chunkSize = 1024 * 1024)
   {
      _streamReader = new StreamReaderSlim(stream, chunkSize);
      _position = 0;
      
      _isStream = true;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public T ReadLittleEndian<T>() 
      where T : unmanaged
   {
      var size = Unsafe.SizeOf<T>();
      var span = AcquireSpan(size);
      _position += size;

      return span.ReadLittleEndian<T>(out _);
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public T ReadBigEndian<T>() 
      where T : unmanaged
   {
      var size = Unsafe.SizeOf<T>();
      var span = AcquireSpan(size);
      _position += size;

      return span.ReadBigEndian<T>(out _);
   }
   
   /// <summary>
   /// Only meant for intra process / memory to memory since it's just a recast.
   /// Same endianness required too.
   /// </summary>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public string ReadStringRawToString(int size)
   {
      var chars = ReadStringRaw(size);
      return new string(chars);
   }
   
   /// <summary>
   /// Only meant for intra process / memory to memory since it's just a recast.
   /// Same endianness required too. (this is not a copy and is based on the underlying bytes)
   /// </summary>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public ReadOnlySpan<char> ReadStringRaw(int size)
   {
      if (size == 0)
      {
         return [];
      }
      
      var raw = AcquireSpan(size);
      var chars = MemoryMarshal.Cast<byte, char>(raw);

      _position += size;
      return chars;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public string ReadString(int size, Encoding encoding)
   {
      var raw = AcquireSpan(size);
      var str = encoding.GetString(raw);
      
      _position += size;
      return str;
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public ReadOnlySpan<byte> ReadBytes(int length)
   {
      var span = AcquireSpan(length);
      _position += length;

      return span;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public byte ReadByte()
   {
      if (_isStream)
      {
         _position++;
         return _streamReader.ReadByte();
      }
      else
      {
         var position = (int)_position;
         _position++;
         
         return _buffer[position];
      }
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private ReadOnlySpan<byte> AcquireSpan(int length)
   {
      return _isStream 
         ? _streamReader.AcquireSpan(length) 
         : _buffer.Slice((int)_position, length);
   }
}
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Me.Memory.Buffers;

[StructLayout(LayoutKind.Auto)]
public ref partial struct TextWriterIndentSlim : IDisposable
{
   public ReadOnlySpan<char> WrittenSpan
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _buffer.WrittenSpan;
   }

   public char IndentCharacter
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get;
   }

   public char NewLineCharacter
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get;
   }

   public int IndentSize
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get;
   }

   public int CurrentIndentLevel
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _currentLevel;
   }

   private BufferWriter<char> _indentCache;
   private ReadOnlySpan<char> _currentLevelBuffer;
   private int _currentLevel;

   private BufferWriter<char> _buffer;

   public TextWriterIndentSlim(
      Span<char> buffer,
      Span<char> indentBuffer,
      int indentSize = DefaultIndentSize,
      char indentChar = DefaultIndent,
      char newLineChar = DefaultNewLine,
      int initialMinGrowCapacity = 1024)
   {
      NewLineCharacter = newLineChar;
      IndentCharacter = indentChar;
      IndentSize = indentSize;
      
      _buffer = new BufferWriter<char>(buffer, initialMinGrowCapacity);
      _currentLevel = 0;
      
      _indentCache = new BufferWriter<char>(indentBuffer);
      _indentCache.Fill(IndentCharacter);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void WriteLine()
   {
      _buffer.Add(NewLineCharacter);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void WriteLineIf(bool condition)
   {
      if (condition)
      {
         WriteLine();
      }
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void WriteLineIf(bool condition, scoped ReadOnlySpan<char> content, bool multiLine = false)
   {
      if (condition)
      {
         WriteLine(content, multiLine);
      }
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void WriteLine(scoped Span<char> content, bool multiLine = false)
   {
      Write(content, multiLine);
      WriteLine();
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void WriteLine(scoped ReadOnlySpan<char> content, bool multiLine = false)
   {
      Write(content, multiLine);
      WriteLine();
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void WriteText(string text)
   {
      WriteText(text.AsSpan());
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void WriteText(scoped ReadOnlySpan<char> text)
   {
      AddIndentOnDemand();
      _buffer.Write(text);
   }

   public void Write(scoped ReadOnlySpan<char> text, bool multiLine = false)
   {
      if (!multiLine)
      {
         WriteText(text);
      }
      else
      {
         while (text.Length > 0)
         {
            var newLinePos = text.IndexOf(NewLineCharacter);

            if (newLinePos >= 0)
            {
               var line = text[..newLinePos];
               
               WriteIf(!line.IsEmpty, line);
               WriteLine();

               text = text[(newLinePos + 1)..];
            }
            else
            {
               WriteText(text);
               break;
            }
         }
      }
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void WriteIf(bool condition, scoped ReadOnlySpan<char> content, bool multiLine = false)
   {
      if (condition)
      {
         Write(content, multiLine);
      }  
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void UpIndent()
   {
      _currentLevel++;
      _currentLevelBuffer = GetCurrentIndentBuffer();
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void DownIndent()
   {
      _currentLevel--;
      ArgumentOutOfRangeException.ThrowIfLessThan(_currentLevel, 0, nameof(_currentLevel));
      
      _currentLevelBuffer = GetCurrentIndentBuffer();
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Span<char> AcquireSpan(int length)
   {
      return _buffer.AcquireSpan(length, true);
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public Span<char> AcquireSpanIndented(int length)
   {
      AddIndentOnDemand();
      return _buffer.AcquireSpan(length, true);
   }
   
   private void AddIndentOnDemand()
   {
      if (_currentLevelBuffer.IsEmpty)
      {
         return;
      }
      
      if (_buffer.Position == 0 || _buffer.WrittenSpan[^1] == NewLineCharacter)
      {
         _buffer.Write(_currentLevelBuffer);
      }
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public ReadOnlySpan<char> GetCurrentIndentBuffer()
   {
      if (_currentLevel == 0)
      {
         return [];
      }

      var levelCount = IndentSize * _currentLevel;
      
      while (_indentCache.Position < levelCount)
      {
         _indentCache.Add(IndentCharacter);
      }

      return _indentCache.WrittenSpan[..levelCount];
   }
   
   public override string ToString()
   {
      return _buffer.WrittenSpan.Trim().ToString();
   }
   
   public void Dispose()
   {
      _buffer.Dispose();
      _indentCache.Dispose();
   }
}
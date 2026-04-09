using MonoTextBox.Deprecated.Positioning.SourceReading;
using MonoTextBox.Formatting;
using MonoTextBox.Storage;
using MonoTextBox.Utils;

namespace MonoTextBox.Compositing.Contract;

public readonly struct SourceSlice
{
    public int Offset { get; }
    public int Length { get; }
    
    private readonly FormatTree _formatTree;

    private readonly ReadOnlyMemory<char> _textCache;
    
    
    public SourceSlice(
        int offset, 
        int length, 
        SourceBuffer source)
    {
        Offset = offset;
        Length = length;
        
        _formatTree = source.FormatTree;
        
        var (firstPiece, relativeIndex) = source.PieceTree.Find(offset);
        _textCache = firstPiece.Length - relativeIndex > length 
            ? SliceContinuousPiece(firstPiece.Source, firstPiece.Start + relativeIndex, length, source)
            : StitchPieceSlice(source, offset, length, new char[length]);
    }

    private SourceSlice(
        int offset,
        int length,
        FormatTree formatTree,
        ReadOnlyMemory<char> textCache)
    {
        Offset = offset;
        Length = length;
        _formatTree = formatTree;
        _textCache = textCache;
    }


    // Interface
    public SourceEnumerator GetEnumerator() => new(this);

    
    public TextElement this[Index index] => throw new NotImplementedException();

    public ISource this[Range range] => throw new NotImplementedException();

    public ISource Slice(Slice slice) => Slice(slice.Start, slice.Length);

    public ISource Slice(Range range)
    {
        var (offset, length) = range.GetOffsetAndLength(Length);
        return Slice(offset, length);
    }

    public ISource Slice(int offset, int length)
    {
        var newOffset = Offset + offset;
        var newTextCache = _textCache.Slice(newOffset, length);
        return new SourceSlice(newOffset, length, _formatTree, newTextCache);
    }

    
    public ReadOnlySpan<char> GetTextSpan() => _textCache.Span;


    // Helpers
    private SourceEnumerator GetEnumeratorInternal()
    {
        var enumerator = new SourceSliceEnumerator(_formatTree, _textCache);
        return new SourceEnumerator(enumerator.MoveNext, enumerator.GetCurrent);
    }
    
    private TextElement GetValueAt(int index)
    {
        var c = _textCache.Span[index];
        var f = _formatTree.Find(c).Value.Format;
        return new TextElement(c, f);
    }
    

    private static ReadOnlyMemory<char> SliceContinuousPiece(
        Piece.SourceType sourceType,
        int sourceStart, 
        int length,
        SourceBuffer source)
    {
        return sourceType == Piece.SourceType.Original
            ? source.Original.AsMemory(sourceStart, length)
            : source.Added.GetMemory(sourceStart, length);
    }

    private static Memory<char> StitchPieceSlice(
        SourceBuffer source,
        int sourceStart, 
        int length, 
        Memory<char> memory)
    {
        var i = sourceStart;
        var l = length;
        while (true)
        {
            var (piece, relativeIndex) = source.PieceTree.Find(i);
            var pieceSpace = piece.Length - relativeIndex;
            
            var spanStart = piece.Start + relativeIndex;
            var spanLength = Math.Min(l, pieceSpace);
            CopyPieceSpan(source, piece.Source, spanStart, spanLength, memory);
            
            i += spanLength;
            l -= spanLength; 
            
            if (l <= 0) break;
        }

        return memory;
    }

    private static void CopyPieceSpan(
        SourceBuffer source,
        Piece.SourceType sourceType, 
        int sourceStart, 
        int length, 
        Memory<char> memory)
    {
        if (sourceType == Piece.SourceType.Add)
            source.Added.GetMemory(sourceStart, length).CopyTo(memory);
        else
            source.Original.AsMemory(sourceStart, length).CopyTo(memory);
    }
}


public ref struct SourceSliceEnumerator
{
    private int _index;
    
    private readonly FormatTree _formatTree;
    private readonly ReadOnlySpan<char> _textCache;

    private int _currentFormatRoom;
    private FormatBranch _currentFormat;
    
    
    public SourceSliceEnumerator(FormatTree formatTree, ReadOnlyMemory<char> textCache)
        : this(formatTree, textCache.Span)
    { }
    
    public SourceSliceEnumerator(FormatTree formatTree, ReadOnlySpan<char> textCache)
    {
        _index = 0;

        _currentFormatRoom = 0;
        _currentFormat = default;
        
        _textCache = textCache;
        _formatTree = formatTree;
    }


    public TextElement GetCurrent()
    {
        var c = _textCache[_index];

        RefreshCurrentFormatBranch();
        var f =  _currentFormat.Format;
        _currentFormatRoom--;
        
        return new TextElement(c, f);
    }
    
    public bool MoveNext()
    {
        _index++;
        return _index <= _textCache.Length;
    }


    private void RefreshCurrentFormatBranch()
    {
        if (_currentFormatRoom > 0)
            return;

        var (format, relativeIndex) = _formatTree.Find(_index);
        _currentFormat = format;
        _currentFormatRoom = format.Length - relativeIndex;
    }
}
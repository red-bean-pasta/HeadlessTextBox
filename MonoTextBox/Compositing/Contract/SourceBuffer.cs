using MonoTextBox.Deprecated.Positioning.SourceReading;
using MonoTextBox.Formatting;
using MonoTextBox.Storage;
using MonoTextBox.Utils;

namespace MonoTextBox.Compositing.Contract;

public class SourceBuffer
{
    public string Original { get; }
    public AddBuffer Added { get; }
    
    public PieceTree PieceTree { get; }
    public FormatTree FormatTree { get; }

    
    public SourceBuffer()
    {
        Original = string.Empty;
        Added = new AddBuffer();
        
        var piece = new Piece(0, 0, Piece.SourceType.Original);
        PieceTree = new PieceTree(piece, null, null);
        
        var format = new FormatBranch();
        FormatTree = new FormatTree(format, null, null);
    }


    public TextElement this[Index index] => GetValueAt(index);

    public ISource this[Range range] => Slice(range);

    private ISource Slice(Range range)
    {
        var (start, end) = range.GetOffsetAndLength(PieceTree.Length);
        return Slice(start, end);
    }
    
    public ISource Slice(Slice slice) => Slice(slice.Start, slice.Length);

    public ISource Slice(int start, int length)
    {
        return new SourceSlice(start, length, this);
    }

    public TextElement GetValueAt(Index index)
    {
        var (piece, pRelativeIndex) = PieceTree.Find(index);
        var content = GetContinuousPieceSpan(piece.Start + pRelativeIndex, 1, piece.Source);
        var c = content[0];
        
        var (format, fRelativeIndex) = FormatTree.Find(index);
        var f = format.Format;

        return new TextElement(c, f);
    }


    private ReadOnlySpan<char> GetContinuousPieceSpan(
        int start, 
        int length, 
        Piece.SourceType type)
    {
        return type == Piece.SourceType.Original
            ? Original.AsSpan(start, length)
            : Added.GetSpan(start, length);
    }
}
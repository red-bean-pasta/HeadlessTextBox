using MonoTextBox.Utils;

namespace MonoTextBox.Storage;

public readonly struct Piece: IBranch<Piece>
{
    public enum SourceType
    {
        Save,
        Add
    }
    
    
    public SourceType Source { get; }
    
    public int StartIndex { get; }
    
    public int Length { get; }


    public Piece(
        int startIndex, 
        int length, 
        SourceType source = SourceType.Add)
    {
        StartIndex = startIndex;
        Source = source;
        Length = length;
    }
    
    
    public (Piece, Piece) Split(int index)
    {
        var left = new Piece(StartIndex, index, Source);
        var right = new Piece(StartIndex + index, Length - index, Source);
        return (left, right);
    }
}
using MonoTextBox.Positioning;

namespace MonoTextBox.Storage;

public class TextBox
{
    private PieceTree _buffer;

    private Document Document;


    public TextBox(string? original = null)
    {
        original ??= string.Empty;
        
        var piece = new Piece(0, original.Length, Piece.SourceType.Original);
        _buffer = new PieceTree(piece, null, null);
        
        Document = document;
    }
}
namespace MonoTextBox.Positioning;

public class Document
{
    private readonly List<Line> _lines = new();
    
    public int LineCount => _lines.Count;
}
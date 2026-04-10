using Icu;
using MonoTextBox.Compositing.Contract;
using MonoTextBox.Compositing.Serialization;
using MonoTextBox.Formatting;
using MonoTextBox.Positioning;

namespace MonoTextBox.Storage;

public class TextBox
{
    private SourceBuffer _storage;
    
    private Document _document;


    public TextBox(float width, Locale? locale = null)
    {
        _storage = new SourceBuffer();
        _document = Document.Build(width, _storage, locale);
    }
    
    public TextBox(
        string text,
        FormatTree format,
        float width,
        Locale? locale = null)
    {
        _storage = new SourceBuffer(text, format);
        _document = Document.Build(width, _storage, locale);
    }

    public static TextBox Build<T>(
        string text,
        string format,
        float width,
        Locale? locale = null
    ) where T: IFormat
    {
        var formatTree = FormatSerializer<T>.Deserialize(format);
        return new TextBox(text, formatTree, width, locale);
    }
}
namespace MonoTextBox.Formatting.Font;

public interface IFont
{
    float Spacing { get; }
    GlyphMetrics GetGlyph(char c);
}
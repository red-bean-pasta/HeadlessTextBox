namespace MonoTextBox.Formatting.Font;

public interface IFont
{
    float Spacing { get; }
    GlyphMetrics GetGlyphMetrics(char c);
}
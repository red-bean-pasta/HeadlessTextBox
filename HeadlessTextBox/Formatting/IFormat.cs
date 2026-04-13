using HeadlessTextBox.Formatting.Font;

namespace HeadlessTextBox.Formatting;

public interface IFormat : IEquatable<IFormat>
{
    GlyphMetrics GetGlyphMetrics(char character);
}
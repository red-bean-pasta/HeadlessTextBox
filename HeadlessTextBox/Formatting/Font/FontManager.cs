namespace MonoTextBox.Formatting.Font;

public static class FontManager
{
    public static Dictionary<int, IFont> Fonts { get; } = new();
    
    public static IFont GetFont(int id) => Fonts[id];
}
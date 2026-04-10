using MonoTextBox.Formatting;

namespace MonoTextBox.Compositing.Contract;

public record struct TextElement(
    char Char, 
    IFormat Format
);
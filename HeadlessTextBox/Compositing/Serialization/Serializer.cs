using HeadlessTextBox.Compositing.Contracts;

namespace HeadlessTextBox.Compositing.Serialization;

public static class Serializer
{
    public static (string Text, string Format) Serialize(SourceBuffer source)
    {
        return (
            TextSerializer.Serialize(source.Text),
            FormatSerializer.SerializeV1(source.Format)
        );
    }
}
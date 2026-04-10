using MonoTextBox.Legacy.Editing.Inputs.Bases;

namespace MonoTextBox.Legacy.Editing.Inputs;

public class PasteInput: UndoRedoInput
{
    public PasteInput(
        int anchorIndex,
        IEnumerable<char> pasted, 
        IEnumerable<char>? replaced = null)
        : base(anchorIndex, pasted, replaced)
    { }
}
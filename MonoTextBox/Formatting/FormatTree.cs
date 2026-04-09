using MonoTextBox.Utils;
using MonoTextBox.Utils.AvlTree;

namespace MonoTextBox.Formatting;

public class FormatTree: Node<FormatBranch>
{
    public FormatTree(
        FormatBranch value, 
        Node<FormatBranch>? leftSubNode, 
        Node<FormatBranch>? rightSubNode) 
        : base(value, leftSubNode, rightSubNode)
    { }
}

public readonly struct FormatBranch : IBranch<FormatBranch>
{
    public Format Format { get; }
    
    public int Length { get; }
    
    
    public FormatBranch(Format format, int length)
    {
        Format = format;
        Length = length;
    }
    
    
    public (FormatBranch, FormatBranch) Split(int index)
    {
        var left = new FormatBranch(Format, index);
        var right = new FormatBranch(Format, Length - index);
        return (left, right);
    }
}
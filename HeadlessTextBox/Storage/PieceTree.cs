using MonoTextBox.Utils.AvlTree;

namespace MonoTextBox.Storage;

public class PieceTree: Node<Piece>
{
    public int Length => SubTreeLength;
    
    public PieceTree(
        Piece value, 
        Node<Piece>? leftSubNode, 
        Node<Piece>? rightSubNode
    ) : base(value, leftSubNode, rightSubNode)
    { }
}
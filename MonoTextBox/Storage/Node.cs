using MonoTextBox.Utils;

namespace MonoTextBox.Storage;

public class Node: Utils.Node<Piece>
{
    public Node(
        Piece value, 
        Node<Piece>? leftSubNode, 
        Node<Piece>? rightSubNode
    ) : base(value, leftSubNode, rightSubNode)
    { }
}
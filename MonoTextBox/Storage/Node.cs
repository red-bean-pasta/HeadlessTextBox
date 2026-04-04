using System.Diagnostics;
using MonoTextBox.Storage;
using MonoTextBox.Utils;

namespace MonoTextBox.Editing.Buffer;

public class Node: Utils.Node<Piece>
{
    public Node(
        Piece value, 
        Node<Piece>? leftSubNode, 
        Node<Piece>? rightSubNode
    ) : base(value, leftSubNode, rightSubNode)
    { }
}
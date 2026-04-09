using System.Diagnostics;

namespace MonoTextBox.Utils.AvlTree;

public class Node<T> where T : IBranch<T>
{
    protected T Value;

    protected Node<T>? LeftSubNode;
    protected Node<T>? RightSubNode;

    protected int SubTreeLength;
    protected int SubTreeHeight;


    protected int LeftLength => LeftSubNode?.SubTreeLength ?? 0;
    protected int BeforeRightLength => LeftLength + Value.Length;
    protected int LeftHeight => LeftSubNode?.SubTreeHeight ?? 0;
    protected int RightHeight => RightSubNode?.SubTreeHeight ?? 0;
    
    
    public Node(
        T value, 
        Node<T>? leftSubNode, 
        Node<T>? rightSubNode)
    {
        Value = value;
        LeftSubNode = leftSubNode;
        RightSubNode = rightSubNode;

        Recalculate();
    }
    
    
    public (T Value, int RelativeIndex) Find(Index absoluteIndex)
    {
        var normalized = absoluteIndex.GetOffset(SubTreeLength);
        return Find(normalized);
    }
    
    public (T Value, int RelativeIndex) Find(int absoluteIndex)
    {
        switch (CheckLeftRight(absoluteIndex))
        {
            case < 0:
                Debug.Assert(LeftSubNode is not null);
                return LeftSubNode.Find(absoluteIndex);
            case 0:
                return (Value, absoluteIndex);
            case > 0:
                Debug.Assert(RightSubNode is not null);
                return RightSubNode.Find(absoluteIndex - BeforeRightLength);
        }
    }

    
    public Node<T> AppendAndBalance(T value)
    {
        var index = BeforeRightLength + RightSubNode?.SubTreeLength ?? 0;
        return InsertAndBalance(index, value);
    }
    
    /// <returns>If depth added</returns>
    public Node<T> InsertAndBalance(int index, T value)
    {
        Debug.Assert(value.Length > 0);

        Insert(value, index);
        return Balance();
    }
    
    private void Insert(T value, int insertIndex)
    {   
        if (insertIndex <= LeftLength)
        {
            Debug.Assert(LeftSubNode is not null);
            LeftSubNode = InsertToSubNode(LeftSubNode, value, insertIndex);
        }
        else if (insertIndex >= BeforeRightLength)
        {
            var rightIndex = insertIndex - BeforeRightLength;
            RightSubNode = InsertToSubNode(RightSubNode, value, rightIndex);
        }
        else
        {
            InsertToCurrentT(value, insertIndex - LeftLength);
        }
        
        Recalculate();
    }

    private void InsertToCurrentT(T value, int splitIndex)
    {
        var (leftSplit, rightSplit) = SplitT(Value, splitIndex);
        LeftSubNode = InsertToSubNode(LeftSubNode, leftSplit, LeftLength);
        RightSubNode = InsertToSubNode(RightSubNode, rightSplit, 0);
        Value = value;
    }

    private static Node<T> InsertToSubNode(Node<T>? subNode, T value, int insertIndex)
    {
        if (subNode is null)
            return new Node<T>(value, null, null);
        else
            return subNode.InsertAndBalance(insertIndex, value);
    }


    public Node<T>? RemoveAndBalance(int index)
    {
        if (index < LeftLength)
        {
            Debug.Assert(LeftSubNode is not null);
            LeftSubNode = LeftSubNode.RemoveAndBalance(index);
        }
        else if (index >= BeforeRightLength)
        {
            RightSubNode = RightSubNode?.RemoveAndBalance(index - BeforeRightLength);
        }
        else if (LeftSubNode is not null)
        {
            var (value, left) = LeftSubNode.PopRightLeaf();
            LeftSubNode = left;
            Value = value;
        }
        else if (RightSubNode is not null)
        {
            var (value, right) = RightSubNode.PopLeftLeaf();
            RightSubNode = right;
            Value = value;
        }
        else
        {
            return null;
        }
        
        Recalculate();
        return Balance();
    }

    private (T Value, Node<T>? Node) PopRightLeaf()
    {
        if (RightSubNode is null)
            return (Value, LeftSubNode);
        
        var (leaf, right) = RightSubNode.PopRightLeaf();
        RightSubNode = right;
        Recalculate();
        return (leaf, Balance());
    }
    
    private (T Value, Node<T>? Node) PopLeftLeaf()
    {
        if (LeftSubNode is null)
            return (Value, RightSubNode);
        
        var (leaf, left) = LeftSubNode.PopLeftLeaf();
        LeftSubNode = left;
        Recalculate();
        return (leaf, Balance());
    }
    

    // Rotate the tree if skewed.
    // Example:
    // a -> b -> c
    // =>
    // a <- b -> c
    private Node<T> Balance()
    {
        var difference = LeftHeight - RightHeight;
        switch (difference)
        {
            case > 1:
                StraightenLeft();
                return RotateRight(this);
            case < -1:
                StraightenRight();
                return RotateLeft(this);
            default:
                return this;
        }
    }

    private static Node<T> RotateRight(Node<T> node)
    {
        Debug.Assert(node.LeftSubNode is not null);
        
        var newRoot = node.LeftSubNode;
        
        var newLeft = newRoot.LeftSubNode;
        
        var newRight = node;
        newRight.LeftSubNode = newRoot.RightSubNode;
        newRight.Recalculate();
        
        newRoot.LeftSubNode = newLeft;
        newRoot.RightSubNode = newRight;
        newRoot.Recalculate();
        
        return newRoot;
    }

    private static Node<T> RotateLeft(Node<T> node)
    {
        Debug.Assert(node.RightSubNode is not null);

        var newRoot = node.RightSubNode;
        
        var newLeft = node;
        newLeft.RightSubNode = newRoot.LeftSubNode;
        newLeft.Recalculate();
        
        var newRight = newRoot.RightSubNode;
        
        newRoot.LeftSubNode = newLeft;
        newRoot.RightSubNode = newRight;
        newRoot.Recalculate();
        
        return newRoot;
    }

    private void StraightenLeft()
    {
        var left = LeftSubNode;
        Debug.Assert(left is not null);
        
        LeftSubNode = left.LeftHeight < left.RightHeight
            ? RotateLeft(left)
            : left;
    }
    
    private void StraightenRight()
    {
        var right = RightSubNode;
        Debug.Assert(right is not null);
        
        RightSubNode = right.RightHeight < right.LeftHeight
            ? RotateRight(right)
            : right;
    }


    private void Recalculate()
    {
        SubTreeHeight = Math.Max(
                LeftSubNode?.SubTreeHeight ?? 0,
                RightSubNode?.SubTreeHeight ?? 0
            ) + 1;

        SubTreeLength = 
            Value.Length
            + (LeftSubNode?.SubTreeLength ?? 0)
            + (RightSubNode?.SubTreeLength ?? 0);
    }

    
    // left subtree: [0, leftLength)
    // current value: [leftLength, leftLength + T.Length)
    // right subtree: [starts at leftLength + T.Length, ...)
    private int CheckLeftRight(int index)
    {
        if (index < LeftLength)
            return -1;
        
        if (index < BeforeRightLength) 
            return 0;
        
        return 1;
    }

    private static (T Left, T Right) SplitT(T value, int index)
    {
        Debug.Assert(0 < index && index < value.Length);
        return value.Split(index);
    }
}
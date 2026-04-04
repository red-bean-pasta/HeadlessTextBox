using System.Diagnostics;

namespace MonoTextBox.Utils;

public class Node<T> where T : IBranch<T>
{
    private T _value;

    private Node<T>? _leftSubNode;
    private Node<T>? _rightSubNode;

    private int _subTreeLength;
    private int _subTreeHeight;


    private int LeftLength => _leftSubNode?._subTreeLength ?? 0;
    private int BeforeRightLength => LeftLength + _value.Length;
    private int LeftHeight => _leftSubNode?._subTreeHeight ?? 0;
    private int RightHeight => _rightSubNode?._subTreeHeight ?? 0;
    
    
    public Node(
        T value, 
        Node<T>? leftSubNode, 
        Node<T>? rightSubNode)
    {
        _value = value;
        _leftSubNode = leftSubNode;
        _rightSubNode = rightSubNode;

        Recalculate();
    }
    
    
    public T FindT(int index)
    {
        switch (CheckLeftRight(index))
        {
            case < 0:
                Debug.Assert(_leftSubNode is not null);
                return _leftSubNode.FindT(index);
            case 0:
                return _value;
            case > 0:
                Debug.Assert(_rightSubNode is not null);
                return _rightSubNode.FindT(index - BeforeRightLength);
        }
    }
    

    /// <returns>If depth added</returns>
    public Node<T> InsertAndBalance(T value, int index)
    {
        Insert(value, index);
        return Balance();
    }
    
    private void Insert(T value, int insertIndex)
    {
        if (insertIndex <= LeftLength)
        {
            Debug.Assert(_leftSubNode is not null);
            _leftSubNode = InsertToSubNode(_leftSubNode, value, insertIndex);
        }
        else if (insertIndex >= BeforeRightLength)
        {
            var rightIndex = insertIndex - BeforeRightLength;
            _rightSubNode = InsertToSubNode(_rightSubNode, value, rightIndex);
        }
        else
        {
            InsertToCurrentT(value, insertIndex - LeftLength);
        }
        
        Recalculate();
    }

    private void InsertToCurrentT(T value, int splitIndex)
    {
        var (leftSplit, rightSplit) = SplitT(_value, splitIndex);
        _leftSubNode = InsertToSubNode(_leftSubNode, leftSplit, LeftLength);
        _rightSubNode = InsertToSubNode(_rightSubNode, rightSplit, 0);
        _value = value;
    }

    private static Node<T> InsertToSubNode(Node<T>? subNode, T value, int insertIndex)
    {
        if (subNode is null)
            return new Node<T>(value, null, null);
        else
            return subNode.InsertAndBalance(value, insertIndex);
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
        Debug.Assert(node._leftSubNode is not null);
        
        var newRoot = node._leftSubNode;
        
        var newLeft = newRoot._leftSubNode;
        
        var newRight = node;
        newRight._leftSubNode = newRoot._rightSubNode;
        newRight.Recalculate();
        
        newRoot._leftSubNode = newLeft;
        newRoot._rightSubNode = newRight;
        newRoot.Recalculate();
        
        return newRoot;
    }

    private static Node<T> RotateLeft(Node<T> node)
    {
        Debug.Assert(node._rightSubNode is not null);

        var newRoot = node._rightSubNode;
        
        var newLeft = node;
        newLeft._rightSubNode = newRoot._leftSubNode;
        newLeft.Recalculate();
        
        var newRight = newRoot._rightSubNode;
        
        newRoot._leftSubNode = newLeft;
        newRoot._rightSubNode = newRight;
        newRoot.Recalculate();
        
        return newRoot;
    }

    private void StraightenLeft()
    {
        var left = _leftSubNode;
        Debug.Assert(left is not null);
        
        _leftSubNode = left.LeftHeight < left.RightHeight
            ? RotateLeft(left)
            : left;
    }
    
    private void StraightenRight()
    {
        var right = _rightSubNode;
        Debug.Assert(right is not null);
        
        _rightSubNode = right.RightHeight < right.LeftHeight
            ? RotateRight(right)
            : right;
    }


    private void Recalculate()
    {
        _subTreeHeight = Math.Max(
                _leftSubNode?._subTreeHeight ?? 0,
                _rightSubNode?._subTreeHeight ?? 0
            ) + 1;

        _subTreeLength = 
            _value.Length
            + (_leftSubNode?._subTreeLength ?? 0)
            + (_rightSubNode?._subTreeLength ?? 0);
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
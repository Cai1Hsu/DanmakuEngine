namespace DanmakuEngine.Allocations;

public class RBTreeNode<T>
{
    public T Value { get; set; }

    public RBTreeColor Color { get; set; }

    public bool IsBlack => Color is RBTreeColor.Black;

    public bool IsRed => Color is RBTreeColor.Red;

    public RBTreeNode<T>? Left { get; set; } = null;

    public RBTreeNode<T>? Right { get; set; } = null;

    public RBTreeNode<T>? Parent { get; set; } = null;

    public RBTreeNode<T>? Grandparent => Parent?.Parent;

    public bool IsLeaf => Left is null && Right is null;

    public bool IsRoot => Parent is null;

    public bool IsLeftChild => Parent?.Left == this;

    public bool IsRightChild => Parent?.Right == this;

    public RBTreeNode<T>? Uncle
    {
        get
        {
            var gp = Grandparent;

            if (gp is null)
                return null;

            return (gp.Right == Parent) switch
            {
                true => gp.Left,
                false => gp.Right
            };
        }
    }

    public RBTreeNode<T>? Sibling
    {
        get
        {
            if (IsRoot)
                return null;

            return IsRightChild switch
            {
                true => Parent!.Left,
                false => Parent!.Right
            };
        }
    }

    public RBTreeNode(T value)
    {
        Value = value;
        Color = RBTreeColor.Black;
    }
}

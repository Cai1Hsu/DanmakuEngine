using System.Collections;
using System.Diagnostics;

namespace DanmakuEngine.Allocations;

public class RBTree<T> : IEnumerable<RBTreeNode<T>>
{
    private RBTreeNode<T>? _root = null;
    private Func<T, T, int> _comparer;

    public RBTree()
    {
        if (typeof(T).IsAssignableTo(typeof(IComparable<T>)))
        {
            throw new ArgumentException("T must implement IComparable<T>");
        }

        _comparer = Comparer<T>.Default.Compare;
    }

    public RBTree(Func<T, T, int> comparer)
    {
        _comparer = comparer;
    }

    public RBTree(RBTreeNode<T> root)
    {
        if (typeof(T).IsAssignableTo(typeof(IComparable<T>)))
        {
            throw new ArgumentException("T must implement IComparable<T>");
        }

        _comparer = Comparer<T>.Default.Compare;
    }

    public RBTree(RBTreeNode<T> root, Func<T, T, int> comparer)
    {
        _root = root;
        _comparer = comparer;
    }

    private int? _count = null;

    public int Count
    {
        get
        {
            if (!_count.HasValue)
            {
                _count = RBTree<T>.countNodes(_root);
            }

            return _count.Value;
        }
        set => _count = value;
    }

    private static int countNodes(RBTreeNode<T>? node)
    {
        if (node is null)
            return 0;

        return 1 + RBTree<T>.countNodes(node.Left) + RBTree<T>.countNodes(node.Right);
    }

    public bool IsReadOnly => false;

    public RBTreeNode<T>? Root => _root;

    public bool IsEmpty => _count == 0;

    public RBTreeNode<T>? SmallestNode
    {
        get
        {
            if (IsEmpty)
                return null;

            return RBTree<T>.smallestNode(_root!);
        }
    }

    private static RBTreeNode<T> smallestNode(RBTreeNode<T> node)
    {
        if (node.Left is null)
            return node;

        return RBTree<T>.smallestNode(node.Left);
    }

    public void Add(T item)
    {
        if (_root is null)
        {
            createRoot(item);

            return;
        }

        insertValue(_root, item);
    }

    private void insertValue(RBTreeNode<T> p, T data)
    {
        if (_comparer(p.Value, data) >= 0)
        {
            if (p.Left is not null)
            {
                insertValue(p.Left, data);
            }
            else
            {
                var node = new RBTreeNode<T>(data)
                {
                    Parent = p
                };
                p.Left = node;
                insertCase(node);
            }
        }
        else
        {
            if (p.Right is not null)
            {
                insertValue(p.Right, data);
            }
            else
            {
                var node = new RBTreeNode<T>(data)
                {
                    Parent = p
                };
                p.Right = node;
                insertCase(node);
            }
        }
    }

    private void insertCase(RBTreeNode<T> p)
    {
        if (p.Parent is null)
        {
            _root = p;
            p.Color = RBTreeColor.Black;
        }
        else if (p.Parent.IsRed)
        {
            var gp = p.Grandparent;
            Debug.Assert(gp is not null);

            if (p.Uncle?.Color is RBTreeColor.Red)
            {
                p.Parent.Color = p.Uncle.Color = RBTreeColor.Black;


                gp.Color = RBTreeColor.Red;
                insertCase(gp);
            }
            else
            {
                if (p.IsRightChild && gp.Left == p.Parent)
                {
                    rotateLeft(p);
                    p.Color = RBTreeColor.Black;
                    p.Parent.Color = RBTreeColor.Red;
                    rotateRight(p);
                }
                else if (p.IsLeftChild && gp.Right == p.Parent)
                {
                    rotateRight(p);
                    p.Color = RBTreeColor.Black;
                    p.Parent.Color = RBTreeColor.Red;
                    rotateLeft(p);
                }
                else if (p.IsLeftChild && gp.Left == p.Parent)
                {
                    p.Parent.Color = RBTreeColor.Black;
                    gp.Color = RBTreeColor.Red;
                    rotateRight(p.Parent);
                }
                else if (p.IsRightChild && gp.Right == p.Parent)
                {
                    p.Parent.Color = RBTreeColor.Black;
                    gp.Color = RBTreeColor.Red;
                    rotateLeft(p.Parent);
                }
            }
        }
    }

    private void rotateRight(RBTreeNode<T> item)
    {
        var gp = item.Grandparent;
        var p = item.Parent;
        var y = item.Right;

        Debug.Assert(p is not null);
        p.Left = y;

        if (y is not null)
            y.Parent = p;

        item.Right = p;
        p.Parent = item;

        if (p.Equals(_root))
            _root = item;

        item.Parent = gp;

        if (gp is not null)
        {
            Debug.Assert(gp.Left is not null);

            if (p.Equals(gp.Left))
                gp.Left = item;
            else
                gp.Right = item;
        }
    }

    private void rotateLeft(RBTreeNode<T> item)
    {
        if (item.Parent is null)
        {
            _root = item;
            return;
        }

        var gp = item.Grandparent;
        var p = item.Parent;
        var y = item.Left;

        p.Right = y;

        if (y is not null)
            y.Parent = p;

        item.Left = p;
        p.Parent = item;

        if (p.Equals(_root))
            _root = item;

        item.Parent = gp;

        if (gp is not null)
        {
            if (p.Equals(gp.Left))
                gp.Left = item;
            else
                gp.Right = item;
        }
    }

    private void createRoot(T item)
    {
        _root = new RBTreeNode<T>(item);
        _count = 0;
    }

    /// <summary>
    /// Clear all nodes from the tree but will NOT dispose of them.
    /// Please use <see cref="DisposeClear"/>
    /// </summary>
    public void Clear()
    {
        _root = null;
        _count = 0;
    }

    public void DisposeClear<TDisposable>()
        where TDisposable : T, IDisposable
    {
        disposeTree(_root);
        Clear();
    }

    public void disposeTree(RBTreeNode<T>? root)
    {
        if (root is null)
            return;

        if (!typeof(T).IsAssignableTo(typeof(IDisposable)))
            return;

        disposeTreeInternal(root);
    }

    private void disposeTreeInternal(RBTreeNode<T> root)
    {
        if (root.Left is not null)
            disposeTreeInternal(root.Left);

        if (root.Right is not null)
            disposeTreeInternal(root.Right);

        ((IDisposable?)root.Value)?.Dispose();
    }

    public bool Contains(T item)
    {
        var current = _root;

        while (current is not null)
        {
            var result = _comparer(current.Value, item);

            switch (result)
            {
                case 0:
                    return true;

                case < 0:
                    current = current.Left;
                    break;

                case > 0:
                    current = current.Right;
                    break;
            }
        }

        return false;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);

        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(arrayIndex, array.Length);

        foreach (var item in this)
        {
            if (arrayIndex == array.Length)
                break;

            array[arrayIndex++] = item.Value;
        }
    }

    public bool Remove(RBTreeNode<T> item, bool bypassRootCheck = false)
    {
        if (IsEmpty)
            return false;

        if (!bypassRootCheck && !underSameTree(item))
            return false;

        removeNode(item);

        return true;
    }

    private void removeNode(RBTreeNode<T> node)
    {
        var child = node.Left is null ? node.Right : node.Left;

        Debug.Assert(child is not null);

        if (node.IsRoot)
        {
            if (node.IsLeaf)
            {
                _root = null;

                return;
            }

            child.Parent = null;
            _root = child;
            _root.Color = RBTreeColor.Black;

            return;
        }

        if (node.IsLeftChild)
        {
            node.Parent!.Left = child;
        }
        else
        {
            node.Parent!.Right = child;
        }

        child.Parent = node.Parent;

        if (node.IsBlack)
        {
            if (child.IsRed)
            {
                child.Color = RBTreeColor.Black;
            }
            else
            {
                removeCase(child);
            }
        }

        // Dispose node
    }

    private void removeCase(RBTreeNode<T> p)
    {
        if (p.IsRoot)
        {
            p.Color = RBTreeColor.Black;

            return;
        }

        var parent = p.Parent!;
        var sibling = p.Sibling!;

        if (sibling.IsRed)
        {
            parent.Color = RBTreeColor.Red;
            sibling.Color = RBTreeColor.Black;

            if (p.IsLeftChild)
                rotateLeft(parent);
            else
                rotateRight(parent);
        }

        switch ((parent.Color, sibling.Color))
        {
            case (RBTreeColor.Black, RBTreeColor.Black):
            {
                if ((sibling.Left?.Color, sibling.Right?.Color)
                    is (RBTreeColor.Black, RBTreeColor.Black))
                {
                    sibling.Color = RBTreeColor.Red;
                    removeCase(parent);
                }
                else
                    goto default;
            }
            break;

            case (RBTreeColor.Red, RBTreeColor.Black):
            {
                if ((sibling.Left?.Color, sibling.Right?.Color)
                    is (RBTreeColor.Black, RBTreeColor.Black))
                {
                    sibling.Color = RBTreeColor.Red;
                    parent.Color = RBTreeColor.Black;
                }
                else
                    goto default;
            }
            break;

            default:
            {
                if (sibling.IsBlack)
                {
                    if (p.IsLeftChild && (sibling.Left?.Color, sibling.Right?.Color)
                        is (RBTreeColor.Red, RBTreeColor.Black))
                    {
                        sibling.Color = RBTreeColor.Red;
                        sibling.Left.Color = RBTreeColor.Black;
                    }
                    else if (p.IsRightChild && (sibling.Left?.Color, sibling.Right?.Color)
                        is (RBTreeColor.Black, RBTreeColor.Red))
                    {
                        sibling.Color = RBTreeColor.Red;
                        sibling.Right.Color = RBTreeColor.Black;
                        rotateLeft(sibling.Right);
                    }
                }

                Debug.Assert(p.Sibling is not null);
                Debug.Assert(p.Parent is not null);

                p.Sibling.Color = p.Parent.Color;
                p.Parent.Color = RBTreeColor.Black;
                if (p == parent.Left)
                {
                    Debug.Assert(p.Sibling.Right is not null);

                    p.Sibling.Right.Color = RBTreeColor.Black;

                    rotateLeft(p.Sibling);
                }
                else
                {
                    Debug.Assert(p.Sibling.Left is not null);

                    p.Sibling.Left.Color = RBTreeColor.Black;

                    rotateRight(p.Sibling);
                }
            }
            break;
        }
    }

    private bool underSameTree(RBTreeNode<T>? item)
    {
        if (item is null)
            return false;

        if (item == _root)
            return true;

        return underSameTree(item.Parent);
    }

    public bool Remove(T item)
    {
        if (IsEmpty)
            return false;

        return removeValue(_root!, item);
    }

    private bool removeValue(RBTreeNode<T> root, T item)
    {
        var res = _comparer(root.Value, item);

        switch (res)
        {
            case 0:
                if (root.Right is null)
                {
                    removeNode(root);
                    return true;
                }

                var smallest = SmallestNode!;
                (smallest.Value, root.Value) = (root.Value, smallest.Value);

                removeNode(smallest);
                return true;

            case < 0:
                if (root.Right is null)
                    return false;

                return removeValue(root.Right, item);

            case > 0:
                if (root.Left is null)
                    return false;

                return removeValue(root.Left, item);
        }
    }

    public IEnumerator<RBTreeNode<T>> GetEnumerator()
        => traverse(_root!).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    private static IEnumerable<RBTreeNode<T>> traverse(RBTreeNode<T> root)
    {
        if (root is not null)
        {
            if (root.Left is not null)
            {
                foreach (var item in traverse(root.Left))
                {
                    yield return item;
                }
            }

            yield return root;

            if (root.Right is not null)
            {
                foreach (var item in traverse(root.Right))
                {
                    yield return item;
                }
            }
        }
    }
}

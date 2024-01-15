using DanmakuEngine.Games;
using Silk.NET.Maths;

namespace DanmakuEngine.Graphics;

public abstract class CompositeDrawable : Drawable
{
    public Vector2D<float> Size = new(0, 0);

    protected virtual IEnumerable<GameObject> Children => children;

    protected virtual LinkedList<GameObject> children { get; set; } = null!;

    public CompositeDrawable(CompositeDrawable parent) : base(parent)
    {
        this.children = new LinkedList<GameObject>();
    }

    public void Add(GameObject child)
        => children.AddLast(child);

    public void Remove(GameObject child)
        => children.Remove(child);

    public void Add(params GameObject[] children)
    {
        foreach (var child in children)
            Add(child);
    }

    public void Remove(params GameObject[] children)
    {
        foreach (var child in children)
            Remove(child);
    }

    /// <summary>
    /// Updates the drawable and all of its children
    /// </summary>
    /// <returns>false if continues to update, and true for stops</returns>
    public override bool UpdateSubTree(bool fixedUpdate = false)
    {
        if (base.UpdateSubTree(fixedUpdate))
            return true;

        if (UpdateChildren(fixedUpdate))
            return true;

        return false;
    }

    protected virtual bool UpdateChildren(bool fixedUpdate = false)
    {
        bool shouldStop = false;

        if (Children is not null)
        {
            foreach (var child in Children)
                shouldStop |= child.UpdateSubTree(fixedUpdate);
        }

        return shouldStop;
    }
}

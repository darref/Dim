using Godot;

namespace Dim.Dimensions;

public abstract partial class DimensionRule : Resource
{
    protected  SubViewport _subViewportRoot;

    public void Init(SubViewport subViewportRoot)
    {
        _subViewportRoot = subViewportRoot;
    }


    
}
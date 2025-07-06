using Godot;

namespace Again.Dimensions;

public partial class DimensionRule : Node
{
    protected  SubViewport _subViewportRoot;
    public DimensionRule(SubViewport subViewportRoot)
    {
        GD.Print("Initilizing new DimensionRule");
        _subViewportRoot = subViewportRoot;
    }
    
}
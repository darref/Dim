using Godot;

namespace Dim.DimensionsSynchronizer;

public partial class DimensionsSynchronizer : Node
{
    public static DimensionsSynchronizer Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }
}
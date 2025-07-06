using Godot;

namespace Dim.Dimensions;

public abstract partial  class DuringDimensionRule : DimensionRule
{
    public abstract void StartApplying();
    public abstract void PauseApplying();
}
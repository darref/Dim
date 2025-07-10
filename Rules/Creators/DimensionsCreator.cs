using Dim.Dimensions;
using Godot;
using Godot.Collections;

namespace Dim.Rules.Creators;

[GlobalClass]
public partial class DimensionsCreator : Resource
{
    [Export] public Array<int> DimensionOrdersToCreate  { get; set; }
    
}
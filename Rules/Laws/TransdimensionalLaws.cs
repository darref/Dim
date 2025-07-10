using Godot;
using Godot.Collections;

namespace Dim.Rules.Laws;

[GlobalClass]
public partial class TransdimensionalLaws : Resource
{
    [Export] public Array<DimensionRule> Rules { get; set; }
}
using Godot;
using Godot.Collections;

namespace Dim.Rules.Creators;

[GlobalClass]
public partial class RulesCreator : Resource
{
    [Export] public Array<DimensionRule> RulesToCreate { get; set; }
    
}
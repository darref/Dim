using Godot;
using Godot.Collections;

//Les lois sont des regles qui seront forcément appliquées a chaque ordre de dimension
namespace Dim.Rules.Laws;

[GlobalClass]
public partial class OrderLaws : Resource
{
    [Export] public int DimensionOrder { get; set; }
    [Export] public Array<DimensionRule> Rules { get; set; }
}
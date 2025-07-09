using Dim.Dimensions;
using Godot;
using Godot.Collections;

//Les lois sont des regles qui seront forcément appliquées a chaque ordre de dimension
namespace Dim.Rules.Laws;

[GlobalClass]
public partial class LawEntriesByOrder : Resource
{
    [Export] public int Order { get; set; }
    [Export] public Array<LawEntry> LawEntries { get; set; }
}
using Godot;
using Godot.Collections;

//Les lois sont des regles qui seront forcément appliquées a chaque ordre de dimension
namespace Dim.Dimensions;

[GlobalClass]
public partial class LawEntryByOrder : Resource
{
    [Export] public int Order { get; set; }
    [Export] public Array<LawsEntry> LawEntries { get; set; }
}
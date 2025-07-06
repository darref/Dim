using Godot;

//Les lois sont des regles qui seront forcément appliquées a chaque ordre de dimension
namespace Dim.Dimensions;

[GlobalClass]
public partial class LawsEntry : Resource
{
    [Export] public DimensionRule Rule { get; set; }
    [Export] public bool Enabled { get; set; } = false;
}
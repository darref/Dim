using Godot;

//Les lois sont des regles qui seront forcément appliquées a chaque ordre de dimension
namespace Dim.Rules.Laws;

[GlobalClass]
public partial class LawEntry : Resource
{
    [Export] public DimensionRule Rule { get; set; }
    [Export] public bool Enabled { get; set; } = false;
    [Export] public bool _applyOnStart { get; set; } = true;
    [Export] public bool _applyOnEnd { get; set; } = false;
    [Export] public bool _applyPermanently { get; set; } = false;
}
using Godot;

namespace Again.Scripts;

public partial class Main : Node
{
	public override void _Ready()
	{
		var dm = Utils.NodeManagement.FindUniqueNamedNodeEverywhere(GetTree().Root, "DimensionsManager") as UI.DimensionsManager;
		GD.Print(Utils.Signals.toggleFullscreenSignal.Name);		
	}
}

using Dim.Utils;
using Godot;

namespace Dim.UI;

public partial class LabelFps : Label
{
	public override void _Process(double delta)
	{
		var fpsValue = Engine.GetFramesPerSecond();
		var player = (NodeManagement.FindUniqueNamedNodeEverywhere(GetTree().Root, "Player") as CharacterBody2D);
		
		if(player == null)return;
		
		var positionValue = player.GlobalPosition;
		var mouseGlobalPos = (Vector2I)GetGlobalMousePosition();
		Text = $"FPS : {fpsValue}	\t"  + $"		WorldPosition : {positionValue}	\t"  + $"		MousePosition : {mouseGlobalPos}" ;
	}

}

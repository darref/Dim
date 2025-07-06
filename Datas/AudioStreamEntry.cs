using Godot;

namespace Again.Datas;

[GlobalClass]
public partial class AudioStreamEntry : Resource
{
	[Export]
	public string Name { get; set; }

	[Export]
	public AudioStream Stream { get; set; }
}

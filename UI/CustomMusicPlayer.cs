using Godot;
using Godot.Collections;
using Again.Datas;

namespace Again.Scripts;

public partial class CustomMusicPlayer : Node
{
	[Export]
	public Array<AudioStreamEntry> Entries { get; set; } = new();

	private AudioStreamPlayer2D _player = new();

	public override void _Ready()
	{

		_player.Bus = new StringName("Music"); 
		AddChild(_player);
		
	}

	/// <summary>
	/// Joue un flux audio par son nom.
	/// </summary>
	public bool PlayStreamByName(string name, bool loop = true)
	{
		foreach (var entry in Entries)
		{
			if (entry != null && entry.Name == name && entry.Stream != null)
			{
				if (_player.Stream == entry.Stream && _player.Playing)
					return false;

				// Appliquer le mode loop en fonction du type de stream
				if (entry.Stream is AudioStreamOggVorbis ogg)
					ogg.Loop = loop;
				else if (entry.Stream is AudioStreamMP3 mp3)
					mp3.Loop = loop;
				else if (entry.Stream is AudioStreamWav wav)
					wav.LoopMode = loop ? AudioStreamWav.LoopModeEnum.Forward : AudioStreamWav.LoopModeEnum.Disabled;

				_player.SetStream(entry.Stream);
				_player.Play();
				GD.Print("Playing: " +  entry.Name);
				return true;
			}
		}
		return false;
	}


	public void Stop() => _player?.Stop();

	public Array<string> GetStreamNames()
	{
		var names = new Array<string>();
		foreach (var entry in Entries)
		{
			if (entry != null && !string.IsNullOrEmpty(entry.Name))
				names.Add(entry.Name);
		}
		return names;
	}
}

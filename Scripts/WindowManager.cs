using Dim.Utils;
using Godot;

namespace Dim.Scripts;

public partial class WindowManager : Node
{
	public override void _Ready()
	{
		// Passe en mode plein écran au démarrage
		DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		AjusterTailleFenetre();
	}

	public override void _Input(InputEvent @event)
	{
		// Vérifie si Alt+Entrée est pressé
		if (@event.IsActionPressed("toggle_fullscreen") && Input.IsKeyPressed(Key.Enter))
		{
			ToggleFullscreen();
			
			
			// Si on passe en mode fenêtré, ajuster la taille
			if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed)
			{
				AjusterTailleFenetre();
			}
			
		}
	}

	private void AjusterTailleFenetre()
	{
		// Obtient la taille de l'écran
		Vector2I tailleEcran = DisplayServer.ScreenGetSize();
		
		// Calcule 90% de la taille de l'écran
		Vector2I nouvelleTaille = new(
			(int)(tailleEcran.X * 0.9f),
			(int)(tailleEcran.Y * 0.9f)
		);
		
		// Centre la fenêtre
		Vector2I position = new(
			(tailleEcran.X - nouvelleTaille.X) / 2,
			(tailleEcran.Y - nouvelleTaille.Y) / 2
		);
		
		// Applique la taille et la position
		DisplayServer.WindowSetSize(nouvelleTaille);
		DisplayServer.WindowSetPosition(position);
	}

	

	public void ToggleFullscreen()
	{
		if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen)
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		}
		else
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		}
		EventBus.EmitToggleFullscreen();



	}

}

using System;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Dim.Dimensions;

public partial class Dimension : TextureRect
{
	public int _dimOrder;
	public SubViewport _subViewportRoot;
	public Array<StartingDimensionRule> _startingRules = new Array<StartingDimensionRule>();
	public Array<DuringDimensionRule> _duringRules = new Array<DuringDimensionRule>();

	public void Init(int dim)
	{
		_dimOrder = dim;
		Name = $"{_dimOrder}D";
		
		// Configuration du layout
		LayoutMode = 1;
		AnchorsPreset = (int)LayoutPreset.FullRect;
		
		_subViewportRoot = GetNode("SubViewport") as SubViewport;
		if (_subViewportRoot != null)
		{
			// Supprimer toutes les caméras existantes
			var existingCameras = _subViewportRoot.GetChildren()
				.Where(node => node is Camera2D || node is Camera3D);
			foreach (var camera in existingCameras)
			{
				camera.QueueFree();
			}

			// Créer la caméra appropriée selon l'ordre de la dimension
			if (_dimOrder < 3)
			{
				var camera2D = new Camera2D
				{
					Name = "MainCamera",
					Enabled = true,
					Position = Vector2.Zero,
					AnchorMode = Camera2D.AnchorModeEnum.FixedTopLeft
				};
				_subViewportRoot.AddChild(camera2D);
			}
			else
			{
				var camera3D = new Camera3D
				{
					Name = "MainCamera",
					Current = true,
					Position = new Vector3(0, 0, 10) // Position par défaut pour voir la scène
				};
				_subViewportRoot.AddChild(camera3D);
			}

			Vector2I windowSize = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen 
				? DisplayServer.ScreenGetSize() 
				: DisplayServer.WindowGetSize();
			
			CustomMinimumSize = windowSize;
			_subViewportRoot.Size = windowSize;
			Texture = _subViewportRoot.GetTexture();
		}
		else
		{
			GD.PushWarning("SubViewport manquant dans TextureRectViewportDisplayer");
		}
	}

	public void defineRules()
	{
		
	
	}



	public void applyRules()
	{
		foreach (var startrule in _startingRules)
		{
			startrule.Apply();
		}
		foreach (var duringrule in _duringRules)
		{
			duringrule.StartApplying();
		}
	}

}

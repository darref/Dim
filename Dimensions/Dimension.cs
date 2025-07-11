using System;
using System.Linq;
using Dim.Rules;
using Godot;
using Godot.Collections;

namespace Dim.Dimensions;

public partial class Dimension : TextureRect
{
	public int _dimOrder;
	public SubViewport _subViewportRoot;
	public Array<DimensionRule> _dimensionRules  = new Array<DimensionRule>();


	public void Init(int dim)
	{
		_dimOrder = dim;
		Name = $"{_dimOrder}D";
		
		
		
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
					AnchorMode = Camera2D.AnchorModeEnum.FixedTopLeft,
					CustomViewport = _subViewportRoot
				};
				_subViewportRoot.AddChild(camera2D);
			}
			else
			{
				var camera3D = new Camera3D
				{
					Name = "MainCamera",
					Current = true,
					Position = new Vector3(0, 0, 10)
				};
				_subViewportRoot.AddChild(camera3D);
				camera3D.LookAtFromPosition(camera3D.Position ,new Vector3(0,0,50));
			}
			
			Texture = _subViewportRoot.GetTexture();
		}
		else
		{
			GD.PushWarning("SubViewport manquant dans TextureRectViewportDisplayer");
		}
	}

	public bool AddRuleIfNotAlreadyContained(DimensionRule rule)
	{
		if (_dimensionRules.Any(r => r.GetType() == rule.GetType()))
		{
			GD.PrintErr($"La regle {rule.GetType()} ne peut etre ajoutée car elle existe deja dans la dimension {_dimOrder}D");
			return false;
		}

		_dimensionRules.Add(rule);
		return true;
	}



	public void Pause(bool pause)
	{
		foreach (var rule in _dimensionRules)
		{
			rule.Enabled = !pause;

			if (rule.HelperNode != null)
			{
				rule.HelperNode.SetProcess(!pause);
				rule.HelperNode.SetProcessInput(!pause);
			}
		}
	}

}
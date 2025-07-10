using System;
using Godot;

namespace Dim.Rules.SpecificRules;

[GlobalClass]
public partial class UnifiedColorScreenRule : DimensionRule
{
	private ColorRect _existingColorRect;
	[Export] public Color ChosenColor { get; set; } = new Color (1,1,1);// blanc par défaut
	[Export] public bool RandomColor = false;


	protected override void ApplyPonctually()
	{
		// Supprimer l'ancien ColorRect s'il existe
		_existingColorRect = SubViewportRootRef.GetNodeOrNull<ColorRect>("BackgroundColor");
		if (_existingColorRect != null)
		{
			_existingColorRect.QueueFree();
		}

		// Créer un nouveau ColorRect pour le fond
		var colorRect = new ColorRect
		{
			Name = "BackgroundColor",
			Color = RandomColor?new Color((float)GD.RandRange(0.0,1.0),(float)GD.RandRange(0.0,1.0),(float)GD.RandRange(0.0,1.0),1f):ChosenColor,
			ZIndex = -1, // S'assure qu'il est derrière tout
			LayoutMode = 1, // Mode layout proportionnel
			AnchorsPreset = (int)Control.LayoutPreset.FullRect, // Remplit tout l'espace
			Position = Vector2.Zero
		};

		// Ajouter le ColorRect comme premier enfant du SubViewport
		SubViewportRootRef.AddChild(colorRect);
		SubViewportRootRef.MoveChild(colorRect, 0); // Le place en premier dans la hiérarchie

	}

	protected override void UnApplyPonctually()
	{
		if(_existingColorRect != null)
			_existingColorRect.QueueFree();
	}

	protected override void AddCommonHelperNodeMethods()
	{
		
	}
}

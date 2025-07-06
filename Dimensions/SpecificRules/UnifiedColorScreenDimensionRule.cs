using Godot;

namespace Dim.Dimensions.SpecificRules;

[GlobalClass]
public partial class UnifiedColorScreenDimensionRule : StartingDimensionRule
{
    [Export] public Color ChosenColor { get; set; } = new Color ();// blanc par défaut




    public override void Apply()
    {
        if (_subViewportRoot == null)
        {
            GD.PrintErr("_subViewportRoot est null dans UnifiedColorScreenDimensionRule");
            return;
        }

        GD.Print($"Application de la couleur {ChosenColor} à la dimension");

        // Supprimer l'ancien ColorRect s'il existe
        var existingColorRect = _subViewportRoot.GetNodeOrNull<ColorRect>("BackgroundColor");
        if (existingColorRect != null)
        {
            existingColorRect.QueueFree();
        }

        // Créer un nouveau ColorRect pour le fond
        var colorRect = new ColorRect
        {
            Name = "BackgroundColor",
            Color = ChosenColor,
            ZIndex = -1, // S'assure qu'il est derrière tout
            LayoutMode = 1, // Mode layout proportionnel
            AnchorsPreset = (int)Control.LayoutPreset.FullRect, // Remplit tout l'espace
            Position = Vector2.Zero
        };

        // Ajouter le ColorRect comme premier enfant du SubViewport
        _subViewportRoot.AddChild(colorRect);
        _subViewportRoot.MoveChild(colorRect, 0); // Le place en premier dans la hiérarchie
    }
}
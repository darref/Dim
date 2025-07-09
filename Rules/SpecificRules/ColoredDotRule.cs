using Godot;

namespace Dim.Rules.SpecificRules;

[GlobalClass]
public partial class ColoredDotRule : DimensionRule
{
    [Export] public Color DotColor { get; set; } = new Color(1, 0, 0); // Rouge par défaut
    
    public override void ApplyPonctually()
    {
        if (_subViewportRoot == null) return;

        var existingDot = _subViewportRoot.GetNodeOrNull<ColorRect>("CenteredDot");
        if (existingDot != null)
        {
            existingDot.QueueFree();
        }

        // Utiliser la taille réelle de la fenêtre
        var viewportSize = DisplayServer.WindowGetSize();
        
        var dot = new ColorRect
        {
            Name = "CenteredDot",
            Color = DotColor,
            Size = new Vector2(20, 20),
            // Centrer par rapport à la taille réelle de la fenêtre
            Position = (Vector2)viewportSize / 2 - new Vector2(10, 10)
        };

        _subViewportRoot.AddChild(dot);
        
        base.ValidationMessageConsole();
    }

    public override void DefineCommonNodeMethods()
    {
        
    }
}
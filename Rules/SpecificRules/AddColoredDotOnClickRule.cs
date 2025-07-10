using Godot;

namespace Dim.Rules.SpecificRules;

[GlobalClass]
public partial class AddColoredDotOnClickRule : DimensionRule
{
    private ColorRect _dot;
    private bool _done;
    [Export] public Color DotColor { get; set; } = new(1, 0, 0); // Rouge par défaut
    [Export] private bool OneShot { get; set; }
    [Export] public MouseButton MouseButtonForApplyingDot { get; set; } = MouseButton.Left;
    [Export] public MouseButton MouseButtonForDeletingDot { get; set; } = MouseButton.Right;

    protected override void AddCommonHelperNodeMethods()
    {
        HelperNode.OnReady += () =>
        {
            ApplyPermanently = false;
            ApplyOnEnd = false;
            ApplyOnStart = false;
            //
            if (SubViewportRootRef == null) return;
            // Centre initial de la souris
            var center = SubViewportRootRef.Size / 2;
            Input.WarpMouse(center);
            // Activer les InputEvents
            DimensionNodeRef.SetProcessInput(true);
        };
        HelperNode.OnInput += e =>
        {
            if (e is InputEventMouseButton mouseButton && mouseButton.Pressed)
            {
                if (mouseButton.ButtonIndex == MouseButtonForApplyingDot)
                    ApplyPonctually();
                if (mouseButton.ButtonIndex == MouseButtonForDeletingDot)
                    UnApplyPonctually();
            }
        };
    }


    protected override void ApplyPonctually()
    {
        if (OneShot && _done) return;
        if (_dot != null) return;
        var dot = new ColorRect
        {
            Name = "CenteredDot",
            Color = DotColor,
            Size = new Vector2(20, 20),
            Position = DisplayServer.MouseGetPosition()
        };
        _dot = dot;
        SubViewportRootRef.AddChild(dot);
        _done = true;
    }

    protected override void UnApplyPonctually()
    {
        if (_dot == null) return;
        if (OneShot) return;
        _dot.QueueFree();
        _dot = null;
    }
}
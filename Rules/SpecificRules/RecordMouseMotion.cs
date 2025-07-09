using System.Collections.Generic;
using Dim.Dimensions;
using Godot;

namespace Dim.Rules.SpecificRules;

[GlobalClass]
public partial class RecordMouseMotion : DimensionRule
{
    private Line2D _line;
    private readonly List<Vector2> _recordedPoints = new();
    [Export] public Color LineColor { get; set; } = new(0.5f, 0.5f, 0.5f);
    [Export] public bool DoesDeletingLineDestroyHigherDimensions { get; set; }
    [Export] public MouseButton MouseButtonForEndRecording { get; set; } = MouseButton.Left;
    [Export] public MouseButton MouseButtonForResetRecording { get; set; } = MouseButton.Right;


    protected override void DefineCommonHelperNodeMethods()
    {
        HelperNode.OnInput += e =>
        {
            if (e is InputEventMouseMotion motion)
            {
                var globalPos = motion.Position;
                _recordedPoints.Add(globalPos);
            }
            if (e is InputEventMouseButton mouseButton && mouseButton.Pressed)
            {
                if (mouseButton.ButtonIndex == MouseButtonForEndRecording)
                    ApplyPonctually();
                else if (mouseButton.ButtonIndex == MouseButtonForResetRecording)
                    UnApplyPonctually();
            }
        };
        HelperNode.OnReady += () =>
        {
            ApplyPermanently = false;
            ApplyOnEnd = false;
            ApplyOnStart = true;
            // Initialise la ligne
            _line = new Line2D
            {
                DefaultColor = LineColor,
                Width = 2.0f,
                Antialiased = true,
                Visible = false
            };
            DimensionNode.AddChild(_line);
            // Centre initial de la souris
            var center = SubViewportRoot.Size / 2;
            Input.WarpMouse(center);
            // Connecte le signal de mouvement de souris (sur `_dimensionNode`)
            DimensionNode.Connect("mouse_entered", Callable.From(() => { DimensionNode.SetProcessInput(true); }));
            DimensionNode.SetProcess(true);
            DimensionNode.SetProcessInput(true);
        };
        
    }


    protected override void ApplyPonctually()
    {
        if (_recordedPoints.Count < 2 || _line == null)
        {
            GD.Print("Pas assez de points pour dessiner une ligne.");
            return;
        }

        _line.ClearPoints();
        _line.Visible = true;
        foreach (var point in _recordedPoints) _line.AddPoint(point);

        // Optionnel : lisser la ligne ?
        // TODO : ajouter algorithme de lissage si besoin
    }

    protected override void UnApplyPonctually()
    {
        _recordedPoints.Clear();
        if (_line != null)
        {
            _line.ClearPoints();
            _line.Visible = false;
        }
    }
}
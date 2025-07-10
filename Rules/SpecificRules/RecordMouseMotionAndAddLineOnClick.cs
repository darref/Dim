using System.Collections.Generic;
using Dim.Dimensions;
using Godot;

namespace Dim.Rules.SpecificRules;

[GlobalClass]
public partial class RecordMouseMotionAndAddLineOnClick : DimensionRule
{
    private Line2D _line;
    private readonly List<Vector2> _recordedPoints = new();
    [Export] public Color LineColor { get; set; } = new(0.5f, 0.5f, 0.5f);
    [Export] public bool DoesDeletingLineDestroyHigherDimensions { get; set; }
    [Export] public MouseButton MouseButtonForEndRecording { get; set; } = MouseButton.Left;
    [Export] public MouseButton MouseButtonForResetRecording { get; set; } = MouseButton.Right;


    protected override void AddCommonHelperNodeMethods()
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
            // Initialise la ligne
            _line = new Line2D
            {
                DefaultColor = LineColor,
                Width = 2.0f,
                Antialiased = true,
                Visible = false
            };
            DimensionNodeRef.AddChild(_line);
            // Centre initial de la souris
            var center = SubViewportRootRef.Size / 2;
            Input.WarpMouse(center);
           
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
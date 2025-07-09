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
    [Export] public bool doesDeletingLineDestroyHigherDimensions { get; set; }
    [Export] public MouseButton mouseButtonForEndRecording { get; set; } = MouseButton.Left;
    [Export] public MouseButton mouseButtonForResetRecording { get; set; } = MouseButton.Right;


    public override void ApplyPonctually()
    {
        
    }

    public override void DefineCommonNodeMethods()
    {
        _helperNode.Name = "HelperFor" + GetType().Name;
        _helperNode.OnInput = e =>
        {
            if (_dimensionNode == null) return;

            if (e is InputEventMouseMotion motion)
            {
                var globalPos = motion.Position;
                _recordedPoints.Add(globalPos);
            }


            if (e is InputEventMouseButton mouseButton && mouseButton.Pressed)
            {
                if (mouseButton.ButtonIndex == mouseButtonForEndRecording)
                    FinalizeRecording();
                else if (mouseButton.ButtonIndex == mouseButtonForResetRecording)
                    ResetRecording();
            }
        };
        _helperNode.OnReady = () =>
        {
            _applyPermanently = false;
            _applyOnEnd = false;
            _applyOnStart = true;
            //
            if (_subViewportRoot == null) return;
            // Message de bienvenue

            // Initialise la ligne
            _line = new Line2D
            {
                DefaultColor = LineColor,
                Width = 2.0f,
                Antialiased = true,
                Visible = false
            };
            _dimensionNode.AddChild(_line);

            // Centre initial de la souris
            var center = _subViewportRoot.Size / 2;
            Input.WarpMouse(center);

            // Connecte le signal de mouvement de souris (sur `_dimensionNode`)
            _dimensionNode.Connect("mouse_entered", Callable.From(() => { _dimensionNode.SetProcessInput(true); }));

            _dimensionNode.SetProcess(true);
            _dimensionNode.SetProcessInput(true);
            
            base.ValidationMessageConsole();
        };
        
    }


    public void FinalizeRecording()
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

    public void ResetRecording()
    {
        _recordedPoints.Clear();
        if (_line != null)
        {
            _line.ClearPoints();
            _line.Visible = false;
        }
    }
}
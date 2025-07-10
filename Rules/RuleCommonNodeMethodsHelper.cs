using System;
using Godot;

namespace Dim.Rules;

public partial class RuleCommonNodeMethodsHelper : Node
{
    private DimensionRule _dimensionRuleRef;
    public Action OnExit;
    public Action<InputEvent> OnInput;
    public Action<float> OnProcessFrame;
    public Action OnReady;

    public void Init(DimensionRule dimensionRule)
    {
        _dimensionRuleRef = dimensionRule;
    }
    public override void _Ready()
    {
        // GD.Print($"appelé malgré {EnabledEvents}");
        if (_dimensionRuleRef.Enabled )
            OnReady?.Invoke();
        
        
        
    }

    public override void _Process(double delta)
    {
        // GD.Print($"appelé malgré {EnabledEvents}");
        if (_dimensionRuleRef.Enabled )
            OnProcessFrame?.Invoke((float)delta);
    }

    public override void _Input(InputEvent @event)
    {
        // GD.Print($"appelé malgré {EnabledEvents}");
        if (_dimensionRuleRef.Enabled )
            OnInput?.Invoke(@event);
    }

    public override void _ExitTree()
    {
        // GD.Print($"appelé malgré {EnabledEvents}");
        if (_dimensionRuleRef.Enabled )
            OnExit?.Invoke();
    }

    
}
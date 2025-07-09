using System;
using Godot;

namespace Dim.Rules;

public partial class RuleCommonNodeMethodsHelper : Node
{
    // Délégués personnalisés
    public Action<float>? OnProcessFrame;
    public Action<InputEvent>? OnInput;
    public Action? OnReady;
    public Action? OnExit;

    public override void _Ready()
    {
        OnReady?.Invoke();
        SetProcess(true);
        SetProcessInput(true);
    }

    public override void _Process(double delta)
    {
        OnProcessFrame?.Invoke((float)delta);
    }

    public override void _Input(InputEvent @event)
    {
        OnInput?.Invoke(@event);
    }
    public override void _ExitTree()
    {
        OnExit?.Invoke();
    }
}
using System;
using Godot;

namespace Dim.Utils;

public partial class EventBus : Node
{
    public static EventBus Singleton { get; private set; }
    

    public override void _EnterTree()
    {
        Singleton = this;
    }
    //
    public static event Action OnToggleFullscreen;
    public static void EmitToggleFullscreen()
    {
        OnToggleFullscreen?.Invoke();
    }
    //
}
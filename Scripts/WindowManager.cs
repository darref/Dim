using Godot;
using System;

namespace Dim.Scripts;

public partial class WindowManager : Node
{
    public static WindowManager Instance { get; private set; }


    
    public static float _scale = 1f;

    private Timer _resizeEndTimer;

    // Signal que tu peux connecter depuis d'autres classes
    [Signal]
    public delegate void ResizeFinishedEventHandler();

    public override void _Ready()
    {
        Instance = this;
        // Timer pour détecter la fin du redimensionnement
        _resizeEndTimer = new Timer
        {
            WaitTime = 0.3f,
            OneShot = true,
            Autostart = false
        };
        AddChild(_resizeEndTimer);
        _resizeEndTimer.Timeout += OnResizeFinished;

        // Démarrage en fullscreen
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMSizeChanged)
        {
            // Relance le timer à chaque resize
            _resizeEndTimer.Stop();
            _resizeEndTimer.Start();
        }
    }

    private void OnResizeFinished()
    {
        EmitSignal(SignalName.ResizeFinished);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("toggle_fullscreen") && Input.IsKeyPressed(Key.Enter))
        {
            ToggleFullscreen();
        }
    }

    private void AjusterTailleFenetre(float scale = 0.9f)
    {
        _scale = scale;

        Vector2I tailleEcran = DisplayServer.ScreenGetSize();
        Vector2I nouvelleTaille = new(
            (int)(tailleEcran.X * scale),
            (int)(tailleEcran.Y * scale)
        );
        Vector2I position = new(
            (tailleEcran.X - nouvelleTaille.X) / 2,
            (tailleEcran.Y - nouvelleTaille.Y) / 2
        );

        DisplayServer.WindowSetSize(nouvelleTaille);
        DisplayServer.WindowSetPosition(position);
    }

    public void ToggleFullscreen()
    {
        if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            AjusterTailleFenetre(0.9f);
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Again.Datas;
using Again.Dimensions;
using Godot;
using Godot.Collections;

namespace Again.UI;

[GlobalClass]
public partial class DimensionsManager : Control
{
    //dim
    private List<Dimension> _dimensions = new();
    private PackedScene _dimensionScene;
    private const string DIMENSION_SCENE_PATH = "res://Dimensions/dimension.tscn";

    //slider
    private Control _viewContainer;
    private AudioStreamPlayer2D audioPlayer = new();
    private AudioStreamPlayer2D audioPlayerAmbiance = new();
    public int currentIndex;
    public bool isSlideDisabled = false;
    private bool isSliding;
    private bool _enabledNavigation = true;
    [Export] public Array<AudioStreamEntry> EntriesMusic { get; set; } = new();
    [Export] public Array<AudioStreamEntry> EntriesAmbiance { get; set; } = new();


    public override void _Ready()
    {
        // Configuration initiale du DimensionsManager
        CustomMinimumSize = Vector2.Zero;
        Size = Vector2.Zero;
        AnchorsPreset = (int)LayoutPreset.FullRect; // Remplit tout l'espace parent
        
        // Création du ViewContainer avec layout approprié
        _viewContainer = new Control
        {
            Name = "ViewContainer",
            MouseFilter = MouseFilterEnum.Ignore,
            LayoutMode = 1, // Mode layout proportionnel
            AnchorsPreset = (int)LayoutPreset.FullRect,
            Position = Vector2.Zero
        };
        AddChild(_viewContainer);

        //dim
        _dimensionScene = GD.Load<PackedScene>(DIMENSION_SCENE_PATH);
        var wm = Utils.NodeManagement.FindUniqueNamedNodeEverywhere(GetTree().Root, "WindowManager");
        wm.Connect(Utils.Signals.toggleFullscreenSignal.Name, 
            Callable.From<string>(UpdateLayout));
            
        AddNewDimension(0);
        AddNewDimension(1);

        //slider
        audioPlayer = new AudioStreamPlayer2D();
        AddChild(audioPlayer);
        audioPlayer.Stream = EntriesMusic[0]?.Stream;
        audioPlayerAmbiance = new AudioStreamPlayer2D();
        AddChild(audioPlayerAmbiance);
        audioPlayerAmbiance.Stream = EntriesAmbiance[0]?.Stream;
        audioPlayer.Play();

        //updateDisplay 
        UpdateLayout();
    }

    private void UpdateLayout(string windowMode = "")
    {
        if (EntriesAmbiance.Count == 0 || EntriesMusic.Count == 0) 
            GD.PrintErr("pas d'ambiance ou de musique");
        
        Vector2I windowSize = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen 
            ? DisplayServer.ScreenGetSize() 
            : DisplayServer.WindowGetSize();

        // Configuration du DimensionsManager pour remplir la fenêtre
        Size = windowSize;
        Position = Vector2.Zero;

        // Configuration des dimensions
        foreach (var dimension in _dimensions)
        {
            // Configuration du layout de la dimension
            dimension.LayoutMode = 1;
            dimension.AnchorsPreset = (int)LayoutPreset.FullRect;
            dimension.Size = windowSize;
            
            // Configuration du SubViewport
            var subViewport = dimension.GetNode<SubViewport>("SubViewport");
            if (subViewport != null)
            {
                subViewport.Size = windowSize;
                subViewport.SetUpdateMode(SubViewport.UpdateMode.Always);
                
                // Forcer la mise à jour de la texture
                dimension.Texture = subViewport.GetTexture();
            }
        }

        // Positionner les dimensions de gauche à droite
        float positionX = 0;
        foreach (var dimension in _dimensions)
        {
            dimension.Position = new Vector2(positionX, 0);
            positionX += windowSize.X;
        }

        // Configuration du ViewContainer
        Vector2 containerSize = new(windowSize.X * _dimensions.Count, windowSize.Y);
        _viewContainer.Size = containerSize;
        _viewContainer.Position = new Vector2(-currentIndex * windowSize.X, 0);
    }

    public void AddNewDimension(int dim)
    {
        var newDimension = _dimensionScene.Instantiate();
        var dimension = newDimension as Dimension;
        if (dimension == null) GD.Print("Erreur de casting de la dimension dans AddNewDimension");
        
        // Configuration du layout avant l'initialisation
        dimension.LayoutMode = 1; // Mode layout proportionnel
        dimension.AnchorsPreset = (int)LayoutPreset.FullRect;
        
        dimension.Init(dim);
        dimension.defineRules();
        dimension.applyRules();
        _viewContainer.AddChild(dimension);
        _dimensions.Add(dimension);
        UpdateLayout();
    }

    public void SlideToView(int targetIndex)
    {
        if (!_enabledNavigation) return;
        if (targetIndex == currentIndex) return;
        if(targetIndex < 0 || targetIndex > _dimensions.Count-1) return;
        
        GD.Print($"Sliding Dimension from {currentIndex} to {targetIndex}");

        isSliding = true;
        var tween = CreateTween();
        tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);

        var toX = -targetIndex * Size.X;
        tween.TweenProperty(_viewContainer, "position:x", toX, 0.5);

        tween.TweenCallback(Callable.From(() =>
        {
            currentIndex = targetIndex;
            isSliding = false;

            if (EntriesMusic.Count > 0)
            {
                audioPlayer.Stream = EntriesMusic[currentIndex]?.Stream;
                audioPlayerAmbiance.Stream = EntriesAmbiance[currentIndex]?.Stream;
                audioPlayer.Play();
                audioPlayerAmbiance.Play();
            }
        }));
    }




    public void ToggleFullscreen()
    {
        if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        }
    }

    ///////////////////////////////////////////////
    public override void _Input(InputEvent @event)
    {
        if (isSlideDisabled) return;
        

        if (@event is InputEventJoypadMotion btn && btn.GetAxisValue() > 0.8f)
        {
            if (btn.GetAxis() == JoyAxis.TriggerLeft)
            {
                if (currentIndex > 0) SlideToView(currentIndex - 1);
            }
            else if (btn.GetAxis() == JoyAxis.TriggerRight)
            {
                if (currentIndex < 2) SlideToView(currentIndex + 1);
            }
            GD.Print("Input detected on DimensionsManager");
        }
        else if (@event is InputEventKey btnPC && btnPC.Pressed)
        {
            if (btnPC.Keycode == Key.Left)
                if (currentIndex > 0)
                    SlideToView(currentIndex - 1);

            if (btnPC.Keycode == Key.Right)
                if (currentIndex < 2)
                    SlideToView(currentIndex + 1);
            GD.Print("Input detected on DimensionsManager");
        }
    }

    

    public void EnableNavigation(bool b)
    {
        _enabledNavigation = b;
    }

}
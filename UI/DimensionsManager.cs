using System;
using System.Collections.Generic;
using Dim.Datas;
using Dim.Dimensions;
using Dim.Rules.Laws;
using Dim.Scripts;
using Dim.Utils;
using Godot;
using Godot.Collections;
using DimensionRule = Dim.Rules.DimensionRule;

namespace Dim.UI;

[GlobalClass]
public partial class DimensionsManager : Control
{
    //base scene
    private const string DIMENSION_SCENE_PATH = "res://Dimensions/dimension.tscn";


    //dim
    private readonly List<Dimension> _dimensions = new();
    private PackedScene _dimensionScene;
    private bool _enabledNavigation = true;

    //slider
    private Control _viewContainer;
    private AudioStreamPlayer2D audioPlayer = new();
    private AudioStreamPlayer2D audioPlayerAmbiance = new();
    public int currentIndex;
    public bool isSlideDisabled = false;
    private bool isSliding;

    //laws
    [Export] public Array<LawEntriesByOrder> lawOrders { get; set; } = new();
    [Export] public Array<AudioStreamEntry> EntriesMusic { get; set; } = new();
    [Export] public Array<AudioStreamEntry> EntriesAmbiance { get; set; } = new();

    

    public override void _Ready()
    {
        WindowManager.Instance.ResizeFinished += UpdateLayout;

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
        AddNewDimension(0);
        AddNewDimension(1);
        AddNewDimension(2);
        AddNewDimension(3);

        //audio
        if (EntriesAmbiance.Count != 0 && EntriesMusic.Count != 0)
        {
            audioPlayer = new AudioStreamPlayer2D();
            AddChild(audioPlayer);
            audioPlayer.Stream = EntriesMusic[0]?.Stream;
            audioPlayerAmbiance = new AudioStreamPlayer2D();
            AddChild(audioPlayerAmbiance);
            audioPlayerAmbiance.Stream = EntriesAmbiance[0]?.Stream;
            audioPlayer.Play();
        }


        
    }

    private void UpdateLayout()
    {
        // Obtenir la taille réelle de la fenêtre ou de l'écran selon le mode
        Vector2 windowSize = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed
            ? DisplayServer.WindowGetSize()
            : DisplayServer.ScreenGetSize();

        //positionner le manager
        Position = Vector2.Zero;
        CustomMinimumSize = windowSize;
        // LayoutMode = 1; // Mode layout proportionnel
        // AnchorsPreset = (int)LayoutPreset.FullRect;

        // Redimensionner le conteneur global si nécessaire (ex: ViewContainer)
        _viewContainer.Position = new Vector2(-currentIndex * windowSize.X, 0);
        _viewContainer.CustomMinimumSize = new Vector2(windowSize.X * _dimensions.Count , windowSize.Y);
        // _viewContainer.LayoutMode = 1; // Mode layout proportionnel
        // _viewContainer.AnchorsPreset = (int)LayoutPreset.FullRect;
        

        // Positionner les dimensions horizontalement
        float positionX = 0;
        foreach (var dimension in _dimensions)
        {

            

            dimension.LayoutMode = 0; 
            dimension.Position = new Vector2(positionX, 0);
            positionX += windowSize.X;
            dimension.CustomMinimumSize = windowSize;
            dimension.Size = windowSize;
            
        }
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
        addLawsToDimension(dimension);
        _viewContainer.AddChild(dimension);
        _dimensions.Add(dimension);
        UpdateLayout();
        GD.Print("Dimension " + dim + " créée etajoutée");
    }


    private void addLawsToDimension(Dimension dimension)
    {
        if (lawOrders == null)
        {
            GD.PrintErr("lawOrders est null");
            return;
        }

        foreach (var laworder in lawOrders)
        {
            if (laworder?.LawEntries == null) continue;

            foreach (var lawEntry in laworder.LawEntries)
            {
                if (lawEntry == null || lawEntry.Rule == null)
                {
                    GD.PrintErr("lawEntry ou sa règle est null");
                    continue;
                }

                if (dimension._dimOrder == laworder.Order && lawEntry.Enabled)
                    try
                    {
                        if (lawEntry.Rule == null)
                        {
                            GD.PrintErr("Aucune règle assignée dans LawEntry");
                            continue;
                        }

                        var lawInstance = lawEntry.Rule.Duplicate() as DimensionRule;
                        if (lawInstance == null)
                        {
                            GD.PrintErr("Échec de la duplication de la règle");
                            continue;
                        }

                        lawInstance.Init(
                            dimension._subViewportRoot,
                            dimension._dimOrder,
                            lawEntry._applyOnStart,
                            lawEntry._applyOnEnd,
                            lawEntry._applyPermanently
                        );

                        dimension._dimensionRules.Add(lawInstance);
                    }
                    catch (Exception e)
                    {
                        GD.PrintErr($"Erreur lors du chargement de la règle : {e.Message}");
                    }
            }
        }
    }


    public void SlideToView(int targetIndex)
    {
        if (!_enabledNavigation) return;
        if (isSliding) return;
        if (targetIndex == currentIndex) return;
        if (targetIndex < 0 || targetIndex >= _dimensions.Count) return;

        GD.Print($"Sliding Dimension from {currentIndex} to {targetIndex}");

        isSliding = true;

        // Facultatif : s'assurer que layout est bon avant de glisser
        // UpdateLayout(); ← à activer uniquement si nécessaire

        var tween = CreateTween();
        tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);

        var windowSize = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed
            ? DisplayServer.WindowGetSize()
            : DisplayServer.ScreenGetSize();

        var fromPos = _viewContainer.Position;
        var toPos = new Vector2(-targetIndex * _dimensions[targetIndex].Size.X, fromPos.Y);

        tween.TweenProperty(_viewContainer, "position", toPos, 0.5);

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
                if (currentIndex < _dimensions.Count - 1) // Utiliser le nombre réel de dimensions
                    SlideToView(currentIndex + 1);
            }
        }
        else if (@event is InputEventKey btnPC && btnPC.Pressed)
        {
            if (btnPC.Keycode == Key.Left)
                if (currentIndex > 0)
                    SlideToView(currentIndex - 1);

            if (btnPC.Keycode == Key.Right)
                if (currentIndex < _dimensions.Count - 1) // Utiliser le nombre réel de dimensions
                    SlideToView(currentIndex + 1);
        }
    }


    public void EnableNavigation(bool b)
    {
        _enabledNavigation = b;
    }
}
using System;
using System.Collections.Generic;
using Dim.Datas;
using Dim.Dimensions;
using Dim.Utils;
using Godot;
using Godot.Collections;

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
	[Export] public Array<LawEntryByOrder> lawOrders { get; set; } = new();
	[Export] public Array<AudioStreamEntry> EntriesMusic { get; set; } = new();
	[Export] public Array<AudioStreamEntry> EntriesAmbiance { get; set; } = new();


	public override void _Ready()
	{

		//Signals
		EventBus.OnToggleFullscreen += UpdateLayout;

		// Configuration initiale du DimensionsManager
		CustomMinimumSize = Vector2.Zero;
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
		AddNewDimension(0);
		AddNewDimension(1);

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


		//updateDisplay 
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		var windowSize = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen
			? DisplayServer.ScreenGetSize()
			: DisplayServer.WindowGetSize();

		// Configuration du DimensionsManager pour remplir la fenêtre
		CustomMinimumSize = windowSize;
		Position = Vector2.Zero;

		// Configuration des dimensions
		foreach (var dimension in _dimensions)
		{
			// Configuration du layout de la dimension
			dimension.LayoutMode = 1;
			dimension.AnchorsPreset = (int)LayoutPreset.FullRect;
			dimension.CustomMinimumSize = windowSize;

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
		_viewContainer.CustomMinimumSize = containerSize;
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
		addLawsToDimension(dimension);
		dimension.defineRules();
		dimension.applyRules();
		_viewContainer.AddChild(dimension);
		_dimensions.Add(dimension);
		UpdateLayout();
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
						var law = ResourceLoader.Load<DimensionRule>(lawEntry.Rule.ResourcePath);
						if (law == null)
						{
							GD.PrintErr($"Impossible de charger la ressource: {lawEntry.Rule.ResourcePath}");
							continue;
						}

						var lawInstance = law.Duplicate() as DimensionRule;
						if (lawInstance == null)
						{
							GD.PrintErr("Échec de la duplication de la règle");
							continue;
						}

						if (lawInstance is StartingDimensionRule sdr)
						{
							sdr.Init(dimension._subViewportRoot);
							dimension._startingRules.Add(sdr);
						}
						else if (lawInstance is DuringDimensionRule ddr)
						{
							ddr.Init(dimension._subViewportRoot);
							dimension._duringRules.Add(ddr);
						}
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
		if (targetIndex == currentIndex) return;
		if (targetIndex < 0 || targetIndex > _dimensions.Count - 1) return;

		GD.Print($"Sliding Dimension from {currentIndex} to {targetIndex}");

		isSliding = true;
		var tween = CreateTween();
		tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);

		var toX = -targetIndex * CustomMinimumSize.X;
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

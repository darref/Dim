using System;
using System.Collections.Generic;
using System.Linq;
using Dim.Datas;
using Dim.Dimensions;
using Dim.Rules.Laws;
using Dim.Scripts;
using Godot;
using Godot.Collections;
using DimensionRule = Dim.Rules.DimensionRule;

namespace Dim.UI;

public enum EnumDimensionSwitchingMode
{
	Slide,
	Fade
}

[GlobalClass]
public partial class DimensionsManager : Control
{
	// base scene
	private const string DIMENSION_SCENE_PATH = "res://Dimensions/dimension.tscn";

	// dim
	private readonly List<Dimension> _dimensions = new();
	public EnumDimensionSwitchingMode _dimensionChangeMode = EnumDimensionSwitchingMode.Fade;
	private PackedScene _dimensionScene;
	private bool _enabledNavigation = true;

	// slider
	private Control _viewContainer;
	private AudioStreamPlayer2D audioPlayer = new();
	private AudioStreamPlayer2D audioPlayerAmbiance = new();
	public int currentIndex;

	private bool IsChangingView;

	// laws
	[Export] public Array<LawEntriesByOrder> lawOrders { get; set; } = new();
	[Export] public Array<AudioStreamEntry> EntriesMusic { get; set; } = new();
	[Export] public Array<AudioStreamEntry> EntriesAmbiance { get; set; } = new();

	public void UpdateLayoutGeneral()
	{
		_dimensions.Sort((a, b) => a._dimOrder.CompareTo(b._dimOrder));
		switch (_dimensionChangeMode)
		{
			case EnumDimensionSwitchingMode.Slide:
				UpdateLayoutSliderMode();
				break;
			case EnumDimensionSwitchingMode.Fade:
				UpdateLayoutFade();
				break;
		}
	}

	public override void _Ready()
	{
		WindowManager.Instance.ResizeFinished += UpdateLayoutGeneral;

		_viewContainer = new Control
		{
			Name = "ViewContainer",
			MouseFilter = MouseFilterEnum.Ignore,
			LayoutMode = 1,
			AnchorsPreset = (int)LayoutPreset.FullRect,
			Position = Vector2.Zero
		};
		AddChild(_viewContainer);

		_dimensionScene = GD.Load<PackedScene>(DIMENSION_SCENE_PATH);
		//créer les dimensions
		if (lawOrders == null)
		{
			GD.PrintErr("lawOrders est null !");
			return;
		}
		foreach (var laworder in lawOrders)
		{
			if (laworder == null)
			{
				GD.PrintErr("Une entrée null détectée dans lawOrders !");
				continue;
			}

			AddNewDimension(laworder.Order);
		}

		//musiques et ambiances
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

		InitChangingView();
		UpdateLayoutGeneral();
		ChangeView(currentIndex);
	}

	private void InitChangingView()
	{
		currentIndex = _dimensions[0]._dimOrder;
		foreach (var dimension in _dimensions)
		{
			
			switch (_dimensionChangeMode)
			{
				case EnumDimensionSwitchingMode.Slide:
					dimension.ZIndex = 100;
					dimension.Modulate = new Color(1, 1, 1, 1);
					break;
				case EnumDimensionSwitchingMode.Fade:
					dimension.ZIndex = dimension._dimOrder == currentIndex ? 100 : 0;
					dimension.Modulate = dimension._dimOrder == currentIndex?  new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0);
					break;
			}
		}
		
	}
	

	public void SetChangingDimensionMode(EnumDimensionSwitchingMode mode)
	{
		_dimensionChangeMode = mode;
	}

	private void UpdateLayoutSliderMode()
	{
		Vector2 windowSize = DisplayServer.ScreenGetSize();

		Position = Vector2.Zero;
		CustomMinimumSize = windowSize;

		_viewContainer.Position = new Vector2(-currentIndex * windowSize.X, 0);
		_viewContainer.CustomMinimumSize = new Vector2(windowSize.X * _dimensions.Count, windowSize.Y);

		foreach (var dimension in _dimensions)
		{
			// Taille de la dimension = taille de l'écran/fenêtre
			dimension.LayoutMode = 0;
			dimension.CustomMinimumSize = windowSize;
			// Position horizontale en fonction de son ordre
			dimension.Position = new Vector2(windowSize.X * dimension._dimOrder, 0);
		}
	}


	public void SlideToView(int targetIndex)
	{
		if (!_enabledNavigation || IsChangingView )
			return;

		GD.Print($"Sliding Dimension from {currentIndex} to {targetIndex}");
		IsChangingView = true;
		UpdateLayoutSliderMode();

		var tween = CreateTween();
		tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);

		var windowSize = DisplayServer.ScreenGetSize();

		var fromPos = _viewContainer.Position;
		var toPos = new Vector2(-targetIndex * windowSize.X, fromPos.Y);
		tween.TweenProperty(_viewContainer, "position", toPos, 0.5);

		tween.TweenCallback(Callable.From(() =>
		{
			currentIndex = targetIndex;
			IsChangingView = false;
			if (EntriesMusic.Count > 0)
			{
				audioPlayer.Stream = EntriesMusic[currentIndex]?.Stream;
				audioPlayerAmbiance.Stream = EntriesAmbiance[currentIndex]?.Stream;
				audioPlayer.Play();
				audioPlayerAmbiance.Play();
			}
		}));
	}

	private void UpdateLayoutFade()
	{
		Vector2 windowSize = DisplayServer.ScreenGetSize();

		foreach (var dimension in _dimensions)
		{
			dimension.LayoutMode = 0;
			dimension.Position = Vector2.Zero;
			dimension.CustomMinimumSize = windowSize;
			dimension.Size = windowSize;
			dimension._subViewportRoot.TransparentBg = true;
		}
		_viewContainer.Position = Vector2.Zero;
	}

	public void FadeToView(int targetIndex)
	{
		if (!_enabledNavigation || IsChangingView   )
			return;

		IsChangingView = true;
		GD.Print($"Fading Dimension from {currentIndex} to {targetIndex}");

		UpdateLayoutFade();

		// Forcer le zindex initial
		for (var index = 0; index < _dimensions.Count; index++)
			_dimensions[index].ZIndex = index == currentIndex || index == targetIndex ? 100 : 0;
		var tween = CreateTween();
		// Crossfade propre
		tween.TweenProperty(_dimensions[targetIndex], "modulate:a", 1.0f, 0.5);
		tween.TweenProperty(_dimensions[currentIndex], "modulate:a", 0.0f, 0.5);

		tween.TweenCallback(Callable.From(() =>
		{
			currentIndex = targetIndex;
			IsChangingView = false;


			if (EntriesMusic.Count > 0)
			{
				audioPlayer.Stream = EntriesMusic[currentIndex]?.Stream;
				audioPlayerAmbiance.Stream = EntriesAmbiance[currentIndex]?.Stream;
				audioPlayer.Play();
				audioPlayerAmbiance.Play();
			}
		}));
	}


	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventJoypadMotion btn && btn.GetAxisValue() > 0.8f)
		{
			if (btn.GetAxis() == JoyAxis.TriggerLeft && currentIndex > 0)
				ChangeView(currentIndex - 1);
			else if (btn.GetAxis() == JoyAxis.TriggerRight && currentIndex < _dimensions.Count - 1)
				ChangeView(currentIndex + 1);
		}
		else if (@event is InputEventKey btnPC && btnPC.Pressed)
		{
			if (btnPC.Keycode == Key.Left && currentIndex > 0)
				ChangeView(currentIndex - 1);
			else if (btnPC.Keycode == Key.Right && currentIndex < _dimensions.Count - 1)
				ChangeView(currentIndex + 1);
		}
	}

	private void ChangeView(int targetIndex)
	{
		if (!_enabledNavigation || targetIndex == currentIndex) return;
		switch (_dimensionChangeMode)
		{
			case EnumDimensionSwitchingMode.Slide:
				SlideToView(targetIndex);
				break;
			case EnumDimensionSwitchingMode.Fade:
				FadeToView(targetIndex);
				break;
		}
	}

	public void EnableNavigation(bool b)
	{
		_enabledNavigation = b;
	}

	public void AddNewDimension(int dim)
	{
		var newDimension = _dimensionScene.Instantiate() as Dimension;
		if (newDimension == null)
		{
			GD.Print("Erreur de casting de la dimension dans AddNewDimension");
			return;
		}

		newDimension.Init(dim);
		addLawsToDimension(newDimension);
		_viewContainer.AddChild(newDimension);
		_dimensions.Add(newDimension);
	}

	private void addLawsToDimension(Dimension dimension)
	{
		if (lawOrders == null) return;

		foreach (var laworder in lawOrders)
		{
			if (laworder?.LawEntries == null) continue;

			foreach (var lawEntry in laworder.LawEntries)
			{
				if (lawEntry == null || lawEntry.Rule == null || dimension._dimOrder != laworder.Order ||
					!lawEntry.Enabled)
					continue;

				try
				{
					var lawInstance = lawEntry.Rule.Duplicate() as DimensionRule;
					

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
					GD.PrintErr($"Erreur lors de l’instanciation de la règle : {e.Message}");
				}

			}
		}
	}
}

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
	private const string DimensionScenePath = "res://Dimensions/dimension.tscn";
	private readonly float _changingViewTweenDuration = 0.5f;
	private readonly List<Dimension> _dimensions = new();
	private AudioStreamPlayer2D _audioPlayer = new();
	private AudioStreamPlayer2D _audioPlayerAmbiance = new();
	private PackedScene _dimensionScene;
	private bool _enabledNavigation = true;

	private bool _isChangingView;
	private Tween _tween;

	// slider
	private Control _viewContainer;
	private int CurrentIndex;
	public EnumDimensionSwitchingMode DimensionChangeMode = EnumDimensionSwitchingMode.Fade;

	// dim
	[Export] public int NumberOfDimensionToCreateInitially { get; set; } = 10;
	[Export] public bool CreateNegativeAndPositiveInitially { get; set; } = true;

	// laws
	[Export] public Array<OrderLaws> OrderLaws { get; set; } = new();
	[Export] public Array<TransdimensionalLaws> TransdimensionalLaws { get; set; } = new();
	[Export] public Array<AudioStreamEntry> EntriesMusic { get; set; } = new();
	[Export] public Array<AudioStreamEntry> EntriesAmbiance { get; set; } = new();


	public override async void _Ready()
	{
		//frontend
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

		_dimensionScene = GD.Load<PackedScene>(DimensionScenePath);
		//musiques et ambiances
		if (EntriesAmbiance.Count != 0 && EntriesMusic.Count != 0)
		{
			_audioPlayer = new AudioStreamPlayer2D();
			AddChild(_audioPlayer);
			_audioPlayer.Stream = EntriesMusic[0]?.Stream;
			_audioPlayerAmbiance = new AudioStreamPlayer2D();
			AddChild(_audioPlayerAmbiance);
			_audioPlayerAmbiance.Stream = EntriesAmbiance[0]?.Stream;
			_audioPlayer.Play();
		}

		//dimensions and laws
		CreateDimensionsAndApplyAllLaws();
		//apply initial fronted !!! important to initialize with thes calls !!!
		CurrentIndex = 0;
		for (var index = 0; index < 4; index++)
			SetChangingDimensionMode(DimensionChangeMode == EnumDimensionSwitchingMode.Slide
				? EnumDimensionSwitchingMode.Fade
				: EnumDimensionSwitchingMode.Slide);

		
		SleepAllDimensionsAndTheirRulesExceptCurrent();

		foreach (var dimension in _dimensions)
			GD.Print(dimension.Name + " --- order: " + dimension._dimOrder + " --- index: " + _dimensions.IndexOf(dimension));
	}


	//Frontend//////////////////////////////////////////////////////////////////////////////////////////////////////////


	private void UpdateLayoutGeneral()
	{
		if (_isChangingView) return;


		switch (DimensionChangeMode)
		{
			case EnumDimensionSwitchingMode.Slide:
				UpdateLayoutSliderMode();
				break;
			case EnumDimensionSwitchingMode.Fade:
				UpdateLayoutFade();
				break;
		}
	}

	public void SetChangingDimensionMode(EnumDimensionSwitchingMode mode)
	{
		//changement de mode et repercussions sur la vue
		DimensionChangeMode = mode;
		UpdateLayoutGeneral();
		GD.Print(CurrentIndex);
	}

	private void UpdateLayoutSliderMode()
	{
		Vector2 windowSize = DisplayServer.ScreenGetSize();

		CustomMinimumSize = windowSize;
		Position = Vector2.Zero;

		// Utilise une copie triée de _dimensions
		var sortedDimensions = _dimensions.OrderBy(d => d._dimOrder).ToList();
		var currentDimension = sortedDimensions.FirstOrDefault(d => d._dimOrder == CurrentIndex);
		var currentIndexInSorted = sortedDimensions.IndexOf(currentDimension);
		_viewContainer.Position = new Vector2(-currentIndexInSorted * windowSize.X, 0);
		_viewContainer.CustomMinimumSize = new Vector2(windowSize.X * sortedDimensions.Count, windowSize.Y);

		for (var i = 0; i < sortedDimensions.Count; i++)
		{
			var dimension = sortedDimensions[i];
			dimension.LayoutMode = 0;
			dimension.CustomMinimumSize = windowSize;
			dimension.Position = new Vector2(i * windowSize.X, 0f);
			dimension.ZIndex = 100;
			dimension.Modulate = new Color(1, 1, 1);
		}
	}


	private void SlideToView(Dimension targetedDimension, float changingViewTweenDuration)
	{
		if (!_enabledNavigation || _isChangingView)
			return;

		GD.Print($"Sliding Dimension from {CurrentIndex} to {targetedDimension._dimOrder}");
		foreach (var dim in _dimensions)
			dim.Pause(true);
		_isChangingView = true;

		var tween = CreateTween();
		tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);

		var windowSize = DisplayServer.ScreenGetSize();

		var fromPos = _viewContainer.Position;
		// Étape 1 : liste triée
		var sortedDimensions = _dimensions.OrderBy(d => d._dimOrder).ToList();
		var targetIndexInSorted = sortedDimensions.IndexOf(targetedDimension);

		// Étape 3 : calcul du décalage
		var toPos = new Vector2(-targetIndexInSorted * windowSize.X, fromPos.Y);
		tween.TweenProperty(_viewContainer, "position", toPos, changingViewTweenDuration);


		tween.TweenCallback(Callable.From(() =>
		{
			CurrentIndex = targetedDimension._dimOrder;
			SleepAllDimensionsAndTheirRulesExceptCurrent( );
			_isChangingView = false;
			if (EntriesMusic.Count > 0)
			{
				_audioPlayer.Stream = EntriesMusic[CurrentIndex]?.Stream;
				_audioPlayerAmbiance.Stream = EntriesAmbiance[CurrentIndex]?.Stream;
				_audioPlayer.Play();
				_audioPlayerAmbiance.Play();
			}
		}));
	}

	private void UpdateLayoutFade()
	{
		Vector2 windowSize = DisplayServer.ScreenGetSize();

		Position = Vector2.Zero;
		CustomMinimumSize = windowSize;

		_viewContainer.Position = Vector2.Zero;
		_viewContainer.CustomMinimumSize = windowSize;

		foreach (var dimension in _dimensions)
		{
			dimension._subViewportRoot.SetTransparentBackground(true);
			dimension.LayoutMode = 0;
			dimension.Position = Vector2.Zero;
			dimension.CustomMinimumSize = windowSize;
			dimension.Size = windowSize;
			dimension._subViewportRoot.TransparentBg = true;
			dimension.ZIndex = dimension._dimOrder == CurrentIndex ? 100 : 0;
			dimension.Modulate = dimension._dimOrder == CurrentIndex ? new Color(1, 1, 1) : new Color(1, 1, 1, 0);
		}

		var currentDimension = _dimensions.Where(d => d._dimOrder == CurrentIndex).FirstOrDefault();
		foreach (var dim in _dimensions)
		{
			dim.ZIndex = dim == currentDimension ? 100 : 0;
			dim.SetModulate(dim == currentDimension ? new Color(1, 1, 1) : new Color(1, 1, 1, 0));
		}
	}

	private void FadeToView(Dimension dimensionTargeted, float changingViewTweenDuration)
	{
		if (!_enabledNavigation || _isChangingView || dimensionTargeted == null)
			return;

		if (!_dimensions.Contains(dimensionTargeted))
		{
			GD.PushError("La dimension cible n'est pas dans la liste des dimensions.");
			return;
		}

		foreach (var dim in _dimensions)
				dim.Pause(true);
		_isChangingView = true;

		var currentDimension = _dimensions.Where(d => d._dimOrder == CurrentIndex).FirstOrDefault();
		GD.Print($"Fading Dimension from '{currentDimension.Name}' to '{dimensionTargeted.Name}'");

		// Applique le ZIndex uniquement aux deux dimensions concernées
		foreach (var dim in _dimensions)
			dim.ZIndex = dim == currentDimension || dim == dimensionTargeted ? 100 : 0;

		_tween = CreateTween();

		_tween.TweenProperty(dimensionTargeted, "modulate:a", 1.0f, changingViewTweenDuration);
		_tween.TweenProperty(currentDimension, "modulate:a", 0.0f, changingViewTweenDuration);

		_tween.TweenCallback(Callable.From(() =>
		{
			// Cacher toutes les dimensions sauf la cible
			foreach (var dim in _dimensions)
				if (dim != dimensionTargeted)
					dim.Modulate = new Color(1, 1, 1, 0);

			// Mise à jour manuelle de CurrentIndex pour qu’il corresponde à la nouvelle
			CurrentIndex = dimensionTargeted._dimOrder;
			SleepAllDimensionsAndTheirRulesExceptCurrent( );
			_isChangingView = false;
		}));
	}


	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventJoypadMotion btn && btn.GetAxisValue() > 0.8f)
		{
			if (btn.GetAxis() == JoyAxis.TriggerLeft)
				ChangeView(CurrentIndex - 1);
			else if (btn.GetAxis() == JoyAxis.TriggerRight)
				ChangeView(CurrentIndex + 1);
		}
		else if (@event is InputEventKey btnPc && btnPc.Pressed)
		{
			if (btnPc.Keycode == Key.Left)
				ChangeView(CurrentIndex - 1);
			else if (btnPc.Keycode == Key.Right)
				ChangeView(CurrentIndex + 1);
			else if (btnPc.Keycode == Key.F1)
				SetChangingDimensionMode(DimensionChangeMode == EnumDimensionSwitchingMode.Slide
					? EnumDimensionSwitchingMode.Fade
					: EnumDimensionSwitchingMode.Slide);
		}
	}

	//To commonly change the dimension call THIS method
	public void ChangeView(int targetIndex)
	{
		if (!_enabledNavigation || _isChangingView) return;
		if (!_dimensions.Where(d => d._dimOrder == targetIndex).Any()) return;
		if (CurrentIndex == targetIndex) return;
		UpdateLayoutGeneral();
		
		switch (DimensionChangeMode)
		{
			case EnumDimensionSwitchingMode.Slide:
				SlideToView(FindDimensionByOrder(targetIndex), _changingViewTweenDuration);
				break;
			case EnumDimensionSwitchingMode.Fade:
				FadeToView(FindDimensionByOrder(targetIndex), _changingViewTweenDuration);
				break;
		}
		
		
	}

	private  void SleepAllDimensionsAndTheirRulesExceptCurrent()
	{
		var currentDimension = FindDimensionByOrder(CurrentIndex);
		foreach (var dim in _dimensions)
			if(dim == currentDimension)
				dim.Pause(false);
			else
			{
				dim.Pause(true);
			}
		
			
		
	}
	
	public Dimension FindDimensionByOrder(int order)
	{
		return _dimensions.Where(d => d._dimOrder == order).FirstOrDefault();
	}

	public void EnableNavigation(bool b)
	{
		_enabledNavigation = b;
	}

	//backend//////////////////////////////////////////////////////////////////////////////////////////////////////////
	private void CreateDimensionsAndApplyAllLaws()
	{
		if (OrderLaws == null) return;
		if (TransdimensionalLaws == null) return;
		var messagealreadyexists = "Dimension already exists , cannot add it";
		for (var i = 0; i <= NumberOfDimensionToCreateInitially; i++)
			if (CreateNegativeAndPositiveInitially)
			{
				if (!AddNewDimensionIfNotAlreadyExists(-i))
					GD.Print(messagealreadyexists);
				if (!AddNewDimensionIfNotAlreadyExists(i))
					GD.Print(messagealreadyexists);
			}
			else
			{
				if (!AddNewDimensionIfNotAlreadyExists(i))
					GD.Print(messagealreadyexists);
			}

		_dimensions.Sort((a, b) => a._dimOrder.CompareTo(b._dimOrder));

		foreach (var law in TransdimensionalLaws)
		foreach (var dimension in _dimensions)
		foreach (var rule in law.Rules)
		{
			var ruleInited = rule.Duplicate() as DimensionRule;
			ruleInited.Init(dimension._subViewportRoot, rule.ApplyOnStart, rule.ApplyOnEnd,
				rule.ApplyPermanently);
			dimension.AddRuleIfNotAlreadyContained(ruleInited);
		}


		foreach (var law in OrderLaws)
		foreach (var dimension in _dimensions)
		foreach (var rule in law.Rules)
			if (law.DimensionOrder == dimension._dimOrder)
			{
				var ruleInited = rule.Duplicate() as DimensionRule;
				ruleInited.Init(dimension._subViewportRoot, rule.ApplyOnStart, rule.ApplyOnEnd,
					rule.ApplyPermanently);
				dimension.AddRuleIfNotAlreadyContained(ruleInited);
			}
	}


	public bool AddNewDimensionIfNotAlreadyExists(int ord)
	{
		if (_dimensions.Where(d => d._dimOrder == ord).Any()) return false;

		var newDimension = _dimensionScene.Instantiate() as Dimension;
		if (newDimension == null)
		{
			GD.Print("Erreur de casting de la dimension dans AddNewDimension");
			return false;
		}

		newDimension.Init(ord);
		_viewContainer.AddChild(newDimension);
		_dimensions.Add(newDimension);
		_dimensions.Sort((a, b) => a._dimOrder.CompareTo(b._dimOrder));
		return true;
	}
}

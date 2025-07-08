using Godot;

namespace Dim.Rules;

public abstract partial class DimensionRule : Resource
{
	protected  SubViewport _subViewportRoot;
	protected int _dimOrder;
	public bool _applyOnStart { get; set; } = false;
	public bool _applyOnEnd { get; set; } = false;
	public bool _applyPermanently { get; set; } = false;
	private float _permanentApplyingFrequency = 1;
	private Timer _timerPermanentApplyingFrequency = new();

	public void Init(SubViewport subViewportRoot , int dimOrder , bool applyOnStart = false , bool applyOnEnd = false  , bool applyPermanently = false , float permanentApplyingFrequency = 1)
	{
		_subViewportRoot = subViewportRoot;
		_dimOrder = dimOrder;
		_applyOnStart = applyOnStart;
		_applyOnEnd = applyOnEnd;
		_applyPermanently = applyPermanently;
		_permanentApplyingFrequency = permanentApplyingFrequency;
		//
		if(_applyOnStart)ApplyNowPonctually();
		if (_applyPermanently)
		{
			_timerPermanentApplyingFrequency.OneShot = false;
			_timerPermanentApplyingFrequency.Paused = false;
			_timerPermanentApplyingFrequency.WaitTime = _permanentApplyingFrequency;
			_timerPermanentApplyingFrequency.Start();
			// Méthode 1 : Utilisation de la syntaxe moderne
			_timerPermanentApplyingFrequency.Timeout += ApplyNowPonctually;
		}
	}
	
	public abstract void ApplyNowPonctually();


	
	public void ExitTree()
	{
		if(_applyOnEnd)ApplyNowPonctually();
	}

	public void Pause()
	{
		if (_applyPermanently)
			_timerPermanentApplyingFrequency.Paused = true;
	}

	public void Play()
	{
		if (_applyPermanently)
			_timerPermanentApplyingFrequency.Paused = false;
	}
	

	public void SetFrequency(float frequency)
	{
		if (_applyPermanently)
			_timerPermanentApplyingFrequency.WaitTime = frequency;
		else
		{
			GD.Print($"this rule is not permanent , cannot set its frequency");
		}
	}
}

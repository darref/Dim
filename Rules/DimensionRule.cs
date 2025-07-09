using Dim.Dimensions;
using Godot;

namespace Dim.Rules;

public abstract partial class DimensionRule : Resource
{
	protected Dimension _dimensionNode;
	protected  SubViewport _subViewportRoot;
	protected int _dimOrder;
	public bool _applyOnStart { get; set; } = false;
	public bool _applyOnEnd { get; set; } = false;
	public bool _applyPermanently { get; set; } = false;
	private float _permanentApplyingFrequency = 1;
	private Timer _timerPermanentApplyingFrequency = new();
	protected RuleCommonNodeMethodsHelper _helperNode ;

	public void Init(SubViewport subViewportRoot , int dimOrder , bool applyOnStart = false , bool applyOnEnd = false  , bool applyPermanently = false , float permanentApplyingFrequency = 1)
	{
		_subViewportRoot = subViewportRoot;
		_dimOrder = dimOrder;
		_applyOnStart = applyOnStart;
		_applyOnEnd = applyOnEnd;
		_applyPermanently = applyPermanently;
		_permanentApplyingFrequency = permanentApplyingFrequency;
		_dimensionNode = _subViewportRoot.GetParent() as Dimension;
		if (_dimensionNode == null)
		{
			GD.PushError("Impossible de récupérer la Dimension parente.");
			return;
		}
		//
		if (_helperNode == null)
		{
			_helperNode = new RuleCommonNodeMethodsHelper();
			_helperNode.OnReady = () =>
			{
				if(_applyOnStart)ApplyPonctually();
			};
			_helperNode.OnProcessFrame = (float delta) =>
			{
				if(_applyPermanently)ApplyPonctually();
			};
			_helperNode.OnExit = () =>
			{
				if(_applyOnEnd)ApplyPonctually();
			};
			_helperNode.ProcessMode = Node.ProcessModeEnum.Always;
			_helperNode.SetProcess(true);
			_subViewportRoot.GetParent<Dimension>().CallDeferred("add_child",_helperNode);
			//define common methods of Node to the helperNode because overrided in children classes ( must not be implemented)
			DefineCommonNodeMethods();
		}
	}
	
	public abstract void ApplyPonctually();
	public virtual void DefineCommonNodeMethods()
	{}

	public virtual void ValidationMessageConsole()
	{
		GD.Print($"Règle {GetClass()} bien appliquée à la dimension {_dimensionNode.Name}.");
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

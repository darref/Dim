using Dim.Dimensions;
using Godot;

namespace Dim.Rules;

public abstract partial class DimensionRule : Resource
{
    private float _elapsedSinceLastApply = 0f;
    private float _permanentApplyingFrequency = 0.05f;
    protected Dimension DimensionNode;
    protected int DimOrder;
    protected RuleCommonNodeMethodsHelper HelperNode;
    protected SubViewport SubViewportRoot;
    public bool ApplyOnStart { get; set; }
    public bool ApplyOnEnd { get; set; }
    public bool ApplyPermanently { get; set; }

    //internalmethods
    public void Init(SubViewport subViewportRoot, int dimOrder, bool applyOnStart = false, bool applyOnEnd = false,
        bool applyPermanently = false , float permanentApplyingFrequency = 0.05f)
    {
        SubViewportRoot = subViewportRoot;
        DimOrder = dimOrder;
        ApplyOnStart = applyOnStart;
        ApplyOnEnd = applyOnEnd;
        ApplyPermanently = applyPermanently;
        _permanentApplyingFrequency = permanentApplyingFrequency;
        DimensionNode = SubViewportRoot.GetParent() as Dimension;
        if (DimensionNode == null)
        {
            GD.PushError(GetType() + " Impossible de récupérer la Dimension parente.");
            return;
        }
        //
        if (HelperNode == null)
        {
            HelperNode = new RuleCommonNodeMethodsHelper();
            HelperNode.OnReady += () =>
            {
                if (SubViewportRoot == null || DimensionNode == null)
                {
                    RuleErrorMessage($"[HelperNodeReady] of {GetName()} : SubViewportRoot or DimensionNode is null");
                    return;
                }

                //
                if (ApplyOnStart) ApplyPonctually();
                //
                RuleValidationMessage($"[HelperNodeReady] of {GetName()} : Input executed.");
            };
            HelperNode.OnProcessFrame += delta =>
            {
                if (!ApplyPermanently)
                    return;

                if (SubViewportRoot == null || DimensionNode == null)
                {
                    RuleErrorMessage($"[HelperNodeProcess] of {GetName()} : SubViewportRoot or DimensionNode is null");
                    return;
                }

                _elapsedSinceLastApply += delta;

                if (_elapsedSinceLastApply >= _permanentApplyingFrequency)
                {
                    _elapsedSinceLastApply = 0f;
                    ApplyPonctually();
                    RuleValidationMessage($"[HelperNodeProcess] of {GetName()} : Process executed.");
                }
            };
            HelperNode.OnInput += e =>
            {
                if (SubViewportRoot == null || DimensionNode == null)
                    RuleErrorMessage($"[HelperNodeInput] of {GetName()} : SubViewportRoot or DimensionNode is null");
                //

                //
                RuleValidationMessage($"[HelperNodeInput] of {GetName()} : Input executed.");
            };
            HelperNode.OnExit += () =>
            {
                if (SubViewportRoot == null || DimensionNode == null)
                    RuleErrorMessage($"[HelperNodeExit] of {GetName()} : SubViewportRoot or DimensionNode is null");
                //
                if (ApplyPermanently) ApplyPonctually();
                //
                RuleValidationMessage($"[HelperNodeExit] of {GetName()} : Exit executed.");
            };
            HelperNode.SetProcessMode(Node.ProcessModeEnum.Pausable);
            HelperNode.SetProcess(true);
            SubViewportRoot.GetParent<Dimension>().CallDeferred("add_child", HelperNode);
            HelperNode.Name = "HelperFor" + GetType().Name;
            //define common methods of Node to the helperNode because overrided in children classes ( must not be implemented)
            DefineCommonHelperNodeMethods();
        }
    }

    protected abstract void ApplyPonctually();

    protected abstract void UnApplyPonctually();

    protected abstract void DefineCommonHelperNodeMethods();


    protected virtual void RuleErrorMessage(string s)
    {
        GD.PrintErr(s);
    }

    protected virtual void RuleValidationMessage(string s)
    {
        GD.Print(s);
    }

    //externalmethods
    public void Pause()
    {
        if (ApplyPermanently)
            HelperNode.SetProcess(false);
    }

    public void Play()
    {
        if (ApplyPermanently)
            HelperNode.SetProcess(true);
    }
    public void SetProcessFrequency(float frequency)
    {
        if (ApplyPermanently)
            _permanentApplyingFrequency = frequency;
        else
            GD.Print($"this rule is not permanent , cannot set its frequency");
    }

}
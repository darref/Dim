using Dim.Dimensions;
using Dim.Rules.Creators;
using Dim.Rules.SpecificRules;
using Dim.UI;
using Godot;

namespace Dim.Rules;

[GlobalClass]
public abstract partial class DimensionRule : Resource
{
    [Export] public bool ApplyOnStart { get; set; }
    [Export] public bool ApplyOnEnd { get; set; }
    [Export] public bool ApplyPermanently { get; set; }
    [Export] public float PermanentApplyingFrequency = 0.05f;
    private float _elapsedSinceLastApply = 0f;
    [Export] public DimensionsCreator DimensionsCreator;
    [Export] public RulesCreator RulesCreator;
    [Export] public bool Enabled = true;
    [Export] public bool ConsoleMessagesEnabled = false;
    //
    protected Dimension DimensionNodeRef;
    protected SubViewport SubViewportRootRef;
    protected DimensionsManager DimensionManagerRef;
    protected RuleCommonNodeMethodsHelper HelperNode;
    
    //internalmethods
    public void Init(SubViewport subViewportRoot, bool applyOnStart = false, bool applyOnEnd = false,
        bool applyPermanently = false, float permanentApplyingFrequency = 0.05f)
    {
        SubViewportRootRef = subViewportRoot;
        ApplyOnStart = applyOnStart;
        ApplyOnEnd = applyOnEnd;
        ApplyPermanently = applyPermanently;
        PermanentApplyingFrequency = permanentApplyingFrequency;
        DimensionNodeRef = SubViewportRootRef.GetParent() as Dimension;
        if (DimensionNodeRef == null)
        {
            GD.PushError(GetType() + " Impossible de récupérer la Dimension parente.");
            return;
        }
        DimensionManagerRef = DimensionNodeRef.GetParent().GetParent() as DimensionsManager;
        if (DimensionManagerRef == null)
        {
            GD.PushError(GetType() + " Impossible de récupérer le DimensionManager parent.");
            return;
        }
        //
        if (HelperNode == null)
        {
            HelperNode = new RuleCommonNodeMethodsHelper();
            HelperNode.OnReady += () =>
            {
                if (SubViewportRootRef == null || DimensionNodeRef == null)
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

                if (SubViewportRootRef == null || DimensionNodeRef == null)
                {
                    RuleErrorMessage($"[HelperNodeProcess] of {GetName()} : SubViewportRoot or DimensionNode is null");
                    return;
                }

                _elapsedSinceLastApply += delta;

                if (_elapsedSinceLastApply >= PermanentApplyingFrequency)
                {
                    _elapsedSinceLastApply = 0f;
                    ApplyPonctually();
                    RuleValidationMessage($"[HelperNodeProcess] of {GetName()} : Process executed.");
                }
            };
            HelperNode.OnInput += e =>
            {
                if (SubViewportRootRef == null || DimensionNodeRef == null)
                    RuleErrorMessage($"[HelperNodeInput] of {GetName()} : SubViewportRoot or DimensionNode is null");
                //

                //
                RuleValidationMessage($"[HelperNodeInput] of {GetName()} : Input executed.");
            };
            HelperNode.OnExit += () =>
            {
                if (SubViewportRootRef == null || DimensionNodeRef == null)
                    RuleErrorMessage($"[HelperNodeExit] of {GetName()} : SubViewportRoot or DimensionNode is null");
                //
                if (ApplyPermanently) ApplyPonctually();
                //
                RuleValidationMessage($"[HelperNodeExit] of {GetName()} : Exit executed.");
            };
            HelperNode.SetProcessMode(Node.ProcessModeEnum.Pausable);
            HelperNode.SetProcess(true);
            SubViewportRootRef.GetParent<Dimension>().CallDeferred("add_child", HelperNode);
            HelperNode.Name = "HelperFor" + GetType().Name;
            //define common methods of Node to the helperNode because overrided in children classes ( must not be implemented)
            AddCommonHelperNodeMethods();
        }
    }

    protected abstract void ApplyPonctually();

    protected abstract void UnApplyPonctually();

    protected abstract void AddCommonHelperNodeMethods();


    protected virtual void RuleErrorMessage(string s)
    {
        if(ConsoleMessagesEnabled)
            GD.PrintErr(s);
    }

    protected virtual void RuleValidationMessage(string s)
    {
        if(ConsoleMessagesEnabled)
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
            PermanentApplyingFrequency = frequency;
        else
            GD.Print($"this rule is not permanent , cannot set its frequency");
    }

}
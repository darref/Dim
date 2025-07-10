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
    public Dimension DimensionNodeRef;
    protected SubViewport SubViewportRootRef;
    protected DimensionsManager DimensionManagerRef;
    public RuleCommonNodeMethodsHelper HelperNode;
    
    //internalmethods
    public void Init(SubViewport subViewportRoot, bool applyOnStart = false, bool applyOnEnd = false,
        bool applyPermanently = false, float permanentApplyingFrequency = 0.05f )
    {
        SubViewportRootRef = subViewportRoot;
        ApplyOnStart = applyOnStart;
        ApplyOnEnd = applyOnEnd;
        ApplyPermanently = applyPermanently;
        PermanentApplyingFrequency = permanentApplyingFrequency;
        DimensionNodeRef = SubViewportRootRef.GetParent() as Dimension;
        DimensionManagerRef = DimensionNodeRef.GetParent().GetParent() as DimensionsManager;
        
        HelperNode = new RuleCommonNodeMethodsHelper();
        HelperNode.Init(this);
        HelperNode.Name = "HelperFor" + GetType().Name;
        AddCommonHelperNodeMethods();
        DimensionNodeRef.AddChild(HelperNode);

    }

    protected abstract void ApplyPonctually();

    protected abstract void UnApplyPonctually();

    protected abstract void AddCommonHelperNodeMethods();
    
    protected void RuleErrorMessage(string s)
    {
        if(ConsoleMessagesEnabled)
            GD.PrintErr(s);
    }

    protected void RuleValidationMessage(string s)
    {
        if(ConsoleMessagesEnabled)
            GD.Print(s);
    }

}
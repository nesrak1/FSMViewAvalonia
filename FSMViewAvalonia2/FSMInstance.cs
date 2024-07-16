
namespace FSMViewAvalonia2;

//not to be confused Structs, which holds the exact data
//from the assets files, these classes hold simplified
//data to be displayed on the canvas

public interface IActionScriptEntryUI : IActionScriptEntry
{
    void BuildView(StackPanel state, int index);
}
public class FsmStateData
{
    public string Name => state.name;
    public IReadOnlyList<IActionScriptEntry> ActionData { get; set; }
    public FsmState state;
    public FsmNodeData node;
    public bool isStartState;
    public FsmStateData(FsmState state)
    {
        this.state = state;
        node = new(state);
        isStartState = state.name == state.fsm.startState.name;
        ActionData = state.fsm.GetActionScriptEntry(state);
    }
}
public class FsmNodeData
{
    public bool isGlobal;
    public Rect transform;
    public Color stateColor;
    public Color transitionColor;
    public string name;
    public FsmTransition[] transitions;
    public FsmNodeData(FsmDataInstanceUI dataInst, FsmGlobalTransition transition)
    {
        isGlobal = true;
        FsmStateData toState = dataInst.states.FirstOrDefault(s => s.Name == transition.toState);
        if (toState != null)
        {
            FsmNodeData toNode = toState.node;
            transform = new Rect(toNode.transform.X, toNode.transform.Y - 50, toNode.transform.Width, 18);
        }
        else
        {
            transform = new Rect(-100, -100, 100, 18);
        }

        stateColor = Constants.STATE_COLORS[transition.colorIndex];
        transitionColor = Constants.TRANSITION_COLORS[transition.colorIndex];
        name = transition.fsmEvent.name;
        transitions =
        [
                new FsmTransition(transition)
        ];
    }
    public FsmNodeData(FsmState state)
    {
        isGlobal = false;
        transform = new Rect(state.position.x, state.position.y, state.position.width, state.position.height);
        stateColor = Constants.STATE_COLORS[state.colorIndex];
        transitionColor = Constants.TRANSITION_COLORS[state.colorIndex];
        name = state.name;
        transitions = state.transitions;
    }
}
//for switch tabs, todo
public class FsmUIInstance
{
    public Matrix matrix;
    public List<Control> states;
    public List<Control> events;
    public List<Control> variables;
    public List<Control> graphElements;
    public List<UINode> nodes;
    public int dataVersion;
}

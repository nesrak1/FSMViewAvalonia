
namespace FSMViewAvalonia2
{
    //not to be confused Structs, which holds the exact data
    //from the assets files, these classes hold simplified
    //data to be displayed on the canvas
    public class FsmDataInstance
    {
        public string fsmName;
        public string goName;
        public int tabIndex;
        public AssetInfo info;
        public List<FsmStateData> states;
        public List<FsmEventData> events;
        public List<FsmVariableData> variables;
        public HashSet<string> variableNames;
        public List<FsmNodeData> globalTransitions;
		public FsmStateData startState;
        public List<UINode> nodes;
        public Controls canvasControls;
        public Matrix matrix;
        public int dataVersion;
    }
    public class FsmStateData
    {
        public string Name { get { return state.name; } }
        public List<IActionScriptEntry> ActionData { get; set; }
        public FsmState state;
        public FsmNodeData node;
		public bool isStartState;
    }
    public class FsmEventData
    {
        public string Name { get; set; }
        public bool Global { get; set; }
    }
    public class FsmVariableData
    {
        public string Type { get; set; }
        public List<Tuple<string, object>> Values { get; set; }
    }
    public class FsmGlobalTransition
    {
        public FsmEvent fsmEvent;
        public string toState;
        public int linkStyle;
        public int linkConstraint;
        public byte colorIndex;
    }
    public interface IActionScriptEntry
    {
        string Name { get; set; }
        List<Tuple<string, object>> Values { get; set; }
        bool Enabled { get; set; }
        void BuildView(StackPanel state, int index);
    }
    public class FsmNodeData
    {
        public bool isGlobal;
        public Rect transform;
        public Color stateColor;
        public Color transitionColor;
        public string name;
        public FsmTransition[] transitions;
        public FsmNodeData(FsmDataInstance dataInst, FsmGlobalTransition transition)
        {
            isGlobal = true;
            FsmStateData toState = dataInst.states.FirstOrDefault(s => s.state.name == transition.toState);
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
            transitions = new FsmTransition[1]
            {
                new FsmTransition(transition)
            };
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
}

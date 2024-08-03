using FSMViewAvalonia2.Context;

namespace FSMViewAvalonia2;
public class FsmDataInstanceUI
{
    public FsmDataInstance fsm;
    public int tabIndex;
    public List<UINode> nodes;
    public Controls canvasControls;
    public Matrix matrix;
    public List<FsmStateData> states;
    public GameContext context;
    public FsmDataInstanceUI(FsmDataInstance fsm, GameContext ctx)
    {
        this.fsm = fsm;
        this.context = ctx;
        states = fsm.states.Select(x => new FsmStateData(x)).ToList();
    }
    //To prevent memory leak because Avalonia's bugs
    public void Detach()
    {
        fsm = null;
        nodes = null;
        canvasControls = null;
        states = null;
        context = null;
    }
}

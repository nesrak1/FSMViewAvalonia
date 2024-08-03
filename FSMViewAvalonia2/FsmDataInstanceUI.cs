namespace FSMViewAvalonia2;
public class FsmDataInstanceUI
{
    public FsmDataInstance fsm;
    public int tabIndex;
    public List<UINode> nodes;
    public Controls canvasControls;
    public Matrix matrix;
    public List<FsmStateData> states;
    public FsmDataInstanceUI(FsmDataInstance fsm)
    {
        this.fsm = fsm;

        states = fsm.states.Select(x => new FsmStateData(x)).ToList();
    }
    //To prevent memory leak because Avalonia's bugs
    public void Detach()
    {
        fsm = null;
        nodes = null;
        canvasControls = null;
        states = null;
    }
}

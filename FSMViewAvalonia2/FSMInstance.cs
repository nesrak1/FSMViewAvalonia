using AssetsTools.NET;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace FSMViewAvalonia2
{
    //not to be confused Structs, which holds the exact data
    //from the assets files, these classes hold simplified
    //data to be displayed on the canvas
    public class FsmDataInstance
    {
        public List<FsmStateData> states;
        public List<FsmEventData> events;
        public List<FsmVariableData> variables;
        public int dataVersion;
    }
    public class FsmStateData
    {
        public string Name { get { return state.name; } }
        public List<Tuple<string, string>> Values { get; set; }
        public FsmState state;
        public FsmNodeData node;
    }
    public class FsmEventData
    {
        public string Name { get; set; }
        public bool Global { get; set; }
    }
    public class FsmVariableData
    {
        public string Name { get; set; }
        public List<Tuple<string, string>> Values { get; set; }
    }
    public class FsmNodeData
    {
        public bool isGlobal;
        public Rect transform;
        public Color stateColor;
        public Color transitionColor;
        public string name;
        public FsmTransition[] transitions;
        public FsmNodeData(FsmState state, bool isGlobal)
        {
            this.isGlobal = isGlobal;
            transform = state.position;
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

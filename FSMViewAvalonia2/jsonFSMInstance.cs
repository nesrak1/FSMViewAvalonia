using System;
using System.Collections.Generic;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace FSMViewAvalonia2.json
{   
    public class ActionData
    {
        public int ActionCount { get; set; }
        public string[] ActionNames { get; set; }
    }

    public class FsmState
    {
        public string name { get; set; }
        public string description{ get; set; }
        public FsmTransition[] transitions { get; set; }
        public List<ActionScriptEntry> actionData { get; set; }
        public byte colorIndex { get; set; }
        public UnityRect position { get; set; }
        public bool isBreakpoint { get; set; }
        public bool isSequence { get; set; }
        public bool hideUnused { get; set; }

        public override string ToString()
        {
            return $"State {name}";
        }
    }
    public class FsmEvent
    {
        public string name { get; set; }
        public bool isSystemEvent { get; set; }
        public bool isGlobal { get; set; }

        public override string ToString()
        {
            return $"Event({name})";
        }
    }
    public class FsmTransition
    {
        public FsmEvent fsmEvent { get; set; }
        public string toState { get; set; }
        public int linkStyle { get; set; }
        public int linkConstraint { get; set; }
        public byte colorIndex { get; set; }

        public override string ToString()
        {
            return $"Transition({fsmEvent.name} -> {toState})";
        }
    }

    public class FsmDataInstance
    {
        public string fsmName { get; set; }
        public string goName { get; set; }
        public List<FsmState> states { get; set; } = new List<FsmState>();
        public List<FsmEvent> events { get; set; }  = new List<FsmEvent>();
        public List<FsmVariableData> variables { get; set; }  = new List<FsmVariableData>();
        public List<FsmNodeData> globalTransitions { get; set; } = new List<FsmNodeData>();
        public int dataVersion { get; set; }

    }
    public class FsmVariableData
    {
        public string Type { get; set; }
        public List<jTuple<string, object>> Values { get; set; }
    }
    public class FsmGlobalTransition
    {
        public FsmEvent fsmEvent { get; set; }
        public string toState { get; set; }
        public int linkStyle { get; set; }
        public int linkConstraint { get; set; }
        public byte colorIndex { get; set; }
    }

    public class jTuple<T,W>{
        public T Item1 { get; set; }
        public W Item2 { get; set; }
    }

    public class FsmParams
    {
        public object Value { get; set; }
        public string ObjectType { get; set; }
        public object RawValue { get; set; }
        public int VariableType { get; set; }
        public bool CastVariable { get; set; }
        public string Name { get; set; }
        public int TypeConstraint { get; set; }
        public string Tooltip { get; set; }
        public bool UseVariable { get; set; }
        public bool ShowInInspector { get; set; }
        public bool NetworkSync { get; set; }
        public bool IsNone { get; set; }
        public bool UsesVariable { get; set; }

        public override string ToString()
        {
            return $"({Name} -> {Value})";
        }
    }
    public class ActionScriptEntry
    {
        public string Name { get; set; }
        public List<jTuple<string, object>> Values { get; set; }

        public bool Enabled { get; set; }

    }
    public class FsmNodeData
    {
        public bool isGlobal { get; set; }
        public string name { get; set; }
        public FsmTransition[] transitions { get; set; }

        public Rect transform { get; set; }
        public Color stateColor { get; set; }
        public Color transitionColor { get; set; }

    }
}
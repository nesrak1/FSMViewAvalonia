using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMViewAvalonia2;
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
    private static void GetActionData(List<IActionScriptEntry> list, ActionData actionData, int dataVersion, FsmState state, FsmDataInstance inst)
    {
        for (int i = 0; i < actionData.actionNames.Count; i++)
        {
            list.Add(new FsmStateAction(actionData, i, dataVersion, state, inst));
        }
    }

    private static void GetVariableValues(List<FsmVariableData> varData, IDataProvider variables)
    {
        IDataProvider[] floatVariables = variables.Get<IDataProvider[]>("floatVariables");
        IDataProvider[] intVariables = variables.Get<IDataProvider[]>("intVariables");
        IDataProvider[] boolVariables = variables.Get<IDataProvider[]>("boolVariables");
        IDataProvider[] stringVariables = variables.Get<IDataProvider[]>("stringVariables");
        IDataProvider[] vector2Variables = variables.Get<IDataProvider[]>("vector2Variables");
        IDataProvider[] vector3Variables = variables.Get<IDataProvider[]>("vector3Variables");
        IDataProvider[] colorVariables = variables.Get<IDataProvider[]>("colorVariables");
        IDataProvider[] rectVariables = variables.Get<IDataProvider[]>("rectVariables");
        IDataProvider[] quaternionVariables = variables.Get<IDataProvider[]>("quaternionVariables");
        IDataProvider[] gameObjectVariables = variables.Get<IDataProvider[]>("gameObjectVariables");
        IDataProvider[] objectVariables = variables.Get<IDataProvider[]>("objectVariables");
        IDataProvider[] materialVariables = variables.Get<IDataProvider[]>("materialVariables");
        IDataProvider[] textureVariables = variables.Get<IDataProvider[]>("textureVariables");
        IDataProvider[] arrayVariables = variables.Get<IDataProvider[]>("arrayVariables");
        IDataProvider[] enumVariables = variables.Get<IDataProvider[]>("enumVariables");

        FsmVariableData enums = new() { VariableType = VariableType.Enum, Type = "Enums", Values = new List<Tuple<string, object>>() };
        varData.Add(enums);
        for (int i = 0; i < enumVariables.Length; i++)
        {
            string name = enumVariables[i].Get<string>("name");
            object value = enumVariables[i].Get<int>("value");
            enums.Values.Add(new Tuple<string, object>(name, value));
        }

        FsmVariableData arrays = new() { VariableType = VariableType.Array, Type = "Arrays", Values = new List<Tuple<string, object>>() };
        varData.Add(arrays);
        for (int i = 0; i < arrayVariables.Length; i++)
        {
            var arr = new FsmArray(arrayVariables[i]);
            arrays.Values.Add(new(arr.name, arr));
        }

        FsmVariableData floats = new() { VariableType = VariableType.Float, Type = "Floats", Values = new List<Tuple<string, object>>() };
        varData.Add(floats);
        for (int i = 0; i < floatVariables.Length; i++)
        {
            string name = floatVariables[i].Get<string>("name");
            object value = floatVariables[i].Get<float>("value");
            floats.Values.Add(new Tuple<string, object>(name, value));
        }

        FsmVariableData ints = new() { VariableType = VariableType.Int, Type = "Ints", Values = new List<Tuple<string, object>>() };
        varData.Add(ints);
        for (int i = 0; i < intVariables.Length; i++)
        {
            string name = intVariables[i].Get<string>("name");
            object value = intVariables[i].Get<int>("value");
            ints.Values.Add(new Tuple<string, object>(name, value));
        }

        FsmVariableData bools = new() { VariableType = VariableType.Bool, Type = "Bools", Values = new List<Tuple<string, object>>() };
        varData.Add(bools);
        for (int i = 0; i < boolVariables.Length; i++)
        {
            string name = boolVariables[i].Get<string>("name");
            object value = boolVariables[i].Get<bool>("value");
            bools.Values.Add(new Tuple<string, object>(name, value));
        }

        FsmVariableData strings = new() { VariableType = VariableType.String, Type = "Strings", Values = new List<Tuple<string, object>>() };
        varData.Add(strings);
        for (int i = 0; i < stringVariables.Length; i++)
        {
            string name = stringVariables[i].Get<string>("name");
            object value = stringVariables[i].Get<string>("value");
            strings.Values.Add(new Tuple<string, object>(name, value));
        }

        FsmVariableData vector2s = new() { VariableType = VariableType.Vector2, Type = "Vector2s", Values = new List<Tuple<string, object>>() };
        varData.Add(vector2s);
        for (int i = 0; i < vector2Variables.Length; i++)
        {
            string name = vector2Variables[i].Get<string>("name");
            IDataProvider vector2 = vector2Variables[i].Get<IDataProvider>("value");
            object value = new Vector2(vector2);
            vector2s.Values.Add(new Tuple<string, object>(name, value));
        }

        FsmVariableData vector3s = new() { VariableType = VariableType.Vector3, Type = "Vector3s", Values = new List<Tuple<string, object>>() };
        varData.Add(vector3s);
        for (int i = 0; i < vector3Variables.Length; i++)
        {
            string name = vector3Variables[i].Get<string>("name");
            IDataProvider vector3 = vector3Variables[i].Get<IDataProvider>("value");
            object value = new Vector2(vector3);
            vector3s.Values.Add(new Tuple<string, object>(name, value));
        }

        FsmVariableData colors = new() { VariableType = VariableType.Color, Type = "Colors", Values = new List<Tuple<string, object>>() };
        varData.Add(colors);
        for (int i = 0; i < colorVariables.Length; i++)
        {
            string name = colorVariables[i].Get<string>("name");
            IDataProvider color = colorVariables[i].Get<IDataProvider>("value");
            object value = new UnityColor(color);
            colors.Values.Add(new Tuple<string, object>(name, value));
        }

        FsmVariableData rects = new() { VariableType = VariableType.Rect, Type = "Rects", Values = new List<Tuple<string, object>>() };
        varData.Add(rects);
        for (int i = 0; i < rectVariables.Length; i++)
        {
            string name = rectVariables[i].Get<string>("name");
            IDataProvider rect = rectVariables[i].Get<IDataProvider>("value");
            object value = new UnityRect(rect);
            rects.Values.Add(new Tuple<string, object>(name, value));
        }

        FsmVariableData quaternions = new() { VariableType = VariableType.Quaternion, Type = "Quaternions", Values = new List<Tuple<string, object>>() };
        varData.Add(quaternions);
        for (int i = 0; i < quaternionVariables.Length; i++)
        {
            string name = quaternionVariables[i].Get<string>("name");
            IDataProvider quaternion = quaternionVariables[i].Get<IDataProvider>("value");
            object value = new Quaternion(quaternion);
            quaternions.Values.Add(new Tuple<string, object>(name, value));
        }

        string[] pptrTypeHeaders = new[] { "GameObjects", "Objects", "Materials", "Textures" };
        VariableType[] pptrTypeVarTypes = new[] { VariableType.GameObject, VariableType.Object, VariableType.Material, VariableType.Texture };
        IDataProvider[][] pptrTypeFields = new[] { gameObjectVariables, objectVariables, materialVariables, textureVariables };
        for (int j = 0; j < pptrTypeHeaders.Length; j++)
        {
            string header = pptrTypeHeaders[j];
            VariableType type = pptrTypeVarTypes[j];
            IDataProvider[] field = pptrTypeFields[j];

            if (field == null)
            {
                continue;
            }

            FsmVariableData genericData = new() { VariableType = type, Type = header, Values = new List<Tuple<string, object>>() };
            varData.Add(genericData);
            for (int i = 0; i < field.Length; i++)
            {
                string name = field[i].Get<string>("name");
                IDataProvider valueField = field[i].Get<IDataProvider>("value");
                INamedAssetProvider pptr = valueField?.As<INamedAssetProvider>();
                object value = pptr?.isNull ?? true ? "[null]" : header == "GameObjects" ? new GameObjectPPtrHolder() { pptr = pptr } : pptr;
                genericData.Values.Add(new Tuple<string, object>(name, value));
            }
        }
    }
    public void LoadFrom(AssetInfo assetInfo, IDataProvider fsm)
    {
        FsmDataInstance dataInstance = this;
        dataInstance.info = assetInfo;


        IDataProvider[] states = fsm.Get<IDataProvider[]>("states");
        IDataProvider[] events = fsm.Get<IDataProvider[]>("events");
        IDataProvider variables = fsm.Get<IDataProvider>("variables");
        IDataProvider[] globalTransitions = fsm.Get<IDataProvider[]>("globalTransitions");
        IDataProvider dataVersionField = fsm.Get<IDataProvider>("dataVersion");

        string startState = fsm.Get<string>("startState");

        dataInstance.fsmName = fsm.Get<string>("name");

        dataInstance.goName = assetInfo.nameBase;

        dataInstance.dataVersion = dataVersionField == null ? fsm.Get<int>("version") + 1 : dataVersionField.As<int>();

        dataInstance.events = new List<FsmEventData>();
        for (int i = 0; i < events.Length; i++)
        {
            FsmEventData eventData = new()
            {
                Global = events[i].Get<bool>("isGlobal"),
                Name = events[i].Get<string>("name")
            };

            dataInstance.events.Add(eventData);
        }

        dataInstance.variables = new List<FsmVariableData>();
        dataInstance.variableNames = new HashSet<string>();
        GetVariableValues(dataInstance.variables, variables);
        foreach (string v in dataInstance.variables.SelectMany(x => x.Values).Select(x => x.Item1))
        {
            _ = dataInstance.variableNames.Add(v);
        }

        dataInstance.states = new List<FsmStateData>();
        for (int i = 0; i < states.Length; i++)
        {
            FsmStateData stateData = new()
            {
                ActionData = new List<IActionScriptEntry>(),
                state = new FsmState(states[i], dataInstance)
            };
            stateData.node = new FsmNodeData(stateData.state);
            stateData.isStartState = stateData.Name == startState;

            if (stateData.isStartState)
            {
                dataInstance.startState = stateData;
            }

            GetActionData(stateData.ActionData, stateData.state.actionData, dataInstance.dataVersion, stateData.state, dataInstance);

            dataInstance.states.Add(stateData);
        }


        dataInstance.globalTransitions = new List<FsmNodeData>();
        for (int i = 0; i < globalTransitions.Length; i++)
        {
            IDataProvider globalTransitionField = globalTransitions[i];
            FsmGlobalTransition globalTransition = new()
            {
                fsmEvent = new FsmEvent(globalTransitionField.Get<IDataProvider>("fsmEvent")),
                toState = globalTransitionField.Get<string>("toState"),
                linkStyle = globalTransitionField.Get<int>("linkStyle"),
                linkConstraint = globalTransitionField.Get<int>("linkConstraint"),
                colorIndex = (byte) globalTransitionField.Get<int>("colorIndex")
            };

            FsmNodeData node = new(dataInstance, globalTransition);
            dataInstance.globalTransitions.Add(node);
        }
    }

    public FsmDataInstance() { }
    public FsmDataInstance(AssetInfo info, IDataProvider provider) : this() => LoadFrom(info, provider);
}

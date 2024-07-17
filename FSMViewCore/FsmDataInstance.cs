namespace FSMViewAvalonia2;
public class FsmDataInstance
{
    public string fsmName;
    public string goName;
    public int tabIndex;
    public AssetInfo info;
    public List<FsmState> states;
    public List<FsmEventData> events;
    public List<FsmVariableData> variables;
    public HashSet<string> variableNames;
    public List<FsmGlobalTransition> globalTransitions;
    public FsmState startState;
    public int dataVersion;

    public IReadOnlyList<IActionScriptEntry> GetActionScriptEntry(FsmState state)
    {
        var actionData = state.actionData;
        if (actionData.actionCache is not null) return actionData.actionCache;
        var result = new List<IActionScriptEntry>();
        for (int i = 0; i < actionData.actionNames.Count; i++)
        {
            result.Add(new FsmStateAction(actionData, i, dataVersion, state, this));
        }

        return actionData.actionCache = result;
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

        FsmVariableData enums = new() { VariableType = VariableType.Enum, Type = "Enums", Values = [] };
        varData.Add(enums);
        for (int i = 0; i < enumVariables.Length; i++)
        {
            string name = enumVariables[i].Get<string>("name");
            object value = enumVariables[i].Get<int>("value");
            enums.Values.Add(new(name, value));
        }

        FsmVariableData arrays = new() { VariableType = VariableType.Array, Type = "Arrays", Values = [] };
        varData.Add(arrays);
        for (int i = 0; i < arrayVariables.Length; i++)
        {
            var arr = new FsmArray(arrayVariables[i]);
            arrays.Values.Add(new(arr.name, arr));
        }

        FsmVariableData floats = new() { VariableType = VariableType.Float, Type = "Floats", Values = [] };
        varData.Add(floats);
        for (int i = 0; i < floatVariables.Length; i++)
        {
            string name = floatVariables[i].Get<string>("name");
            object value = floatVariables[i].Get<float>("value");
            floats.Values.Add(new(name, value));
        }

        FsmVariableData ints = new() { VariableType = VariableType.Int, Type = "Ints", Values = [] };
        varData.Add(ints);
        for (int i = 0; i < intVariables.Length; i++)
        {
            string name = intVariables[i].Get<string>("name");
            object value = intVariables[i].Get<int>("value");
            ints.Values.Add(new(name, value));
        }

        FsmVariableData bools = new() { VariableType = VariableType.Bool, Type = "Bools", Values = [] };
        varData.Add(bools);
        for (int i = 0; i < boolVariables.Length; i++)
        {
            string name = boolVariables[i].Get<string>("name");
            object value = boolVariables[i].Get<bool>("value");
            bools.Values.Add(new(name, value));
        }

        FsmVariableData strings = new() { VariableType = VariableType.String, Type = "Strings", Values = [] };
        varData.Add(strings);
        for (int i = 0; i < stringVariables.Length; i++)
        {
            string name = stringVariables[i].Get<string>("name");
            object value = stringVariables[i].Get<string>("value");
            strings.Values.Add(new(name, value));
        }

        FsmVariableData vector2s = new() { VariableType = VariableType.Vector2, Type = "Vector2s", Values = [] };
        varData.Add(vector2s);
        for (int i = 0; i < vector2Variables.Length; i++)
        {
            string name = vector2Variables[i].Get<string>("name");
            IDataProvider vector2 = vector2Variables[i].Get<IDataProvider>("value");
            object value = new Vector2(vector2);
            vector2s.Values.Add(new(name, value));
        }

        FsmVariableData vector3s = new() { VariableType = VariableType.Vector3, Type = "Vector3s", Values = [] };
        varData.Add(vector3s);
        for (int i = 0; i < vector3Variables.Length; i++)
        {
            string name = vector3Variables[i].Get<string>("name");
            IDataProvider vector3 = vector3Variables[i].Get<IDataProvider>("value");
            object value = new Vector2(vector3);
            vector3s.Values.Add(new(name, value));
        }

        FsmVariableData colors = new() { VariableType = VariableType.Color, Type = "Colors", Values = [] };
        varData.Add(colors);
        for (int i = 0; i < colorVariables.Length; i++)
        {
            string name = colorVariables[i].Get<string>("name");
            IDataProvider color = colorVariables[i].Get<IDataProvider>("value");
            object value = new UnityColor(color);
            colors.Values.Add(new(name, value));
        }

        FsmVariableData rects = new() { VariableType = VariableType.Rect, Type = "Rects", Values = [] };
        varData.Add(rects);
        for (int i = 0; i < rectVariables.Length; i++)
        {
            string name = rectVariables[i].Get<string>("name");
            IDataProvider rect = rectVariables[i].Get<IDataProvider>("value");
            object value = new UnityRect(rect);
            rects.Values.Add(new(name, value));
        }

        FsmVariableData quaternions = new() { VariableType = VariableType.Quaternion, Type = "Quaternions", Values = [] };
        varData.Add(quaternions);
        for (int i = 0; i < quaternionVariables.Length; i++)
        {
            string name = quaternionVariables[i].Get<string>("name");
            IDataProvider quaternion = quaternionVariables[i].Get<IDataProvider>("value");
            object value = new Quaternion(quaternion);
            quaternions.Values.Add(new(name, value));
        }

        string[] pptrTypeHeaders = ["GameObjects", "Objects", "Materials", "Textures"];
        VariableType[] pptrTypeVarTypes = [VariableType.GameObject, VariableType.Object, VariableType.Material, VariableType.Texture];
        IDataProvider[][] pptrTypeFields = [gameObjectVariables, objectVariables, materialVariables, textureVariables];
        for (int j = 0; j < pptrTypeHeaders.Length; j++)
        {
            string header = pptrTypeHeaders[j];
            VariableType type = pptrTypeVarTypes[j];
            IDataProvider[] field = pptrTypeFields[j];

            if (field == null)
            {
                continue;
            }

            FsmVariableData genericData = new() { VariableType = type, Type = header, Values = [] };
            varData.Add(genericData);
            for (int i = 0; i < field.Length; i++)
            {
                string name = field[i].Get<string>("name");
                IDataProvider valueField = field[i].Get<IDataProvider>("value");
                INamedAssetProvider pptr = valueField?.As<INamedAssetProvider>();
                object value = pptr?.isNull ?? true ? "[null]" : header == "GameObjects" ? new GameObjectPPtrHolder() { pptr = pptr } : pptr;
                genericData.Values.Add(new(name, pptr, value));
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

        dataInstance.events = [];
        for (int i = 0; i < events.Length; i++)
        {
            FsmEventData eventData = new()
            {
                Global = events[i].Get<bool>("isGlobal"),
                Name = events[i].Get<string>("name")
            };

            dataInstance.events.Add(eventData);
        }

        dataInstance.variables = [];
        dataInstance.variableNames = [];
        GetVariableValues(dataInstance.variables, variables);
        foreach (string v in dataInstance.variables.SelectMany(x => x.Values).Select(x => x.Name))
        {
            _ = dataInstance.variableNames.Add(v);
        }

        dataInstance.states = [];
        for (int i = 0; i < states.Length; i++)
        {
            var state = new FsmState(states[i], dataInstance);

            if (state.name == startState)
            {
                dataInstance.startState = state;
            }

            dataInstance.states.Add(state);
        }


        dataInstance.globalTransitions = [];
        for (int i = 0; i < globalTransitions.Length; i++)
        {
            IDataProvider globalTransitionField = globalTransitions[i];
            FsmGlobalTransition globalTransition = new()
            {
                fsmEvent = new FsmEvent(globalTransitionField.Get<IDataProvider>("fsmEvent")),
                toState = globalTransitionField.Get<string>("toState"),
                linkStyle = globalTransitionField.Get<int>("linkStyle"),
                linkConstraint = globalTransitionField.Get<int>("linkConstraint"),
                colorIndex = (byte)globalTransitionField.Get<int>("colorIndex")
            };

            dataInstance.globalTransitions.Add(globalTransition);
        }
    }

    public FsmDataInstance() { }
    public FsmDataInstance(AssetInfo info, IDataProvider provider) : this() => LoadFrom(info, provider);
}

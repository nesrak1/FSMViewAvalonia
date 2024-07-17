namespace FSMViewAvalonia2;
public class FsmStateAction : IActionScriptEntry
{

    public FsmStateAction(ActionData actionData, int index, int dataVersion, FsmState state,
        FsmDataInstance dataInstance)
    {
        string actionName = actionData.actionNames[index];
        FsmData = dataInstance;
        FullName = actionName;
        Type = AssemblyProvider?.GetType(FullName);
        if (actionName.Contains('.'))
        {
            actionName = actionName[(actionName.LastIndexOf(".") + 1)..];
        }

        int startIndex = actionData.actionStartIndex[index];
        int endIndex = index == actionData.actionNames.Count - 1 ? actionData.paramDataType.Count : actionData.actionStartIndex[index + 1];

        HashSet<string> parmaNames = [];
        for (int j = startIndex; j < endIndex; j++)
        {
            string paramName = actionData.paramName[j];
            if (string.IsNullOrEmpty(paramName))
            {
                break;
            }

            object obj = ActionReader.GetFsmObject(actionData, AssemblyProvider, ref j, dataVersion);
            var raw = obj;
            string displayValue = "";
            string sectionName = null;
            UIHint? uitype = null;
            FieldDefinition field = Type?.Fields?.FirstOrDefault(x => x.Name == paramName);

            if (obj is NamedVariable nv)
            {
                if (!string.IsNullOrEmpty(nv.name))
                {
                    if (!dataInstance.variableNames.Contains(nv.name))
                    {
                        nv.isGlobal = true;
                    }
                }
            }

            if (field != null)
            {
                TypeDefinition ftype = field.FieldType.Resolve();
                CustomAttribute UIHintAttr = field.CustomAttributes
                        .FirstOrDefault(x => x.AttributeType.FullName == "HutongGames.PlayMaker.UIHintAttribute");
                uitype = (UIHint?)(int?)UIHintAttr?.ConstructorArguments[0].Value;

                CustomAttribute ActionSectionAttr = field.CustomAttributes
                        .FirstOrDefault(x => x.AttributeType.FullName == "HutongGames.PlayMaker.ActionSection");
                sectionName = (string)ActionSectionAttr?.ConstructorArguments[0].Value;

                if (ftype.IsEnum && obj is int val)
                {
                    displayValue = Utils.GetFsmEnumString(ftype, val);
                }

            }

            Values.Add(new(paramName, raw, displayValue, uitype, sectionName));
        }

        State = state;
        Name = actionName;
        Enabled = actionData.actionEnabled[index];
    }
    public string FullName { get; set; }
    public string Name { get; set; }
    public List<IActionScriptEntry.PropertyInfo> Values { get; set; } = [];
    public bool Enabled { get; set; } = true;

    public FsmState State { get; init; }
    public TypeDefinition Type { get; }
    public FsmDataInstance FsmData { get; }
    public IAssemblyProvider AssemblyProvider => FsmData.info.assemblyProvider;

}

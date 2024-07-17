

using System.Collections;
using System.Text;

namespace FSMViewAvalonia2;


public static class StructUtil
{

    public static List<T> ReadAssetList<T>(IDataProvider[] field)
    {
        List<T> data = [];
        if (field == null)
        {
            return data;
        }

        int size = field.Length;
        data.Capacity = size;
        switch (typeof(T))
        {
            case Type intType when intType == typeof(int):
                for (int i = 0; i < size; i++)
                {
                    data.Add((T)(object)field[i].As<int>());
                }

                return data;
            case Type stringType when stringType == typeof(string):
                for (int i = 0; i < size; i++)
                {
                    data.Add((T)(object)field[i].As<string>());
                }

                return data;
            case Type boolType when boolType == typeof(bool):
                for (int i = 0; i < size; i++)
                {
                    data.Add((T)(object)field[i].As<bool>());
                }

                return data;
            case Type byteType when byteType == typeof(byte):
                for (int i = 0; i < size; i++)
                {
                    data.Add((T)(object)(byte)field[i].As<int>());
                }

                return data;
            case Type enumType when enumType.IsEnum:
                for (int i = 0; i < size; i++)
                {
                    data.Add((T)(object)field[i].As<int>());
                }

                return data;
            case Type namedPPtrType when namedPPtrType == typeof(INamedAssetProvider):
                for (int i = 0; i < size; i++)
                {
                    data.Add((T)(object)field[i].As<INamedAssetProvider>());
                }

                return data;
            default:
                //no error checking so don't put something stupid for <T>
                for (int i = 0; i < size; i++)
                {
                    data.Add((T)Activator.CreateInstance(typeof(T), field[i]));
                }

                return data;
        }
    }
}

public class FsmEventData
{
    public string Name { get; set; }
    public bool Global { get; set; }
}
public class FsmVariableData
{
    public string Type { get; set; }
    public VariableType VariableType { get; set; }
    public class ValueTuple(string name, object raw = null, object val = null)
    {
        public string Name { get; set; } = name;
        public object RawValue { get; set; } = raw;
        public object Value { get; set; } = val ?? raw;
    }
    public List<ValueTuple> Values { get; set; }
}
public class FsmGlobalTransition
{
    public FsmEvent fsmEvent;
    public string toState;
    public int linkStyle;
    public int linkConstraint;
    public byte colorIndex;
}
public class FsmState
{
    public string name;
    public string description;
    public byte colorIndex;
    public UnityRect position;
    public bool isBreakpoint;
    public bool isSequence;
    public bool hideUnused;
    public FsmTransition[] transitions;
    public ActionData actionData;
    public FsmDataInstance fsm;
    public FsmState(IDataProvider provider, FsmDataInstance fsm = null)
    {
        name = provider.GetValue<string>("name");
        description = provider.GetValue<string>("description");
        colorIndex = (byte)provider.GetValue<uint>("colorIndex");
        position = new UnityRect(provider.GetValue<IDataProvider>("position"));
        isBreakpoint = provider.GetValue<bool>("isBreakpoint");
        isSequence = provider.GetValue<bool>("isSequence");
        hideUnused = provider.GetValue<bool>("hideUnused");
        IDataProvider[] transitionsData = provider.GetValue<IDataProvider[]>("transitions");
        transitions = new FsmTransition[transitionsData.Length];
        for (int i = 0; i < transitions.Length; i++)
        {
            transitions[i] = new FsmTransition(transitionsData[i]);
        }

        actionData = new ActionData(provider.GetValue<IDataProvider>("actionData"));
        this.fsm = fsm;

    }
    public override string ToString() => $"State {name}";
}

public class ActionData(IDataProvider field)
{
    internal IReadOnlyList<IActionScriptEntry> actionCache;
    public List<string> actionNames = StructUtil.ReadAssetList<string>(field.Get<IDataProvider[]>("actionNames"));
    public List<string> customNames = StructUtil.ReadAssetList<string>(field.Get<IDataProvider[]>("customNames"));
    public List<bool> actionEnabled = [.. field.Get<bool[]>("actionEnabled")];
    public List<bool> actionIsOpen = [.. field.Get<bool[]>("actionIsOpen")];
    public List<int> actionStartIndex = [.. field.Get<int[]>("actionStartIndex")];
    public List<int> actionHashCodes = [.. field.Get<int[]>("actionHashCodes")];
    public List<INamedAssetProvider> unityObjectParams = StructUtil.ReadAssetList<INamedAssetProvider>(field.Get<IDataProvider[]>("unityObjectParams"));
    public List<FsmGameObject> fsmGameObjectParams = StructUtil.ReadAssetList<FsmGameObject>(field.Get<IDataProvider[]>("fsmGameObjectParams"));
    public List<FsmOwnerDefault> fsmOwnerDefaultParams = StructUtil.ReadAssetList<FsmOwnerDefault>(field.Get<IDataProvider[]>("fsmOwnerDefaultParams"));
    public List<FsmAnimationCurve> animationCurveParams = StructUtil.ReadAssetList<FsmAnimationCurve>(field.Get<IDataProvider[]>("animationCurveParams"));
    public List<FsmFunctionCall> functionCallParams = StructUtil.ReadAssetList<FsmFunctionCall>(field.Get<IDataProvider[]>("functionCallParams"));
    public List<FsmTemplateControl> fsmTemplateControlParams = StructUtil.ReadAssetList<FsmTemplateControl>(field.Get<IDataProvider[]>("fsmTemplateControlParams"));
    public List<FsmEventTarget> fsmEventTargetParams = StructUtil.ReadAssetList<FsmEventTarget>(field.Get<IDataProvider[]>("fsmEventTargetParams"));
    public List<FsmProperty> fsmPropertyParams = StructUtil.ReadAssetList<FsmProperty>(field.Get<IDataProvider[]>("fsmPropertyParams"));
    public List<FsmLayoutOption> layoutOptionParams = StructUtil.ReadAssetList<FsmLayoutOption>(field.Get<IDataProvider[]>("layoutOptionParams"));
    public List<FsmString> fsmStringParams = StructUtil.ReadAssetList<FsmString>(field.Get<IDataProvider[]>("fsmStringParams"));
    public List<FsmObject> fsmObjectParams = StructUtil.ReadAssetList<FsmObject>(field.Get<IDataProvider[]>("fsmObjectParams"));
    public List<FsmVar> fsmVarParams = StructUtil.ReadAssetList<FsmVar>(field.Get<IDataProvider[]>("fsmVarParams"));
    public List<FsmArray> fsmArrayParams = StructUtil.ReadAssetList<FsmArray>(field.Get<IDataProvider[]>("fsmArrayParams"));
    public List<FsmEnum> fsmEnumParams = StructUtil.ReadAssetList<FsmEnum>(field.Get<IDataProvider[]>("fsmEnumParams"));
    public List<FsmFloat> fsmFloatParams = StructUtil.ReadAssetList<FsmFloat>(field.Get<IDataProvider[]>("fsmFloatParams"));
    public List<FsmInt> fsmIntParams = StructUtil.ReadAssetList<FsmInt>(field.Get<IDataProvider[]>("fsmIntParams"));
    public List<FsmBool> fsmBoolParams = StructUtil.ReadAssetList<FsmBool>(field.Get<IDataProvider[]>("fsmBoolParams"));
    public List<FsmVector2> fsmVector2Params = StructUtil.ReadAssetList<FsmVector2>(field.Get<IDataProvider[]>("fsmVector2Params"));
    public List<FsmVector3> fsmVector3Params = StructUtil.ReadAssetList<FsmVector3>(field.Get<IDataProvider[]>("fsmVector3Params"));
    public List<FsmColor> fsmColorParams = StructUtil.ReadAssetList<FsmColor>(field.Get<IDataProvider[]>("fsmColorParams"));
    public List<FsmRect> fsmRectParams = StructUtil.ReadAssetList<FsmRect>(field.Get<IDataProvider[]>("fsmRectParams"));
    public List<FsmQuaternion> fsmQuaternionParams = StructUtil.ReadAssetList<FsmQuaternion>(field.Get<IDataProvider[]>("fsmQuaternionParams"));
    public List<string> stringParams = StructUtil.ReadAssetList<string>(field.Get<IDataProvider[]>("stringParams"));
    public List<byte> byteData = [.. field.Get<byte[]>("byteData")];
    public List<int> arrayParamSizes = [.. field.Get<int[]>("arrayParamSizes")];
    public List<string> arrayParamTypes = StructUtil.ReadAssetList<string>(field.Get<IDataProvider[]>("arrayParamTypes"));
    public List<int> customTypeSizes = StructUtil.ReadAssetList<int>(field.Get<IDataProvider[]>("customTypeSizes"));
    public List<string> customTypeNames = StructUtil.ReadAssetList<string>(field.Get<IDataProvider[]>("customTypeNames"));
    public List<ParamDataType> paramDataType = StructUtil.ReadAssetList<ParamDataType>(field.Get<IDataProvider[]>("paramDataType"));
    public List<string> paramName = StructUtil.ReadAssetList<string>(field.Get<IDataProvider[]>("paramName"));
    public List<int> paramDataPos = StructUtil.ReadAssetList<int>(field.Get<IDataProvider[]>("paramDataPos"));
    public List<int> paramByteDataSize = StructUtil.ReadAssetList<int>(field.Get<IDataProvider[]>("paramByteDataSize"));
}

public class FsmGameObject : NamedVariable
{
    public INamedAssetProvider value;
    public FsmGameObject() { }
    public FsmGameObject(IDataProvider field) : base(field)
    {
        value = field.Get<INamedAssetProvider>("value");
        if (name == "")
        {
            name = value?.name ?? "";
        }
    }
    public override string MyToString()
    {
        string namepart = string.IsNullOrEmpty(name) ? "" : $"GameObject {name}";
        string valuepart = string.IsNullOrEmpty(value?.name) ? "" : $"[{value}]";
        return namepart == "" || name == value?.name ? valuepart : valuepart == "" ? namepart : $"{valuepart}";
    }
}

public class FsmOwnerDefault
{
    public OwnerDefaultOption ownerOption;
    public FsmGameObject gameObject;
    public FsmOwnerDefault() { }
    public FsmOwnerDefault(IDataProvider field)
    {
        ownerOption = (OwnerDefaultOption)field.Get<int>("ownerOption");
        gameObject = new FsmGameObject(field.Get<IDataProvider>("gameObject"));
    }
    public override string ToString() => $"OwnerDefault {ToStringWithNoHead()}";
    public string ToStringWithNoHead() => ownerOption == OwnerDefaultOption.UseOwner ? "FSM Owner" : $"{gameObject.name ?? gameObject.ToString()}";
}

public class FsmAnimationCurve
{
    public AnimationCurve value;
    public FsmAnimationCurve() { }
    public FsmAnimationCurve(IDataProvider field) => value = new AnimationCurve(field);
    public override string ToString() => "AnimationCurve";
}

public class FsmFunctionCall
{
    public string FunctionName;
    public string parameterType;
    public FsmBool BoolParameter;
    public FsmFloat FloatParameter;
    public FsmInt IntParameter;
    public FsmGameObject GameObjectParameter;
    public FsmObject ObjectParameter;
    public FsmString StringParameter;
    public FsmVector2 Vector2Parameter;
    public FsmVector3 Vector3Parameter;
    public FsmRect RectParamater;
    public FsmQuaternion QuaternionParameter;
    public FsmObject MaterialParameter;
    public FsmObject TextureParameter;
    public FsmColor ColorParameter;
    public FsmEnum EnumParameter;
    public FsmArray ArrayParameter;
    public FsmFunctionCall() { }
    public FsmFunctionCall(IDataProvider field)
    {
        FunctionName = field.Get<string>("FunctionName");
        parameterType = field.Get<string>("parameterType");
        BoolParameter = new FsmBool(field.Get<IDataProvider>("BoolParameter"));
        FloatParameter = new FsmFloat(field.Get<IDataProvider>("FloatParameter"));
        IntParameter = new FsmInt(field.Get<IDataProvider>("IntParameter"));
        GameObjectParameter = new FsmGameObject(field.Get<IDataProvider>("GameObjectParameter"));
        ObjectParameter = new FsmObject(field.Get<IDataProvider>("ObjectParameter"));
        StringParameter = new FsmString(field.Get<IDataProvider>("StringParameter"));
        Vector2Parameter = new FsmVector2(field.Get<IDataProvider>("Vector2Parameter"));
        Vector3Parameter = new FsmVector3(field.Get<IDataProvider>("Vector3Parameter"));
        RectParamater = new FsmRect(field.Get<IDataProvider>("RectParamater"));
        QuaternionParameter = new FsmQuaternion(field.Get<IDataProvider>("QuaternionParameter"));
        MaterialParameter = new FsmObject(field.Get<IDataProvider>("MaterialParameter"));
        TextureParameter = new FsmObject(field.Get<IDataProvider>("TextureParameter"));
        ColorParameter = new FsmColor(field.Get<IDataProvider>("ColorParameter"));
        EnumParameter = new FsmEnum(field.Get<IDataProvider>("EnumParameter"));
        ArrayParameter = new FsmArray(field.Get<IDataProvider>("ArrayParameter"));
    }
    public override string ToString()
    {
        NamedVariable value = null;
        switch (parameterType)
        {
            case "bool":
                value = BoolParameter;
                break;
            case "float":
                value = FloatParameter;
                break;
            case "int":
                value = IntParameter;
                break;
            case "GameObject":
                value = GameObjectParameter;
                break;
            case "Object":
                value = ObjectParameter;
                break;
            case "string":
                value = StringParameter;
                break;
            case "Vector2":
                value = Vector2Parameter;
                break;
            case "Vector3":
                value = Vector3Parameter;
                break;
            case "Rect":
                value = RectParamater;
                break;
            case "Quaternion":
                value = QuaternionParameter;
                break;
            case "Material":
                value = MaterialParameter;
                break;
            case "Texture":
                value = TextureParameter;
                break;
            case "Color":
                value = ColorParameter;
                break;
            case "Enum":
                value = EnumParameter;
                break;
            case "Array":
                value = ArrayParameter;
                break;
        }

        return value != null
            ? value.name == "" ? $"{FunctionName}({value})" : $"{FunctionName}({value.name}={value})"
            : $"{FunctionName}(???)";
    }
}

public class FsmTemplateControl
{
    public INamedAssetProvider fsmTemplate;
    public FsmVarOverride[] fsmVarOverrides;
    public FsmTemplateControl() { }
    public FsmTemplateControl(IDataProvider field)
    {
        //fsmTemplate = StructUtil.ReadAssetPPtr(field.Get("fsmTemplate"));
        //fsmVarOverrides = field.Get<IDataProvider[]>("fsmVarOverrides").Select(i => new FsmVarOverride(i)).ToArray();
    }
    public override string ToString() => "TemplateControl";
}

public class FsmEventTarget
{
    public EventTarget target;
    public FsmBool excludeSelf;
    public FsmOwnerDefault gameObject;
    public FsmString fsmName;
    public FsmBool sendToChildren;
    public INamedAssetProvider fsmComponent;
    public FsmEventTarget() { }
    public FsmEventTarget(IDataProvider field)
    {
        target = (EventTarget)field.Get<int>("target");
        excludeSelf = new FsmBool(field.Get<IDataProvider>("excludeSelf"));
        gameObject = new FsmOwnerDefault(field.Get<IDataProvider>("gameObject"));
        fsmName = new FsmString(field.Get<IDataProvider>("fsmName"));
        sendToChildren = new FsmBool(field.Get<IDataProvider>("sendToChildren"));
        //fsmComponent = StructUtil.ReadNamedAssetPPtr(field.Get("fsmComponent"));
    }
    public override string ToString()
    {
        StringBuilder flags = new();
        if (excludeSelf.value)
        {
            _ = flags.Append("[ExcludeSelf]");
        }

        if (sendToChildren.value)
        {
            _ = flags.Append("[SendToChildren]");
        }

        if (!string.IsNullOrEmpty(fsmName.value))
        {
            _ = flags.Append($"[SendToFSM: {fsmName.value}]");
        }

        return $"EventTarget({target}){flags}:{gameObject.ToStringWithNoHead()}";
    }
}

public class FsmProperty
{
    public FsmObject TargetObject;
    public string TargetTypeName;
    public string PropertyName;
    public FsmBool BoolParameter;
    public FsmFloat FloatParameter;
    public FsmInt IntParameter;
    public FsmGameObject GameObjectParameter;
    public FsmString StringParameter;
    public FsmVector2 Vector2Parameter;
    public FsmVector3 Vector3Parameter;
    public FsmRect RectParamater;
    public FsmQuaternion QuaternionParameter;
    public FsmObject ObjectParameter;
    public FsmObject MaterialParameter;
    public FsmObject TextureParameter;
    public FsmColor ColorParameter;
    public FsmEnum EnumParameter;
    public FsmArray ArrayParameter;
    public bool setProperty;
    public FsmProperty() { }
    public FsmProperty(IDataProvider field)
    {
        TargetObject = new FsmObject(field.Get<IDataProvider>("TargetObject"));
        TargetTypeName = field.Get<string>("TargetTypeName");
        PropertyName = field.Get<string>("PropertyName");
        BoolParameter = new FsmBool(field.Get<IDataProvider>("BoolParameter"));
        FloatParameter = new FsmFloat(field.Get<IDataProvider>("FloatParameter"));
        IntParameter = new FsmInt(field.Get<IDataProvider>("IntParameter"));
        GameObjectParameter = new FsmGameObject(field.Get<IDataProvider>("GameObjectParameter"));
        ObjectParameter = new FsmObject(field.Get<IDataProvider>("ObjectParameter"));
        StringParameter = new FsmString(field.Get<IDataProvider>("StringParameter"));
        Vector2Parameter = new FsmVector2(field.Get<IDataProvider>("Vector2Parameter"));
        Vector3Parameter = new FsmVector3(field.Get<IDataProvider>("Vector3Parameter"));
        RectParamater = new FsmRect(field.Get<IDataProvider>("RectParamater"));
        QuaternionParameter = new FsmQuaternion(field.Get<IDataProvider>("QuaternionParameter"));
        MaterialParameter = new FsmObject(field.Get<IDataProvider>("MaterialParameter"));
        TextureParameter = new FsmObject(field.Get<IDataProvider>("TextureParameter"));
        ColorParameter = new FsmColor(field.Get<IDataProvider>("ColorParameter"));
        EnumParameter = new FsmEnum(field.Get<IDataProvider>("EnumParameter"));
        ArrayParameter = new FsmArray(field.Get<IDataProvider>("ArrayParameter"));
        setProperty = field.Get<bool>("setProperty");
    }
    public override string ToString() => $"Property {{{TargetObject}}}.{PropertyName}";
}

public class FsmLayoutOption : NamedVariable
{
    public LayoutOptionType option;
    public FsmFloat floatParam;
    public FsmBool boolParam;
    public FsmLayoutOption() { }
    public FsmLayoutOption(IDataProvider field) : base(field)
    {
        option = (LayoutOptionType)field.Get<int>("option");
        floatParam = new FsmFloat(field.Get<IDataProvider>("floatParam"));
        boolParam = new FsmBool(field.Get<IDataProvider>("boolParam"));
    }
    public override string MyToString() => name != "" ? $"LayoutOption {name} = {option}" : $"LayoutOption.{option}";
}

public class FsmString : NamedVariable
{
    public string value;
    public FsmString() { }
    public FsmString(IDataProvider field) : base(field) => value = field.Get<string>("value");
    public override string MyToString() => name != "" ? value == "" ? $"string {name}" : $"string {name} = \"{value}\"" : $"\"{value}\"";
}

public class FsmObject : NamedVariable
{
    public INamedAssetProvider value;
    public FsmObject() { }
    public FsmObject(IDataProvider field) : base(field) => value = field.Get<INamedAssetProvider>("value");
    public override string MyToString() => name != "" ? string.IsNullOrEmpty(value?.name) ? $"object {name}" : $"object {name} = [{value}]" : $"[{value}]";
}

public class FsmVar
{
    public string variableName;
    public string objectType;
    public bool useVariable;
    public VariableType type;
    public float floatValue;
    public int intValue;
    public bool boolValue;
    public string stringValue;
    public Vector4 vector4Value;
    public INamedAssetProvider objectReference;
    public FsmArray arrayValue;
    public FsmVar() { }
    public FsmVar(IDataProvider field)
    {
        variableName = field.Get<string>("variableName");
        objectType = field.Get<string>("objectType");
        useVariable = field.Get<bool>("useVariable");
        type = (VariableType)field.Get<int>("type");
        floatValue = field.Get<float>("floatValue");
        intValue = field.Get<int>("intValue");
        boolValue = field.Get<bool>("boolValue");
        stringValue = field.Get<string>("stringValue");
        vector4Value = new Vector4(field.Get<IDataProvider>("vector4Value"));
        objectReference = field.Get<INamedAssetProvider>("objectReference");
        arrayValue = new FsmArray(field.Get<IDataProvider>("arrayValue"));
    }
    public override string ToString()
    {
        object value = null;
        switch (type)
        {
            case VariableType.Float:
                value = floatValue;
                break;
            case VariableType.Int:
                value = intValue;
                break;
            case VariableType.Bool:
                value = boolValue;
                break;
            case VariableType.String:
                value = stringValue;
                break;
            case VariableType.Vector2:
                value = new Vector2() { x = vector4Value.x, y = vector4Value.y };
                break;
            case VariableType.Vector3:
                value = new Vector3() { x = vector4Value.x, y = vector4Value.y, z = vector4Value.z };
                break;
            case VariableType.Object:
            case VariableType.GameObject:
            case VariableType.Material:
            case VariableType.Texture:
                value = objectReference;
                break;
            case VariableType.Color:
                value = new UnityColor() { r = vector4Value.x, g = vector4Value.y, b = vector4Value.z, a = vector4Value.w };
                break;
            case VariableType.Rect:
                value = new UnityRect() { x = vector4Value.x, y = vector4Value.y, width = vector4Value.z, height = vector4Value.w };
                break;
            case VariableType.Quaternion:
                value = new Quaternion() { x = vector4Value.x, y = vector4Value.y, z = vector4Value.z, w = vector4Value.w };
                break;
            case VariableType.Array:
                value = arrayValue;
                break;
        }

        if (value != null)
        {
            object valueObj = value is INamedAssetProvider namedPPtr && namedPPtr.name != "" ? namedPPtr.name : value;

            return variableName != "" ? $"Var {variableName} = {valueObj}" : $"Var unnamed = {valueObj}";
        }
        else
        {
            return variableName != "" ? $"Var {variableName}" : $"Var";
        }
    }
}
public class FsmArray2
{
    public string type;
    public object[] array;
}
public class FsmArray : NamedVariable, IEnumerable<object>
{
    public VariableType type;
    public string objectTypeName;
    public float[] floatValues;
    public int[] intValues;
    public bool[] boolValues;
    public string[] stringValues;
    public Vector4[] vector4Values;
    public FsmArray() { }
    public FsmArray(IDataProvider field) : base(field)
    {
        type = (VariableType)field.Get<int>("type");
        objectTypeName = field.Get<string>("objectTypeName");
        floatValues = field.Get<IDataProvider[]>("floatValues").Select(x => x.As<float>()).ToArray();
        intValues = field.Get<IDataProvider[]>("intValues").Select(x => x.As<int>()).ToArray();
        boolValues = field.Get<IDataProvider[]>("boolValues").Select(x => x.As<bool>()).ToArray();
        stringValues = field.Get<IDataProvider[]>("stringValues").Select(x => x.As<string>()).ToArray();
        vector4Values = field.Get<IDataProvider[]>("vector4Values").Select(x => new Vector4(x)).ToArray();
    }
    public IEnumerator<object> GetEnumerator() => floatValues.OfType<object>()
        .Concat(intValues.OfType<object>())
        .Concat(boolValues.OfType<object>())
        .Concat(vector4Values.OfType<object>())
        .Concat(stringValues).GetEnumerator();
    public override string MyToString() => name != "" ? $"Array {name}" : $"Array {type} {objectTypeName}";

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class FsmFloat : NamedVariable
{
    public float value;
    public FsmFloat() { }
    public FsmFloat(IDataProvider field) : base(field) => value = field.Get<float>("value");
    public override string MyToString() => name != "" ? $"float {name}" : value + "f";
}

public class FsmInt : NamedVariable
{
    public int value;
    public FsmInt() { }
    public FsmInt(IDataProvider field) : base(field) => value = field.Get<int>("value");
    public override string MyToString() => name != "" ? $"int {name}" : value.ToString();
}

public class FsmBool : NamedVariable
{
    public bool value;
    public FsmBool() { }
    public FsmBool(IDataProvider field) : base(field) => value = field.Get<bool>("value");
    public override string MyToString() => name != "" ? $"bool {name}" : value.ToString().ToLower();
}

public class FsmVector2 : NamedVariable
{
    public Vector2 value;
    public FsmVector2() { }
    public FsmVector2(IDataProvider field) : base(field) => value = new Vector2(field.Get<IDataProvider>("value"));
    public override string MyToString() => name != "" ? $"Vector2 {name}" : value.ToString();
}

public class FsmVector3 : NamedVariable
{
    public Vector3 value;
    public FsmVector3() { }
    public FsmVector3(IDataProvider field) : base(field) => value = new Vector3(field.Get<IDataProvider>("value"));
    public override string MyToString() => name != "" ? $"Vector3 {name}" : value.ToString();
}

public class FsmColor : NamedVariable
{
    public UnityColor value;
    public FsmColor() { }
    public FsmColor(IDataProvider field) : base(field) => value = new UnityColor(field.Get<IDataProvider>("value"));
    public override string MyToString() => name != "" ? $"Color {name}" : value.ToString();
}

public class FsmRect : NamedVariable
{
    public UnityRect value;
    public FsmRect() { }
    public FsmRect(IDataProvider field) : base(field) => value = new UnityRect(field.Get<IDataProvider>("value"));
    public override string MyToString() => name != "" ? $"Rect {name}" : value.ToString();
}

public class FsmQuaternion : NamedVariable
{
    public Quaternion value;
    public FsmQuaternion() { }
    public FsmQuaternion(IDataProvider field) : base(field) => value = new Quaternion(field.Get<IDataProvider>("value"));
    public override string MyToString() => name != "" ? $"Quaternion {name}" : value.ToString();
}

/////////////////////////////

public class NamedVariable
{
    public bool useVariable;
    public string name;
    public string tooltip;
    public bool showInInspector;
    public bool networkSync;
    public bool isGlobal;
    public NamedVariable() { }
    public NamedVariable(IDataProvider field)
    {
        useVariable = field.Get<bool>("useVariable");
        name = field.Get<string>("name");
        tooltip = field.Get<string>("tooltip");
        showInInspector = field.Get<bool>("showInInspector");
        networkSync = field.Get<bool>("networkSync");
    }
    public virtual string MyToString() => base.ToString();
    public sealed override string ToString() => (isGlobal && !string.IsNullOrWhiteSpace(name)) ? "[Global] " + MyToString() : MyToString();
}

public class AnimationCurve
{
    public List<Keyframe> m_Curve;
    public int m_PreInfinity;
    public int m_PostInfinity;
    public int m_RotationOrder;
    public AnimationCurve() { }
    public AnimationCurve(IDataProvider field)
    {
        m_Curve = StructUtil.ReadAssetList<Keyframe>(field.Get<IDataProvider[]>("m_Curve"));
        m_PreInfinity = field.Get<int>("m_PreInfinity");
        m_PostInfinity = field.Get<int>("m_PostInfinity");
        m_RotationOrder = field.Get<int>("m_RotationOrder");
    }
}

public class Keyframe
{
    public float time;
    public float value;
    public float inSlope;
    public float outSlope;
    //new in 2019
    public int weightedMode;
    public float inWeight;
    public float outWeight;
    /////////////
    public Keyframe() { }
    public Keyframe(IDataProvider field)
    {
        time = field.Get<float>("time");
        value = field.Get<float>("value");
        inSlope = field.Get<float>("inSlope");
        outSlope = field.Get<float>("outSlope");
    }
}

public class Vector2
{
    public float x;
    public float y;
    public Vector2() { }
    public Vector2(IDataProvider field)
    {
        x = field.Get<float>("x");
        y = field.Get<float>("y");
    }
    public override string ToString() => $"Vector2({x}, {y})";
}

public class Vector3
{
    public float x;
    public float y;
    public float z;
    public Vector3() { }
    public Vector3(IDataProvider field)
    {
        x = field.Get<float>("x");
        y = field.Get<float>("y");
        z = field.Get<float>("z");
    }
    public override string ToString() => $"Vector3({x}, {y}, {z})";
}

public class Vector4
{
    public float x;
    public float y;
    public float z;
    public float w;
    public Vector4() { }
    public Vector4(IDataProvider field)
    {
        x = field.Get<float>("x");
        y = field.Get<float>("y");
        z = field.Get<float>("z");
        w = field.Get<float>("w");
    }
    public override string ToString() => $"Vector4({x}, {y}, {z}, {w})";
}

public class Quaternion
{
    public float x;
    public float y;
    public float z;
    public float w;
    public Quaternion() { }
    public Quaternion(IDataProvider field)
    {
        x = field.Get<float>("x");
        y = field.Get<float>("y");
        z = field.Get<float>("z");
        w = field.Get<float>("w");
    }
    public override string ToString() => $"Quaternion({x}, {y}, {z}, {w})";
}

public class UnityColor
{
    public float r;
    public float g;
    public float b;
    public float a;
    public UnityColor() { }
    public UnityColor(IDataProvider field)
    {
        r = field.Get<float>("r");
        g = field.Get<float>("g");
        b = field.Get<float>("b");
        a = field.Get<float>("a");
    }
    public override string ToString() => $"Color({r}, {g}, {b}, {a})";
}

public class UnityRect
{
    public float x;
    public float y;
    public float width;
    public float height;
    public UnityRect() { }
    public UnityRect(IDataProvider field)
    {
        x = field.Get<float>("x");
        y = field.Get<float>("y");
        width = field.Get<float>("width");
        height = field.Get<float>("height");
    }
    public override string ToString() => $"Rect({x}, {y}, {width}, {height})";
}

public class FsmEnum : NamedVariable
{
    public string enumName;
    public int intValue;
    public FsmEnum() { }
    public FsmEnum(IDataProvider field) : base(field)
    {
        enumName = field.Get<string>("enumName");
        intValue = field.Get<int>("intValue");
    }
    public override string MyToString() => name != "" ? $"Enum {name}" : $"Enum({enumName}, {intValue})";
}

public class FsmVarOverride
{
    public NamedVariable variable;
    public FsmVar fsmVar;
    public bool isEdited;
    public FsmVarOverride() { }
    public FsmVarOverride(IDataProvider field)
    {
        variable = new NamedVariable(field.Get<IDataProvider>("variable"));
        fsmVar = new FsmVar(field.Get<IDataProvider>("fsmVar"));
        isEdited = field.Get<bool>("isEdited");
    }
    public override string ToString() => $"VarOverride({variable.name})";
}

public class FsmTransition
{
    public FsmEvent fsmEvent;
    public string toState;
    public int linkStyle;
    public int linkConstraint;
    public byte colorIndex;
    public FsmTransition() { }
    public FsmTransition(FsmGlobalTransition globalTransition)
    {
        fsmEvent = null;
        toState = globalTransition.toState;
        linkStyle = globalTransition.linkStyle;
        linkConstraint = globalTransition.linkConstraint;
        colorIndex = globalTransition.colorIndex;
    }
    public FsmTransition(IDataProvider valueField)
    {
        fsmEvent = new FsmEvent(valueField.Get<IDataProvider>("fsmEvent"));
        toState = valueField.Get<string>("toState");
        linkStyle = valueField.Get<int>("linkStyle");
        linkConstraint = valueField.Get<int>("linkConstraint");
        colorIndex = (byte)valueField.Get<int>("colorIndex");
    }
    public override string ToString() => $"Transition({fsmEvent.name} -> {toState})";
}

public class FsmEvent
{
    public string name;
    public bool isSystemEvent;
    public bool isGlobal;
    public FsmEvent() { }
    public FsmEvent(IDataProvider valueField)
    {
        name = valueField.Get<string>("name");
        isSystemEvent = valueField.Get<bool>("isSystemEvent");
        isGlobal = valueField.Get<bool>("isGlobal");
    }
    public override string ToString() => $"Event({name})";
}

public enum OwnerDefaultOption
{
    UseOwner,
    SpecifyGameObject
}

public enum LayoutOptionType
{
    Width,
    Height,
    MinWidth,
    MaxWidth,
    MinHeight,
    MaxHeight,
    ExpandWidth,
    ExpandHeight
}

public enum ParamDataType
{
    Integer,
    Boolean,
    Float,
    String,
    Color,
    ObjectReference,
    LayerMask,
    Enum,
    Vector2,
    Vector3,
    Vector4,
    Rect,
    Array,
    Character,
    AnimationCurve,
    FsmFloat,
    FsmInt,
    FsmBool,
    FsmString,
    FsmGameObject,
    FsmOwnerDefault,
    FunctionCall,
    FsmAnimationCurve,
    FsmEvent,
    FsmObject,
    FsmColor,
    Unsupported,
    GameObject,
    FsmVector3,
    LayoutOption,
    FsmRect,
    FsmEventTarget,
    FsmMaterial,
    FsmTexture,
    Quaternion,
    FsmQuaternion,
    FsmProperty,
    FsmVector2,
    FsmTemplateControl,
    FsmVar,
    CustomClass,
    FsmArray,
    FsmEnum
}

public enum EventTarget
{
    Self,
    GameObject,
    GameObjectFSM,
    FSMComponent,
    BroadcastAll,
    HostFSM,
    SubFSMs
}

public enum VariableType
{
    Unknown = -1,
    Float,
    Int,
    Bool,
    GameObject,
    String,
    Vector2,
    Vector3,
    Color,
    Rect,
    Material,
    Texture,
    Quaternion,
    Object,
    Array,
    Enum
}

public enum UIHint
{
    None,
    TextArea,
    Behaviour,
    Script,
    Method,
    Coroutine,
    Animation,
    Tag,
    Layer,
    Description,
    Variable,
    ScriptComponent,
    Comment,
    NamedColor,
    NamedTexture,
    FsmName,
    FsmEvent,
    FsmFloat,
    FsmInt,
    FsmBool,
    FsmString,
    FsmVector3,
    FsmGameObject,
    FsmColor,
    FsmRect,
    FsmMaterial,
    FsmTexture,
    FsmQuaternion,
    FsmObject,
    FsmVector2,
    FsmEnum,
    FsmArray,
    AnimatorFloat,
    AnimatorBool,
    AnimatorInt,
    AnimatorTrigger,
    SortingLayer,
    TagMenu
}

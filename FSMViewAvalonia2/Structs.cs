

namespace FSMViewAvalonia2
{
    public static class Constants
    {
        public static readonly Color[] STATE_COLORS =
        {
            Color.FromRgb(128, 128, 128),
            Color.FromRgb(116, 143, 201),
            Color.FromRgb(58, 182, 166),
            Color.FromRgb(93, 164, 53),
            Color.FromRgb(225, 254, 50),
            Color.FromRgb(235, 131, 46),
            Color.FromRgb(187, 75, 75),
            Color.FromRgb(117, 53, 164)
        };

        public static readonly Color[] TRANSITION_COLORS =
        {
            Color.FromRgb(222, 222, 222),
            Color.FromRgb(197, 213, 248),
            Color.FromRgb(159, 225, 216),
            Color.FromRgb(183, 225, 159),
            Color.FromRgb(225, 254, 102),
            Color.FromRgb(255, 198, 152),
            Color.FromRgb(225, 159, 160),
            Color.FromRgb(197, 159, 225)
        };
    }

    public static class StructUtil
    {
        
        public static List<T> ReadAssetList<T>(IDataProvider[] field)
        {
            List<T> data = new();
            int size = field.Length;
            switch (typeof(T))
            {
                case Type intType when intType == typeof(int):
                    for (int i = 0; i < size; i++)
                        data.Add((T)(object)field[i].As<int>());
                    return data;
                case Type stringType when stringType == typeof(string):
                    for (int i = 0; i < size; i++)
                        data.Add((T)(object)field[i].As<string>());
                    return data;
                case Type boolType when boolType == typeof(bool):
                    for (int i = 0; i < size; i++)
                        data.Add((T)(object)field[i].As<bool>());
                    return data;
                case Type byteType when byteType == typeof(byte):
                    for (int i = 0; i < size; i++)
                        data.Add((T)(object)(byte)field[i].As<int>());
                    return data;
                case Type enumType when enumType.IsEnum:
                    for (int i = 0; i < size; i++)
                        data.Add((T)(object)field[i].As<int>());
                    return data;
                case Type namedPPtrType when namedPPtrType == typeof(INamedAssetProvider):
                    for (int i = 0; i < size; i++)
                        data.Add((T)(object)field[i].As<INamedAssetProvider>());
                    return data;
                default:
                    //no error checking so don't put something stupid for <T>
                    for (int i = 0; i < size; i++)
                        data.Add((T)Activator.CreateInstance(typeof(T), field[i]));
                    return data;
            }
        }
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
            var transitionsData = provider.GetValue<IDataProvider[]>("transitions");
            transitions = new FsmTransition[transitionsData.Length];
            for (int i = 0; i < transitions.Length; i++)
            {
                transitions[i] = new FsmTransition(transitionsData[i]);
            }
            actionData = new ActionData(provider.GetValue<IDataProvider>("actionData"));
            this.fsm = fsm;

        }
        public override string ToString()
        {
            return $"State {name}";
        }
    }

    public class ActionData
    {
        public List<string> actionNames;
        public List<string> customNames;
        public List<bool> actionEnabled;
        public List<bool> actionIsOpen;
        public List<int> actionStartIndex;
        public List<int> actionHashCodes;
        public List<INamedAssetProvider> unityObjectParams;
        public List<FsmGameObject> fsmGameObjectParams;
        public List<FsmOwnerDefault> fsmOwnerDefaultParams;
        public List<FsmAnimationCurve> animationCurveParams;
        public List<FsmFunctionCall> functionCallParams;
        public List<FsmTemplateControl> fsmTemplateControlParams;
        public List<FsmEventTarget> fsmEventTargetParams;
        public List<FsmProperty> fsmPropertyParams;
        public List<FsmLayoutOption> layoutOptionParams;
        public List<FsmString> fsmStringParams;
        public List<FsmObject> fsmObjectParams;
        public List<FsmVar> fsmVarParams;
        public List<FsmArray> fsmArrayParams;
        public List<FsmEnum> fsmEnumParams;
        public List<FsmFloat> fsmFloatParams;
        public List<FsmInt> fsmIntParams;
        public List<FsmBool> fsmBoolParams;
        public List<FsmVector2> fsmVector2Params;
        public List<FsmVector3> fsmVector3Params;
        public List<FsmColor> fsmColorParams;
        public List<FsmRect> fsmRectParams;
        public List<FsmQuaternion> fsmQuaternionParams;
        public List<string> stringParams;
        public List<byte> byteData;
        public List<int> arrayParamSizes;
        public List<string> arrayParamTypes;
        public List<int> customTypeSizes;
        public List<string> customTypeNames;
        public List<ParamDataType> paramDataType;
        public List<string> paramName;
        public List<int> paramDataPos;
        public List<int> paramByteDataSize;
        public ActionData(IDataProvider field)
        {
            actionNames = StructUtil.ReadAssetList<string>(field.Get<IDataProvider[]>("actionNames"));
            customNames = StructUtil.ReadAssetList<string>(field.Get<IDataProvider[]>("customNames"));
            actionEnabled = StructUtil.ReadAssetList<bool>(field.Get<IDataProvider[]>("actionEnabled"));
            actionIsOpen = StructUtil.ReadAssetList<bool>(field.Get<IDataProvider[]>("actionIsOpen"));
            actionStartIndex = StructUtil.ReadAssetList<int>(field.Get<IDataProvider[]>("actionStartIndex"));
            actionHashCodes = StructUtil.ReadAssetList<int>(field.Get<IDataProvider[]>("actionHashCodes"));
            unityObjectParams = StructUtil.ReadAssetList<INamedAssetProvider>(field.Get<IDataProvider[]>("unityObjectParams"));
            fsmGameObjectParams = StructUtil.ReadAssetList<FsmGameObject>(field.Get<IDataProvider[]>("fsmGameObjectParams"));
            fsmOwnerDefaultParams = StructUtil.ReadAssetList<FsmOwnerDefault>(field.Get<IDataProvider[]>("fsmOwnerDefaultParams"));
            animationCurveParams = StructUtil.ReadAssetList<FsmAnimationCurve>(field.Get<IDataProvider[]>("animationCurveParams"));
            functionCallParams = StructUtil.ReadAssetList<FsmFunctionCall>(field.Get<IDataProvider[]>("functionCallParams"));
            fsmTemplateControlParams = StructUtil.ReadAssetList<FsmTemplateControl>(field.Get<IDataProvider[]>("fsmTemplateControlParams"));
            fsmEventTargetParams = StructUtil.ReadAssetList<FsmEventTarget>(field.Get<IDataProvider[]>("fsmEventTargetParams"));
            fsmPropertyParams = StructUtil.ReadAssetList<FsmProperty>(field.Get<IDataProvider[]>("fsmPropertyParams"));
            layoutOptionParams = StructUtil.ReadAssetList<FsmLayoutOption>(field.Get<IDataProvider[]>("layoutOptionParams"));
            fsmStringParams = StructUtil.ReadAssetList<FsmString>(field.Get<IDataProvider[]>("fsmStringParams"));
            fsmObjectParams = StructUtil.ReadAssetList<FsmObject>(field.Get<IDataProvider[]>("fsmObjectParams"));
            fsmVarParams = StructUtil.ReadAssetList<FsmVar>(field.Get<IDataProvider[]>("fsmVarParams"));
            fsmArrayParams = StructUtil.ReadAssetList<FsmArray>(field.Get<IDataProvider[]>("fsmArrayParams"));
            fsmEnumParams = StructUtil.ReadAssetList<FsmEnum>(field.Get<IDataProvider[]>("fsmEnumParams"));
            fsmFloatParams = StructUtil.ReadAssetList<FsmFloat>(field.Get<IDataProvider[]>("fsmFloatParams"));
            fsmIntParams = StructUtil.ReadAssetList<FsmInt>(field.Get<IDataProvider[]>("fsmIntParams"));
            fsmBoolParams = StructUtil.ReadAssetList<FsmBool>(field.Get<IDataProvider[]>("fsmBoolParams"));
            fsmVector2Params = StructUtil.ReadAssetList<FsmVector2>(field.Get<IDataProvider[]>("fsmVector2Params"));
            fsmVector3Params = StructUtil.ReadAssetList<FsmVector3>(field.Get<IDataProvider[]>("fsmVector3Params"));
            fsmColorParams = StructUtil.ReadAssetList<FsmColor>(field.Get<IDataProvider[]>("fsmColorParams"));
            fsmRectParams = StructUtil.ReadAssetList<FsmRect>(field.Get<IDataProvider[]>("fsmRectParams"));
            fsmQuaternionParams = StructUtil.ReadAssetList<FsmQuaternion>(field.Get<IDataProvider[]>("fsmQuaternionParams"));
            stringParams = StructUtil.ReadAssetList<string>(field.Get<IDataProvider[]>("stringParams"));
            byteData = StructUtil.ReadAssetList<byte>(field.Get<IDataProvider[]>("byteData"));
            arrayParamSizes = StructUtil.ReadAssetList<int>(field.Get<IDataProvider[]>("arrayParamSizes"));
            arrayParamTypes = StructUtil.ReadAssetList<string>(field.Get<IDataProvider[]>("arrayParamTypes"));
            customTypeSizes = StructUtil.ReadAssetList<int>(field.Get<IDataProvider[]>("customTypeSizes"));
            customTypeNames = StructUtil.ReadAssetList<string>(field.Get<IDataProvider[]>("customTypeNames"));
            paramDataType = StructUtil.ReadAssetList<ParamDataType>(field.Get<IDataProvider[]>("paramDataType"));
            paramName = StructUtil.ReadAssetList<string>(field.Get<IDataProvider[]>("paramName"));
            paramDataPos = StructUtil.ReadAssetList<int>(field.Get<IDataProvider[]>("paramDataPos"));
            paramByteDataSize = StructUtil.ReadAssetList<int>(field.Get<IDataProvider[]>("paramByteDataSize"));
        }
    }

    public class FsmGameObject : NamedVariable
    {
        public INamedAssetProvider value;
        public FsmGameObject() { }
        public FsmGameObject(IDataProvider field) : base(field)
        {
            value = field.Get<INamedAssetProvider>("value");
            if (name == "")
                name = value?.name ?? "";
        }
        public override string MyToString()
        {
            var namepart = string.IsNullOrEmpty(name) ? "" : $"GameObject {name}";
            var valuepart = string.IsNullOrEmpty(value?.name) ? "" : $"[{value}]";
            if (namepart == "" || name == value?.name) return valuepart;
            if(valuepart == "") return namepart;
            return $"{valuepart}";
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
        public override string ToString()
        {
            return $"OwnerDefault {ToStringWithNoHead()}";
        }
        public string ToStringWithNoHead()
        {
            if (ownerOption == OwnerDefaultOption.UseOwner)
                return "FSM Owner";
            else
                return $"{gameObject.name ?? gameObject.ToString()}";
        }
    }

    public class FsmAnimationCurve
    {
        public AnimationCurve value;
        public FsmAnimationCurve() { }
        public FsmAnimationCurve(IDataProvider field)
        {
            value = new AnimationCurve(field);
        }
        public override string ToString()
        {
            return "AnimationCurve";
        }
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
            if (value != null)
            {
                if (value.name == "")
                    return $"{FunctionName}({value})";
                else
                    return $"{FunctionName}({value.name}={value})";
            }
            else
            {
                return $"{FunctionName}(???)";
            }
        }
    }

    public class FsmTemplateControl
    {
        public AssetPPtr fsmTemplate;
        public FsmVarOverride[] fsmVarOverrides;
        public FsmTemplateControl() { }
        public FsmTemplateControl(IDataProvider field)
        {
            //fsmTemplate = StructUtil.ReadAssetPPtr(field.Get("fsmTemplate"));
            fsmVarOverrides = field.Get<IDataProvider[]>("fsmVarOverrides").Select(i => new FsmVarOverride(i)).ToArray();
        }
        public override string ToString()
        {
            return "TemplateControl";
        }
    }

    public class FsmEventTarget
    {
        public EventTarget target;
        public FsmBool excludeSelf;
        public FsmOwnerDefault gameObject;
        public FsmString fsmName;
        public FsmBool sendToChildren;
        public AssetPPtr fsmComponent;
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
            if(excludeSelf.value)
            {
                flags.Append("[ExcludeSelf]");
            }
            if(sendToChildren.value)
            {
                flags.Append("[SendToChildren]");
            }
            if (!string.IsNullOrEmpty(fsmName.value))
            {
                flags.Append($"[SendToFSM: {fsmName.value}]");
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
        public override string ToString()
        {
            return $"Property {TargetTypeName} {PropertyName}";
        }
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
        public override string MyToString()
        {
            if (name != "")
                return $"LayoutOption {name} = {option}";
            else
                return $"LayoutOption.{option}";
        }
    }

    public class FsmString : NamedVariable
    {
        public string value;
        public FsmString() { }
        public FsmString(IDataProvider field) : base(field)
        {
            value = field.Get<string>("value");
        }
        public override string MyToString()
        {
            if (name != "")
                if (value == "")
                    return $"string {name}";
                else
                    return $"string {name} = \"{value}\"";
            else
                return $"\"{value}\"";
        }
    }

    public class FsmObject : NamedVariable
    {
        public INamedAssetProvider value;
        public FsmObject() { }
        public FsmObject(IDataProvider field) : base(field)
        {
            value = field.Get<INamedAssetProvider>("value");
        }
        public override string MyToString()
        {
            if (name != "")
                if (string.IsNullOrEmpty(value?.name))
                    return $"object {name}";
                else
                    return $"object {name} = [{value}]";
            else
                return $"[{value}]";
        }
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
                object valueObj;
                if (value is NamedAssetPPtr namedPPtr && namedPPtr.name != "")
                    valueObj = namedPPtr.name;
                else
                    valueObj = value;
                
                if (variableName != "")
                    return $"Var {variableName} = {valueObj}";
                else
                    return $"Var unnamed = {valueObj}";
            }
            else
            {
                if (variableName != "")
                    return $"Var {variableName}";
                else
                    return $"Var";
            }
        }
    }

    public class FsmArray : NamedVariable
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
        public override string MyToString()
        {
            if (name != "")
                return $"Array {name}";
            else
                return $"Array {type} {objectTypeName}";
        }
    }

    public class FsmFloat : NamedVariable
    {
        public float value;
        public FsmFloat() { }
        public FsmFloat(IDataProvider field) : base(field)
        {
            value = field.Get<float>("value");
        }
        public override string MyToString()
        {
            if (name != "")
                return $"float {name}";
            else
                return value + "f";
        }
    }

    public class FsmInt : NamedVariable
    {
        public int value;
        public FsmInt() { }
        public FsmInt(IDataProvider field) : base(field)
        {
            value = field.Get<int>("value");
        }
        public override string MyToString()
        {
            if (name != "")
                return $"int {name}";
            else
                return value.ToString();
        }
    }

    public class FsmBool : NamedVariable
    {
        public bool value;
        public FsmBool() { }
        public FsmBool(IDataProvider field) : base(field)
        {
            value = field.Get<bool>("value");
        }
        public override string MyToString()
        {
            if (name != "")
                return $"bool {name}";
            else
                return value.ToString().ToLower();
        }
    }

    public class FsmVector2 : NamedVariable
    {
        public Vector2 value;
        public FsmVector2() { }
        public FsmVector2(IDataProvider field) : base(field)
        {
            value = new Vector2(field.Get<IDataProvider>("value"));
        }
        public override string MyToString()
        {
            if (name != "")
                return $"Vector2 {name}";
            else
                return value.ToString();
        }
    }

    public class FsmVector3 : NamedVariable
    {
        public Vector3 value;
        public FsmVector3() { }
        public FsmVector3(IDataProvider field) : base(field)
        {
            value = new Vector3(field.Get<IDataProvider>("value"));
        }
        public override string MyToString()
        {
            if (name != "")
                return $"Vector3 {name}";
            else
                return value.ToString();
        }
    }

    public class FsmColor : NamedVariable
    {
        public UnityColor value;
        public FsmColor() { }
        public FsmColor(IDataProvider field) : base(field)
        {
            value = new UnityColor(field.Get<IDataProvider>("value"));
        }
        public override string MyToString()
        {
            if (name != "")
                return $"Color {name}";
            else
                return value.ToString();
        }
    }

    public class FsmRect : NamedVariable
    {
        public UnityRect value;
        public FsmRect() { }
        public FsmRect(IDataProvider field) : base(field)
        {
            value = new UnityRect(field.Get<IDataProvider>("value"));
        }
        public override string MyToString()
        {
            if (name != "")
                return $"Rect {name}";
            else
                return value.ToString();
        }
    }

    public class FsmQuaternion : NamedVariable
    {
        public Quaternion value;
        public FsmQuaternion() { }
        public FsmQuaternion(IDataProvider field) : base(field)
        {
            value = new Quaternion(field.Get<IDataProvider>("value"));
        }
        public override string MyToString()
        {
            if (name != "")
                return $"Quaternion {name}";
            else
                return value.ToString();
        }
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
        public override sealed string ToString()
        {
            if (isGlobal)
            {
                return "[Global] " + MyToString();
            }
            else
            {
                return MyToString();
            }
        }
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
        public override string ToString()
        {
            return $"Vector2({x}, {y})";
        }
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
        public override string ToString()
        {
            return $"Vector3({x}, {y}, {z})";
        }
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
        public override string ToString()
        {
            return $"Vector4({x}, {y}, {z}, {w})";
        }
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
        public override string ToString()
        {
            return $"Quaternion({x}, {y}, {z}, {w})";
        }
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
        public override string ToString()
        {
            return $"Color({r}, {g}, {b}, {a})";
        }
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
        public override string ToString()
        {
            return $"Rect({x}, {y}, {width}, {height})";
        }
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
        public override string MyToString()
        {
            if (name != "")
                return $"Enum {name}";
            else
                return $"Enum({enumName}, {intValue})";
        }
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
        public override string ToString()
        {
            return $"VarOverride({variable.name})";
        }
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
        public override string ToString()
        {
            return $"Transition({fsmEvent.name} -> {toState})";
        }
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
        public override string ToString()
        {
            return $"Event({name})";
        }
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
}

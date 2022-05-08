using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

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
        public static AssetPPtr ReadAssetPPtr(AssetTypeValueField field)
        {
            int fileId = field.Get("m_FileID").GetValue().AsInt();
            long pathId = field.Get("m_PathID").GetValue().AsInt64();
            return new AssetPPtr(fileId, pathId);
        }
        public static NamedAssetPPtr ReadNamedAssetPPtr(AssetNameResolver namer, AssetTypeValueField field)
        {
            int fileId = field.Get("m_FileID").GetValue().AsInt();
            long pathId = field.Get("m_PathID").GetValue().AsInt64();
            return namer.GetNamedPtr(new AssetPPtr(fileId, pathId));
        }
        public static List<T> ReadAssetList<T>(AssetNameResolver namer, AssetTypeValueField field)
        {
            List<T> data = new List<T>();
            int size = field.GetChildrenCount();
            switch (typeof(T))
            {
                case Type intType when intType == typeof(int):
                    for (int i = 0; i < size; i++)
                        data.Add((T)(object)field[i].GetValue().AsInt());
                    return data;
                case Type stringType when stringType == typeof(string):
                    for (int i = 0; i < size; i++)
                        data.Add((T)(object)field[i].GetValue().AsString());
                    return data;
                case Type boolType when boolType == typeof(bool):
                    for (int i = 0; i < size; i++)
                        data.Add((T)(object)field[i].GetValue().AsBool());
                    return data;
                case Type byteType when byteType == typeof(byte):
                    for (int i = 0; i < size; i++)
                        data.Add((T)(object)(byte)field[i].GetValue().AsInt());
                    return data;
                case Type enumType when enumType.IsEnum:
                    for (int i = 0; i < size; i++)
                        data.Add((T)(object)field[i].GetValue().AsInt());
                    return data;
                //don't use this anymore, does not have name info
                case Type pptrType when pptrType == typeof(AssetPPtr):
                    for (int i = 0; i < size; i++)
                        data.Add((T)(object)ReadAssetPPtr(field[i]));
                    return data;
                case Type namedPPtrType when namedPPtrType == typeof(NamedAssetPPtr):
                    for (int i = 0; i < size; i++)
                        data.Add((T)(object)ReadNamedAssetPPtr(namer, field[i]));
                    return data;
                default:
                    //no error checking so don't put something stupid for <T>
                    for (int i = 0; i < size; i++)
                        data.Add((T)Activator.CreateInstance(typeof(T), namer, field[i]));
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
        public FsmState(AssetNameResolver namer, AssetTypeValueField field)
        {
            name = field.Get("name").GetValue().AsString();
            description = field.Get("description").GetValue().AsString();
            colorIndex = (byte)field.Get("colorIndex").GetValue().AsUInt();
            position = new UnityRect(field.Get("position"));
            isBreakpoint = field.Get("isBreakpoint").GetValue().AsBool();
            isSequence = field.Get("isSequence").GetValue().AsBool();
            hideUnused = field.Get("hideUnused").GetValue().AsBool();
            transitions = new FsmTransition[field.Get("transitions").GetChildrenCount()];
            for (int i = 0; i < transitions.Length; i++)
            {
                transitions[i] = new FsmTransition(field.Get("transitions")[i]);
            }
            actionData = new ActionData(namer, field.Get("actionData"));
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
        public List<NamedAssetPPtr> unityObjectParams;
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
        public ActionData(AssetNameResolver namer, AssetTypeValueField field)
        {
            actionNames = StructUtil.ReadAssetList<string>(namer, field.Get("actionNames"));
            customNames = StructUtil.ReadAssetList<string>(namer, field.Get("customNames"));
            actionEnabled = StructUtil.ReadAssetList<bool>(namer, field.Get("actionEnabled"));
            actionIsOpen = StructUtil.ReadAssetList<bool>(namer, field.Get("actionIsOpen"));
            actionStartIndex = StructUtil.ReadAssetList<int>(namer, field.Get("actionStartIndex"));
            actionHashCodes = StructUtil.ReadAssetList<int>(namer, field.Get("actionHashCodes"));
            unityObjectParams = StructUtil.ReadAssetList<NamedAssetPPtr>(namer, field.Get("unityObjectParams"));
            fsmGameObjectParams = StructUtil.ReadAssetList<FsmGameObject>(namer, field.Get("fsmGameObjectParams"));
            fsmOwnerDefaultParams = StructUtil.ReadAssetList<FsmOwnerDefault>(namer, field.Get("fsmOwnerDefaultParams"));
            animationCurveParams = StructUtil.ReadAssetList<FsmAnimationCurve>(namer, field.Get("animationCurveParams"));
            functionCallParams = StructUtil.ReadAssetList<FsmFunctionCall>(namer, field.Get("functionCallParams"));
            fsmTemplateControlParams = StructUtil.ReadAssetList<FsmTemplateControl>(namer, field.Get("fsmTemplateControlParams"));
            fsmEventTargetParams = StructUtil.ReadAssetList<FsmEventTarget>(namer, field.Get("fsmEventTargetParams"));
            fsmPropertyParams = StructUtil.ReadAssetList<FsmProperty>(namer, field.Get("fsmPropertyParams"));
            layoutOptionParams = StructUtil.ReadAssetList<FsmLayoutOption>(namer, field.Get("layoutOptionParams"));
            fsmStringParams = StructUtil.ReadAssetList<FsmString>(namer, field.Get("fsmStringParams"));
            fsmObjectParams = StructUtil.ReadAssetList<FsmObject>(namer, field.Get("fsmObjectParams"));
            fsmVarParams = StructUtil.ReadAssetList<FsmVar>(namer, field.Get("fsmVarParams"));
            fsmArrayParams = StructUtil.ReadAssetList<FsmArray>(namer, field.Get("fsmArrayParams"));
            fsmEnumParams = StructUtil.ReadAssetList<FsmEnum>(namer, field.Get("fsmEnumParams"));
            fsmFloatParams = StructUtil.ReadAssetList<FsmFloat>(namer, field.Get("fsmFloatParams"));
            fsmIntParams = StructUtil.ReadAssetList<FsmInt>(namer, field.Get("fsmIntParams"));
            fsmBoolParams = StructUtil.ReadAssetList<FsmBool>(namer, field.Get("fsmBoolParams"));
            fsmVector2Params = StructUtil.ReadAssetList<FsmVector2>(namer, field.Get("fsmVector2Params"));
            fsmVector3Params = StructUtil.ReadAssetList<FsmVector3>(namer, field.Get("fsmVector3Params"));
            fsmColorParams = StructUtil.ReadAssetList<FsmColor>(namer, field.Get("fsmColorParams"));
            fsmRectParams = StructUtil.ReadAssetList<FsmRect>(namer, field.Get("fsmRectParams"));
            fsmQuaternionParams = StructUtil.ReadAssetList<FsmQuaternion>(namer, field.Get("fsmQuaternionParams"));
            stringParams = StructUtil.ReadAssetList<string>(namer, field.Get("stringParams"));
            byteData = StructUtil.ReadAssetList<byte>(namer, field.Get("byteData"));
            arrayParamSizes = StructUtil.ReadAssetList<int>(namer, field.Get("arrayParamSizes"));
            arrayParamTypes = StructUtil.ReadAssetList<string>(namer, field.Get("arrayParamTypes"));
            customTypeSizes = StructUtil.ReadAssetList<int>(namer, field.Get("customTypeSizes"));
            customTypeNames = StructUtil.ReadAssetList<string>(namer, field.Get("customTypeNames"));
            paramDataType = StructUtil.ReadAssetList<ParamDataType>(namer, field.Get("paramDataType"));
            paramName = StructUtil.ReadAssetList<string>(namer, field.Get("paramName"));
            paramDataPos = StructUtil.ReadAssetList<int>(namer, field.Get("paramDataPos"));
            paramByteDataSize = StructUtil.ReadAssetList<int>(namer, field.Get("paramByteDataSize"));
        }
    }

    public class FsmGameObject : NamedVariable
    {
        public NamedAssetPPtr value;
        public FsmGameObject() { }
        public FsmGameObject(AssetNameResolver namer, AssetTypeValueField field) : base(field)
        {
            value = StructUtil.ReadNamedAssetPPtr(namer, field.Get("value"));
            if (name == "")
                name = value.name;
        }
        public override string ToString()
        {
            if (value.name != "")
                return $"[{value}]";
            else
                return $"GameObject {name}";
        }
    }

    public class FsmOwnerDefault
    {
        public OwnerDefaultOption ownerOption;
        public FsmGameObject gameObject;
        public FsmOwnerDefault() { }
        public FsmOwnerDefault(AssetNameResolver namer, AssetTypeValueField field)
        {
            ownerOption = (OwnerDefaultOption)field.Get("ownerOption").GetValue().AsInt();
            gameObject = new FsmGameObject(namer, field.Get("gameObject"));
        }
        public override string ToString()
        {
            if (ownerOption == OwnerDefaultOption.UseOwner)
                return "OwnerDefault FSM Owner";
            else
                return $"OwnerDefault {(gameObject.name == null ? gameObject.ToString() : gameObject.name)}";
        }
    }

    public class FsmAnimationCurve
    {
        public AnimationCurve value;
        public FsmAnimationCurve() { }
        public FsmAnimationCurve(AssetNameResolver namer, AssetTypeValueField field)
        {
            value = new AnimationCurve(namer, field);
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
        public FsmFunctionCall(AssetNameResolver namer, AssetTypeValueField field)
        {
            FunctionName = field.Get("FunctionName").GetValue().AsString();
            parameterType = field.Get("parameterType").GetValue().AsString();
            BoolParameter = new FsmBool(namer, field.Get("BoolParameter"));
            FloatParameter = new FsmFloat(namer, field.Get("FloatParameter"));
            IntParameter = new FsmInt(namer, field.Get("IntParameter"));
            GameObjectParameter = new FsmGameObject(namer, field.Get("GameObjectParameter"));
            ObjectParameter = new FsmObject(namer, field.Get("ObjectParameter"));
            StringParameter = new FsmString(namer, field.Get("StringParameter"));
            Vector2Parameter = new FsmVector2(namer, field.Get("Vector2Parameter"));
            Vector3Parameter = new FsmVector3(namer, field.Get("Vector3Parameter"));
            RectParamater = new FsmRect(namer, field.Get("RectParamater"));
            QuaternionParameter = new FsmQuaternion(namer, field.Get("QuaternionParameter"));
            MaterialParameter = new FsmObject(namer, field.Get("MaterialParameter"));
            TextureParameter = new FsmObject(namer, field.Get("TextureParameter"));
            ColorParameter = new FsmColor(namer, field.Get("ColorParameter"));
            EnumParameter = new FsmEnum(namer, field.Get("EnumParameter"));
            ArrayParameter = new FsmArray(namer, field.Get("ArrayParameter"));
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
        public FsmTemplateControl(AssetNameResolver namer, AssetTypeValueField field)
        {
            fsmTemplate = StructUtil.ReadAssetPPtr(field.Get("fsmTemplate"));
            fsmVarOverrides = new FsmVarOverride[field.Get("fsmVarOverrides").GetChildrenCount()];
            for (int i = 0; i < fsmVarOverrides.Length; i++)
            {
                fsmVarOverrides[i] = new FsmVarOverride(namer, field.Get("fsmVarOverrides")[i]);
            }
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
        public FsmEventTarget(AssetNameResolver namer, AssetTypeValueField field)
        {
            target = (EventTarget)field.Get("target").GetValue().AsInt();
            excludeSelf = new FsmBool(namer, field.Get("excludeSelf"));
            gameObject = new FsmOwnerDefault(namer, field.Get("gameObject"));
            fsmName = new FsmString(namer, field.Get("fsmName"));
            sendToChildren = new FsmBool(namer, field.Get("sendToChildren"));
            fsmComponent = StructUtil.ReadNamedAssetPPtr(namer, field.Get("fsmComponent"));
        }
        public override string ToString()
        {
            return $"EventTarget {target.ToString()} {(excludeSelf.value ? "!" : "")}{gameObject}";
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
        public FsmProperty(AssetNameResolver namer, AssetTypeValueField field)
        {
            TargetObject = new FsmObject(namer, field.Get("TargetObject"));
            TargetTypeName = field.Get("TargetTypeName").GetValue().AsString();
            PropertyName = field.Get("PropertyName").GetValue().AsString();
            BoolParameter = new FsmBool(namer, field.Get("BoolParameter"));
            FloatParameter = new FsmFloat(namer, field.Get("FloatParameter"));
            IntParameter = new FsmInt(namer, field.Get("IntParameter"));
            GameObjectParameter = new FsmGameObject(namer, field.Get("GameObjectParameter"));
            ObjectParameter = new FsmObject(namer, field.Get("ObjectParameter"));
            StringParameter = new FsmString(namer, field.Get("StringParameter"));
            Vector2Parameter = new FsmVector2(namer, field.Get("Vector2Parameter"));
            Vector3Parameter = new FsmVector3(namer, field.Get("Vector3Parameter"));
            RectParamater = new FsmRect(namer, field.Get("RectParamater"));
            QuaternionParameter = new FsmQuaternion(namer, field.Get("QuaternionParameter"));
            MaterialParameter = new FsmObject(namer, field.Get("MaterialParameter"));
            TextureParameter = new FsmObject(namer, field.Get("TextureParameter"));
            ColorParameter = new FsmColor(namer, field.Get("ColorParameter"));
            EnumParameter = new FsmEnum(namer, field.Get("EnumParameter"));
            ArrayParameter = new FsmArray(namer, field.Get("ArrayParameter"));
            setProperty = field.Get("setProperty").GetValue().AsBool();
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
        public FsmLayoutOption(AssetNameResolver namer, AssetTypeValueField field) : base(field)
        {
            option = (LayoutOptionType)field.Get("option").GetValue().AsInt();
            floatParam = new FsmFloat(namer, field.Get("floatParam"));
            boolParam = new FsmBool(namer, field.Get("boolParam"));
        }
        public override string ToString()
        {
            if (name != "")
                return $"LayoutOption {name} = {option.ToString()}";
            else
                return $"LayoutOption.{option.ToString()}";
        }
    }

    public class FsmString : NamedVariable
    {
        public string value;
        public FsmString() { }
        public FsmString(AssetNameResolver namer, AssetTypeValueField field) : base(field)
        {
            value = field.Get("value").GetValue().AsString();
        }
        public override string ToString()
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
        public NamedAssetPPtr value;
        public FsmObject() { }
        public FsmObject(AssetNameResolver namer, AssetTypeValueField field) : base(field)
        {
            value = StructUtil.ReadNamedAssetPPtr(namer, field.Get("value"));
        }
        public override string ToString()
        {
            if (name != "")
                if (value.name == "")
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
        public NamedAssetPPtr objectReference;
        public FsmArray arrayValue;
        public FsmVar() { }
        public FsmVar(AssetNameResolver namer, AssetTypeValueField field)
        {
            variableName = field.Get("variableName").GetValue().AsString();
            objectType = field.Get("objectType").GetValue().AsString();
            useVariable = field.Get("useVariable").GetValue().AsBool();
            type = (VariableType)field.Get("type").GetValue().AsInt();
            floatValue = field.Get("floatValue").GetValue().AsFloat();
            intValue = field.Get("intValue").GetValue().AsInt();
            boolValue = field.Get("boolValue").GetValue().AsBool();
            stringValue = field.Get("stringValue").GetValue().AsString();
            vector4Value = new Vector4(field.Get("vector4Value"));
            objectReference = StructUtil.ReadNamedAssetPPtr(namer, field.Get("objectReference"));
            arrayValue = new FsmArray(namer, field.Get("arrayValue"));
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
        public AssetPPtr[] objectReferences;
        public FsmArray() { }
        public FsmArray(AssetNameResolver namer, AssetTypeValueField field) : base(field)
        {
            type = (VariableType)field.Get("type").GetValue().AsInt();
            objectTypeName = field.Get("objectTypeName").GetValue().AsString();
            floatValues = new float[field.Get("floatValues").GetChildrenCount()];
            for (int i = 0; i < floatValues.Length; i++)
            {
                floatValues[i] = field.Get("floatValues")[i].GetValue().AsFloat();
            }
            intValues = new int[field.Get("intValues").GetChildrenCount()];
            for (int i = 0; i < intValues.Length; i++)
            {
                intValues[i] = field.Get("intValues")[i].GetValue().AsInt();
            }
            boolValues = new bool[field.Get("boolValues").GetChildrenCount()];
            for (int i = 0; i < boolValues.Length; i++)
            {
                boolValues[i] = field.Get("boolValues")[i].GetValue().AsBool();
            }
            stringValues = new string[field.Get("stringValues").GetChildrenCount()];
            for (int i = 0; i < stringValues.Length; i++)
            {
                stringValues[i] = field.Get("stringValues")[i].GetValue().AsString();
            }
            vector4Values = new Vector4[field.Get("vector4Values").GetChildrenCount()];
            for (int i = 0; i < vector4Values.Length; i++)
            {
                vector4Values[i] = new Vector4(field.Get("vector4Values")[i]);
            }
            objectReferences = new AssetPPtr[field.Get("objectReferences").GetChildrenCount()];
            for (int i = 0; i < objectReferences.Length; i++)
            {
                objectReferences[i] = StructUtil.ReadAssetPPtr(field.Get("objectReferences")[i]);
            }
        }
        public override string ToString()
        {
            if (name != "")
                return $"Array {name}";
            else
                return $"Array {type.ToString()} {objectTypeName}";
        }
    }

    public class FsmFloat : NamedVariable
    {
        public float value;
        public FsmFloat() { }
        public FsmFloat(AssetNameResolver namer, AssetTypeValueField field) : base(field)
        {
            value = field.Get("value").GetValue().AsFloat();
        }
        public override string ToString()
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
        public FsmInt(AssetNameResolver namer, AssetTypeValueField field) : base(field)
        {
            value = field.Get("value").GetValue().AsInt();
        }
        public override string ToString()
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
        public FsmBool(AssetNameResolver namer, AssetTypeValueField field) : base(field)
        {
            value = field.Get("value").GetValue().AsBool();
        }
        public override string ToString()
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
        public FsmVector2(AssetNameResolver namer, AssetTypeValueField field) : base(field)
        {
            value = new Vector2(field.Get("value"));
        }
        public override string ToString()
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
        public FsmVector3(AssetNameResolver namer, AssetTypeValueField field) : base(field)
        {
            value = new Vector3(field.Get("value"));
        }
        public override string ToString()
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
        public FsmColor(AssetNameResolver namer, AssetTypeValueField field) : base(field)
        {
            value = new UnityColor(field.Get("value"));
        }
        public override string ToString()
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
        public FsmRect(AssetNameResolver namer, AssetTypeValueField field) : base(field)
        {
            value = new UnityRect(field.Get("value"));
        }
        public override string ToString()
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
        public FsmQuaternion(AssetNameResolver namer, AssetTypeValueField field) : base(field)
        {
            value = new Quaternion(field.Get("value"));
        }
        public override string ToString()
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
        public NamedVariable() { }
        public NamedVariable(AssetTypeValueField field)
        {
            useVariable = field.Get("useVariable").GetValue().AsBool();
            name = field.Get("name").GetValue().AsString();
            tooltip = field.Get("tooltip").GetValue().AsString();
            showInInspector = field.Get("showInInspector").GetValue().AsBool();
            networkSync = field.Get("networkSync").GetValue().AsBool();
        }
    }

    public class AnimationCurve
    {
        public List<Keyframe> m_Curve;
        public int m_PreInfinity;
        public int m_PostInfinity;
        public int m_RotationOrder;
        public AnimationCurve() { }
        public AnimationCurve(AssetNameResolver namer, AssetTypeValueField field)
        {
            m_Curve = StructUtil.ReadAssetList<Keyframe>(namer, field.Get("m_Curve"));
            m_PreInfinity = field.Get("m_PreInfinity").GetValue().AsInt();
            m_PostInfinity = field.Get("m_PostInfinity").GetValue().AsInt();
            m_RotationOrder = field.Get("m_RotationOrder").GetValue().AsInt();
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
        public Keyframe(AssetTypeValueField field)
        {
            time = field.Get("time").GetValue().AsFloat();
            value = field.Get("value").GetValue().AsFloat();
            inSlope = field.Get("inSlope").GetValue().AsFloat();
            outSlope = field.Get("outSlope").GetValue().AsFloat();
        }
    }

    public class Vector2
    {
        public float x;
        public float y;
        public Vector2() { }
        public Vector2(AssetTypeValueField field)
        {
            x = field.Get("x").GetValue().AsFloat();
            y = field.Get("y").GetValue().AsFloat();
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
        public Vector3(AssetTypeValueField field)
        {
            x = field.Get("x").GetValue().AsFloat();
            y = field.Get("y").GetValue().AsFloat();
            z = field.Get("z").GetValue().AsFloat();
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
        public Vector4(AssetTypeValueField field)
        {
            x = field.Get("x").GetValue().AsFloat();
            y = field.Get("y").GetValue().AsFloat();
            z = field.Get("z").GetValue().AsFloat();
            w = field.Get("w").GetValue().AsFloat();
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
        public Quaternion(AssetTypeValueField field)
        {
            x = field.Get("x").GetValue().AsFloat();
            y = field.Get("y").GetValue().AsFloat();
            z = field.Get("z").GetValue().AsFloat();
            w = field.Get("w").GetValue().AsFloat();
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
        public UnityColor(AssetTypeValueField field)
        {
            r = field.Get("r").GetValue().AsFloat();
            g = field.Get("g").GetValue().AsFloat();
            b = field.Get("b").GetValue().AsFloat();
            a = field.Get("a").GetValue().AsFloat();
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
        public UnityRect(AssetTypeValueField field)
        {
            x = field.Get("x").GetValue().AsFloat();
            y = field.Get("y").GetValue().AsFloat();
            width = field.Get("width").GetValue().AsFloat();
            height = field.Get("height").GetValue().AsFloat();
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
        public FsmEnum(AssetNameResolver namer, AssetTypeValueField field) : base(field)
        {
            enumName = field.Get("enumName").GetValue().AsString();
            intValue = field.Get("intValue").GetValue().AsInt();
        }
        public override string ToString()
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
        public FsmVarOverride(AssetNameResolver namer, AssetTypeValueField field)
        {
            variable = new NamedVariable(field.Get("variable"));
            fsmVar = new FsmVar(namer, field.Get("fsmVar"));
            isEdited = field.Get("isEdited").GetValue().AsBool();
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
        public FsmTransition(AssetTypeValueField valueField)
        {
            fsmEvent = new FsmEvent(valueField.Get("fsmEvent"));
            toState = valueField.Get("toState").GetValue().AsString();
            linkStyle = valueField.Get("linkStyle").GetValue().AsInt();
            linkConstraint = valueField.Get("linkConstraint").GetValue().AsInt();
            colorIndex = (byte)valueField.Get("colorIndex").GetValue().AsInt();
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
        public FsmEvent(AssetTypeValueField valueField)
        {
            name = valueField.Get("name").GetValue().AsString();
            isSystemEvent = valueField.Get("isSystemEvent").GetValue().AsBool();
            isGlobal = valueField.Get("isGlobal").GetValue().AsBool();
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

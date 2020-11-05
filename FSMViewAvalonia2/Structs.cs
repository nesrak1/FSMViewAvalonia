using AssetsTools.NET;
using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace FSMViewAvalonia2
{
    public class StructUtil
    {
        public static AssetPPtr ReadAssetPPtr(AssetTypeValueField field)
        {
            int fileId = field.Get("m_FileID").GetValue().AsInt();
            long pathId = field.Get("m_PathID").GetValue().AsInt64();
            return new AssetPPtr(fileId, pathId);
        }
        public static List<T> ReadAssetList<T>(AssetTypeValueField field)
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
                case Type pptrType when pptrType == typeof(AssetPPtr):
                    for (int i = 0; i < size; i++)
                        data.Add((T)(object)ReadAssetPPtr(field[i]));
                    return data;
                default:
                    //no error checking so don't put something stupid for <T>
                    for (int i = 0; i < size; i++)
                        data.Add((T)Activator.CreateInstance(typeof(T), field[i]));
                    return data;
            }
        }
    }

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

    public class FsmState
    {
        public string name;
        public string description;
        public byte colorIndex;
        public Rect position;
        public bool isBreakpoint;
        public bool isSequence;
        public bool hideUnused;
        public FsmTransition[] transitions;
        public ActionData actionData;
        public FsmState(AssetTypeValueField field)
        {
            name = field.Get("name").GetValue().AsString();
            description = field.Get("description").GetValue().AsString();
            colorIndex = (byte)field.Get("colorIndex").GetValue().AsUInt();
            AssetTypeValueField positionField = field.Get("position");
            position = new Rect(positionField.Get("x").GetValue().AsFloat(),
                                positionField.Get("y").GetValue().AsFloat(),
                                positionField.Get("width").GetValue().AsFloat(),
                                positionField.Get("height").GetValue().AsFloat());
            isBreakpoint = field.Get("isBreakpoint").GetValue().AsBool();
            isSequence = field.Get("isSequence").GetValue().AsBool();
            hideUnused = field.Get("hideUnused").GetValue().AsBool();
            transitions = new FsmTransition[field.Get("transitions").GetChildrenCount()];
            for (int i = 0; i < transitions.Length; i++)
            {
                transitions[i] = new FsmTransition(field.Get("transitions")[i]);
            }
            actionData = new ActionData(field.Get("actionData"));
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
        public List<AssetPPtr> unityObjectParams;
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
        public ActionData(AssetTypeValueField field)
        {
            actionNames = StructUtil.ReadAssetList<string>(field.Get("actionNames"));
            customNames = StructUtil.ReadAssetList<string>(field.Get("customNames"));
            actionEnabled = StructUtil.ReadAssetList<bool>(field.Get("actionEnabled"));
            actionIsOpen = StructUtil.ReadAssetList<bool>(field.Get("actionIsOpen"));
            actionStartIndex = StructUtil.ReadAssetList<int>(field.Get("actionStartIndex"));
            actionHashCodes = StructUtil.ReadAssetList<int>(field.Get("actionHashCodes"));
            unityObjectParams = StructUtil.ReadAssetList<AssetPPtr>(field.Get("unityObjectParams"));
            fsmGameObjectParams = StructUtil.ReadAssetList<FsmGameObject>(field.Get("fsmGameObjectParams"));
            fsmOwnerDefaultParams = StructUtil.ReadAssetList<FsmOwnerDefault>(field.Get("fsmOwnerDefaultParams"));
            animationCurveParams = StructUtil.ReadAssetList<FsmAnimationCurve>(field.Get("animationCurveParams"));
            fsmTemplateControlParams = StructUtil.ReadAssetList<FsmTemplateControl>(field.Get("fsmTemplateControlParams"));
            fsmEventTargetParams = StructUtil.ReadAssetList<FsmEventTarget>(field.Get("fsmEventTargetParams"));
            fsmPropertyParams = StructUtil.ReadAssetList<FsmProperty>(field.Get("fsmPropertyParams"));
            layoutOptionParams = StructUtil.ReadAssetList<FsmLayoutOption>(field.Get("layoutOptionParams"));
            fsmStringParams = StructUtil.ReadAssetList<FsmString>(field.Get("fsmStringParams"));
            fsmObjectParams = StructUtil.ReadAssetList<FsmObject>(field.Get("fsmObjectParams"));
            fsmVarParams = StructUtil.ReadAssetList<FsmVar>(field.Get("fsmVarParams"));
            fsmArrayParams = StructUtil.ReadAssetList<FsmArray>(field.Get("fsmArrayParams"));
            fsmEnumParams = StructUtil.ReadAssetList<FsmEnum>(field.Get("fsmEnumParams"));
            fsmFloatParams = StructUtil.ReadAssetList<FsmFloat>(field.Get("fsmFloatParams"));
            fsmIntParams = StructUtil.ReadAssetList<FsmInt>(field.Get("fsmIntParams"));
            fsmBoolParams = StructUtil.ReadAssetList<FsmBool>(field.Get("fsmBoolParams"));
            fsmVector2Params = StructUtil.ReadAssetList<FsmVector2>(field.Get("fsmVector2Params"));
            fsmVector3Params = StructUtil.ReadAssetList<FsmVector3>(field.Get("fsmVector3Params"));
            fsmColorParams = StructUtil.ReadAssetList<FsmColor>(field.Get("fsmColorParams"));
            fsmRectParams = StructUtil.ReadAssetList<FsmRect>(field.Get("fsmRectParams"));
            fsmQuaternionParams = StructUtil.ReadAssetList<FsmQuaternion>(field.Get("fsmQuaternionParams"));
            stringParams = StructUtil.ReadAssetList<string>(field.Get("stringParams"));
            byteData = StructUtil.ReadAssetList<byte>(field.Get("byteData"));
            arrayParamSizes = StructUtil.ReadAssetList<int>(field.Get("arrayParamSizes"));
            arrayParamTypes = StructUtil.ReadAssetList<string>(field.Get("arrayParamTypes"));
            customTypeSizes = StructUtil.ReadAssetList<int>(field.Get("customTypeSizes"));
            customTypeNames = StructUtil.ReadAssetList<string>(field.Get("customTypeNames"));
            paramDataType = StructUtil.ReadAssetList<ParamDataType>(field.Get("paramDataType"));
            paramName = StructUtil.ReadAssetList<string>(field.Get("paramName"));
            paramDataPos = StructUtil.ReadAssetList<int>(field.Get("paramDataPos"));
            paramByteDataSize = StructUtil.ReadAssetList<int>(field.Get("paramByteDataSize"));
        }
    }

    public class FsmGameObject : NamedVariable
    {
        public AssetPPtr value;
        public FsmGameObject(AssetTypeValueField field) : base(field)
        {
            value = StructUtil.ReadAssetPPtr(field.Get("value"));
        }
    }

    public class FsmOwnerDefault
    {
        public OwnerDefaultOption ownerOption;
        public FsmGameObject gameObject;
        public FsmOwnerDefault(AssetTypeValueField field)
        {
            ownerOption = (OwnerDefaultOption)field.Get("ownerOption").GetValue().AsInt();
            gameObject = new FsmGameObject(field.Get("gameObject"));
        }
    }

    public class FsmAnimationCurve
    {
        public AnimationCurve value;
        public FsmAnimationCurve(AssetTypeValueField field)
        {
            value = new AnimationCurve(field);
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
        public FsmFunctionCall(AssetTypeValueField field)
        {
            FunctionName = field.Get("FunctionName").GetValue().AsString();
            parameterType = field.Get("parameterType").GetValue().AsString();
            BoolParameter = new FsmBool(field.Get("BoolParameter"));
            FloatParameter = new FsmFloat(field.Get("FloatParameter"));
            IntParameter = new FsmInt(field.Get("IntParameter"));
            GameObjectParameter = new FsmGameObject(field.Get("GameObjectParameter"));
            ObjectParameter = new FsmObject(field.Get("ObjectParameter"));
            StringParameter = new FsmString(field.Get("StringParameter"));
            Vector2Parameter = new FsmVector2(field.Get("Vector2Parameter"));
            Vector3Parameter = new FsmVector3(field.Get("Vector3Parameter"));
            RectParamater = new FsmRect(field.Get("RectParamater"));
            QuaternionParameter = new FsmQuaternion(field.Get("QuaternionParameter"));
            MaterialParameter = new FsmObject(field.Get("MaterialParameter"));
            TextureParameter = new FsmObject(field.Get("TextureParameter"));
            ColorParameter = new FsmColor(field.Get("ColorParameter"));
            EnumParameter = new FsmEnum(field.Get("EnumParameter"));
            ArrayParameter = new FsmArray(field.Get("ArrayParameter"));
        }
    }

    public class FsmTemplateControl
    {
        public AssetPPtr fsmTemplate;
        public FsmVarOverride[] fsmVarOverrides;
        public FsmTemplateControl(AssetTypeValueField field)
        {
            fsmTemplate = StructUtil.ReadAssetPPtr(field.Get("fsmTemplate"));
            fsmVarOverrides = new FsmVarOverride[field.Get("fsmVarOverrides").GetChildrenCount()];
            for (int i = 0; i < fsmVarOverrides.Length; i++)
            {
                fsmVarOverrides[i] = new FsmVarOverride(field.Get("fsmVarOverrides")[i]);
            }
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
        public FsmEventTarget(AssetTypeValueField field)
        {
            target = (EventTarget)field.Get("target").GetValue().AsInt();
            excludeSelf = new FsmBool(field.Get("excludeSelf"));
            gameObject = new FsmOwnerDefault(field.Get("gameObject"));
            fsmName = new FsmString(field.Get("fsmName"));
            sendToChildren = new FsmBool(field.Get("sendToChildren"));
            fsmComponent = StructUtil.ReadAssetPPtr(field.Get("fsmComponent"));
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
        public FsmProperty(AssetTypeValueField field)
        {
            TargetObject = new FsmObject(field.Get("TargetObject"));
            TargetTypeName = field.Get("TargetTypeName").GetValue().AsString();
            PropertyName = field.Get("PropertyName").GetValue().AsString();
            BoolParameter = new FsmBool(field.Get("BoolParameter"));
            FloatParameter = new FsmFloat(field.Get("FloatParameter"));
            IntParameter = new FsmInt(field.Get("IntParameter"));
            GameObjectParameter = new FsmGameObject(field.Get("GameObjectParameter"));
            ObjectParameter = new FsmObject(field.Get("ObjectParameter"));
            StringParameter = new FsmString(field.Get("StringParameter"));
            Vector2Parameter = new FsmVector2(field.Get("Vector2Parameter"));
            Vector3Parameter = new FsmVector3(field.Get("Vector3Parameter"));
            RectParamater = new FsmRect(field.Get("RectParamater"));
            QuaternionParameter = new FsmQuaternion(field.Get("QuaternionParameter"));
            MaterialParameter = new FsmObject(field.Get("MaterialParameter"));
            TextureParameter = new FsmObject(field.Get("TextureParameter"));
            ColorParameter = new FsmColor(field.Get("ColorParameter"));
            EnumParameter = new FsmEnum(field.Get("EnumParameter"));
            ArrayParameter = new FsmArray(field.Get("ArrayParameter"));
            setProperty = field.Get("setProperty").GetValue().AsBool();
        }
    }

    public class FsmLayoutOption : NamedVariable
    {
        public LayoutOptionType option;
        public FsmFloat floatParam;
        public FsmBool boolParam;
        public FsmLayoutOption(AssetTypeValueField field) : base(field)
        {
            option = (LayoutOptionType)field.Get("option").GetValue().AsInt();
            floatParam = new FsmFloat(field.Get("floatParam"));
            boolParam = new FsmBool(field.Get("boolParam"));
        }
    }

    public class FsmString : NamedVariable
    {
        public string value;
        public FsmString(AssetTypeValueField field) : base(field)
        {
            value = field.Get("value").GetValue().AsString();
        }
    }

    public class FsmObject : NamedVariable
    {
        public AssetPPtr value;
        public FsmObject(AssetTypeValueField field) : base(field)
        {
            value = StructUtil.ReadAssetPPtr(field.Get("value"));
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
        public AssetPPtr objectReference;
        public FsmArray arrayValue;
        public FsmVar(AssetTypeValueField field)
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
            objectReference = StructUtil.ReadAssetPPtr(field.Get("objectReference"));
            arrayValue = new FsmArray(field.Get("arrayValue"));
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
        public FsmArray(AssetTypeValueField field) : base(field)
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
    }

    public class FsmFloat : NamedVariable
    {
        public float value;
        public FsmFloat(AssetTypeValueField field) : base(field)
        {
            value = field.Get("value").GetValue().AsFloat();
        }
    }

    public class FsmInt : NamedVariable
    {
        public int value;
        public FsmInt(AssetTypeValueField field) : base(field)
        {
            value = field.Get("value").GetValue().AsInt();
        }
    }

    public class FsmBool : NamedVariable
    {
        public bool value;
        public FsmBool(AssetTypeValueField field) : base(field)
        {
            value = field.Get("value").GetValue().AsBool();
        }
    }

    public class FsmVector2 : NamedVariable
    {
        public Vector2 value;
        public FsmVector2(AssetTypeValueField field) : base(field)
        {
            value = new Vector2(field.Get("value"));
        }
    }

    public class FsmVector3 : NamedVariable
    {
        public Vector3 value;
        public FsmVector3(AssetTypeValueField field) : base(field)
        {
            value = new Vector3(field.Get("value"));
        }
    }

    public class FsmColor : NamedVariable
    {
        public Color value;
        public FsmColor(AssetTypeValueField field) : base(field)
        {
            AssetTypeValueField valueField = field.Get("value");
            value = new Color((byte)valueField.Get("a").GetValue().AsInt(),
                              (byte)valueField.Get("r").GetValue().AsInt(),
                              (byte)valueField.Get("g").GetValue().AsInt(),
                              (byte)valueField.Get("b").GetValue().AsInt());
        }
    }

    public class FsmRect : NamedVariable
    {
        public Rect value;
        public FsmRect(AssetTypeValueField field) : base(field)
        {
            AssetTypeValueField valueField = field.Get("value");
            value = new Rect(valueField.Get("x").GetValue().AsFloat(),
                             valueField.Get("y").GetValue().AsFloat(),
                             valueField.Get("width").GetValue().AsFloat(),
                             valueField.Get("height").GetValue().AsFloat());
        }
    }

    public class FsmQuaternion : NamedVariable
    {
        public Quaternion value;
        public FsmQuaternion(AssetTypeValueField field) : base(field)
        {
            AssetTypeValueField valueField = field.Get("value");
            value = new Quaternion(valueField.Get("x").GetValue().AsFloat(),
                                   valueField.Get("y").GetValue().AsFloat(),
                                   valueField.Get("z").GetValue().AsFloat(),
                                   valueField.Get("w").GetValue().AsFloat());
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
        public AnimationCurve(AssetTypeValueField field)
        {
            m_Curve = StructUtil.ReadAssetList<Keyframe>(field.Get("m_Curve"));
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
        public Vector2(AssetTypeValueField field)
        {
            x = field.Get("x").GetValue().AsFloat();
            y = field.Get("y").GetValue().AsFloat();
        }
    }

    public class Vector3
    {
        public float x;
        public float y;
        public float z;
        public Vector3(AssetTypeValueField field)
        {
            x = field.Get("x").GetValue().AsFloat();
            y = field.Get("y").GetValue().AsFloat();
            z = field.Get("z").GetValue().AsFloat();
        }
    }

    public class Vector4
    {
        public float x;
        public float y;
        public float z;
        public float w;
        public Vector4(AssetTypeValueField field)
        {
            x = field.Get("x").GetValue().AsFloat();
            y = field.Get("y").GetValue().AsFloat();
            z = field.Get("z").GetValue().AsFloat();
            w = field.Get("w").GetValue().AsFloat();
        }
    }

    public class FsmEnum : NamedVariable
    {
        public string enumName;
        public int intValue;
        public FsmEnum(AssetTypeValueField field) : base(field)
        {
            enumName = field.Get("enumName").GetValue().AsString();
            intValue = field.Get("intValue").GetValue().AsInt();
        }
    }

    public class FsmVarOverride
    {
        public NamedVariable variable;
        public FsmVar fsmVar;
        public bool isEdited;
        public FsmVarOverride(AssetTypeValueField field)
        {
            variable = new NamedVariable(field.Get("variable"));
            fsmVar = new FsmVar(field.Get("fsmVar"));
            isEdited = field.Get("isEdited").GetValue().AsBool();
        }
    }

    public class FsmTransition
    {
        public FsmEvent fsmEvent;
        public string toState;
        public int linkStyle;
        public int linkConstraint;
        public byte colorIndex;
        public FsmTransition(AssetTypeValueField valueField)
        {
            fsmEvent = new FsmEvent(valueField.Get("fsmEvent"));
            toState = valueField.Get("toState").GetValue().AsString();
            linkStyle = valueField.Get("linkStyle").GetValue().AsInt();
            linkConstraint = valueField.Get("linkConstraint").GetValue().AsInt();
            colorIndex = (byte)valueField.Get("colorIndex").GetValue().AsInt();
        }
    }

    public class FsmEvent
    {
        public string name;
        public bool isSystemEvent;
        public bool isGlobal;
        public FsmEvent(AssetTypeValueField valueField)
        {
            name = valueField.Get("name").GetValue().AsString();
            isSystemEvent = valueField.Get("isSystemEvent").GetValue().AsBool();
            isGlobal = valueField.Get("isGlobal").GetValue().AsBool();
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

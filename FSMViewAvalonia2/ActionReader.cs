using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FSMViewAvalonia2
{
    public static class ActionReader
    {
        public static object GetFsmObject(this ActionData actionData, int index, int dataVersion)
        {
            BinaryReader r = new BinaryReader(new MemoryStream(actionData.byteData.ToArray()));
            //string actionName = actionData.actionNames[index];
            ParamDataType paramDataType = actionData.paramDataType[index];
            int paramDataPos = actionData.paramDataPos[index];
            int paramByteDataSize = actionData.paramByteDataSize[index];

            r.BaseStream.Position = paramDataPos;

            object ret;
            switch (paramDataType)
            {
                case ParamDataType.Integer:
                    ret = r.ReadInt32();
                    break;
                case ParamDataType.FsmInt when dataVersion == 1:
                    ret = new FsmInt() { value = r.ReadInt32() };
                    break;
                case ParamDataType.Enum:
                    ret = r.ReadInt32();
                    //ret = ((Enum)AssemblyHelper.GetPublicFields(typeDef)[index])[ret];
                    break;
                case ParamDataType.Boolean:
                    ret = r.ReadBoolean();
                    break;
                case ParamDataType.FsmBool when dataVersion == 1:
                    ret = new FsmBool { value = r.ReadBoolean() };
                    break;
                case ParamDataType.Float:
                    ret = r.ReadSingle();
                    break;
                case ParamDataType.FsmFloat when dataVersion == 1:
                    ret = new FsmFloat { value = r.ReadSingle() };
                    break;
                case ParamDataType.String:
                    ret = Encoding.UTF8.GetString(r.ReadBytes(paramByteDataSize));
                    break;
                case ParamDataType.FsmEvent when dataVersion == 1:
                    ret = new FsmEvent { name = Encoding.UTF8.GetString(r.ReadBytes(paramByteDataSize)) };
                    break;
                case ParamDataType.Vector2:
                    ret = new Vector2 { x = r.ReadSingle(), y = r.ReadSingle() };
                    break;
                case ParamDataType.FsmVector2 when dataVersion == 1:
                    ret = new FsmVector2 { value = new Vector2 { x = r.ReadSingle(), y = r.ReadSingle() } };
                    break;
                case ParamDataType.Vector3:
                    ret = new Vector3 { x = r.ReadSingle(), y = r.ReadSingle(), z = r.ReadSingle() };
                    break;
                case ParamDataType.FsmVector3 when dataVersion == 1:
                    ret = new FsmVector3 { value = new Vector3 { x = r.ReadSingle(), y = r.ReadSingle(), z = r.ReadSingle() } };
                    break;
                case ParamDataType.Quaternion:
                    ret = new Quaternion { x = r.ReadSingle(), y = r.ReadSingle(), z = r.ReadSingle(), w = r.ReadSingle() };
                    break;
                case ParamDataType.FsmQuaternion when dataVersion == 1:
                    ret = new FsmQuaternion { value = new Quaternion { x = r.ReadSingle(), y = r.ReadSingle(), z = r.ReadSingle(), w = r.ReadSingle() } };
                    break;
                case ParamDataType.Color:
                    ret = new UnityColor { r = r.ReadSingle(), g = r.ReadSingle(), b = r.ReadSingle(), a = r.ReadSingle() };
                    break;
                case ParamDataType.FsmColor when dataVersion == 1:
                    ret = new FsmColor { value = new UnityColor { r = r.ReadSingle(), g = r.ReadSingle(), b = r.ReadSingle(), a = r.ReadSingle() } };
                    break;
                case ParamDataType.Rect:
                    ret = new UnityRect { x = r.ReadSingle(), y = r.ReadSingle(), width = r.ReadSingle(), height = r.ReadSingle() };
                    break;
                case ParamDataType.FsmRect when dataVersion == 1:
                    ret = new FsmRect { value = new UnityRect { x = r.ReadSingle(), y = r.ReadSingle(), width = r.ReadSingle(), height = r.ReadSingle() } };
                    break;
                /////////////////////////////////////////////////////////
                case ParamDataType.FsmEnum when dataVersion > 1:
                    ret = actionData.fsmEnumParams[paramDataPos];
                    break;
                case ParamDataType.FsmBool when dataVersion > 1:
                    ret = actionData.fsmBoolParams[paramDataPos];
                    break;
                case ParamDataType.FsmFloat when dataVersion > 1:
                    ret = actionData.fsmFloatParams[paramDataPos];
                    break;
                case ParamDataType.FsmVector2 when dataVersion > 1:
                    ret = actionData.fsmVector2Params[paramDataPos];
                    break;
                case ParamDataType.FsmVector3 when dataVersion > 1:
                    ret = actionData.fsmVector3Params[paramDataPos];
                    break;
                case ParamDataType.FsmQuaternion when dataVersion > 1:
                    ret = actionData.fsmQuaternionParams[paramDataPos];
                    break;
                case ParamDataType.FsmColor when dataVersion > 1:
                    ret = actionData.fsmColorParams[paramDataPos];
                    break;
                case ParamDataType.FsmRect when dataVersion > 1:
                    ret = actionData.fsmRectParams[paramDataPos];
                    break;
                /////////////////////////////////////////////////////////
                case ParamDataType.FsmGameObject:
                    ret = actionData.fsmGameObjectParams[paramDataPos];
                    break;
                case ParamDataType.FsmOwnerDefault:
                    ret = actionData.fsmOwnerDefaultParams[paramDataPos];
                    break;
                case ParamDataType.FsmObject:
                    ret = actionData.fsmObjectParams[paramDataPos];
                    break;
                case ParamDataType.FsmVar:
                    ret = actionData.fsmVarParams[paramDataPos];
                    break;
                case ParamDataType.FsmString:
                    ret = actionData.fsmStringParams[paramDataPos];
                    break;
                case ParamDataType.FsmEvent:
                    ret = actionData.stringParams[paramDataPos];
                    break;
                case ParamDataType.FsmEventTarget:
                    ret = actionData.fsmEventTargetParams[paramDataPos];
                    break;
                case ParamDataType.FsmArray:
                    ret = actionData.fsmArrayParams[paramDataPos];
                    break;
                case ParamDataType.ObjectReference:
                    ret = $"ObjRef([{actionData.unityObjectParams[paramDataPos]}])";
                    break;
                case ParamDataType.FunctionCall:
                    ret = actionData.functionCallParams[paramDataPos];
                    break;
                case ParamDataType.Array:
                    ret = "[Array]";
                    break;
                default:
                    ret = $"[{paramDataType.ToString()} not implemented]";
                    break;
            }

            if (dataVersion == 1 && ret is NamedVariable nv)
            {
                switch (paramDataType)
                {
                    case ParamDataType.FsmInt:
                    case ParamDataType.FsmBool:
                    case ParamDataType.FsmFloat:
                    case ParamDataType.FsmVector2:
                    case ParamDataType.FsmVector3:
                    case ParamDataType.FsmQuaternion:
                    case ParamDataType.FsmColor:
                        nv.useVariable = r.ReadBoolean();
                        int nameLength = paramByteDataSize - ((int)r.BaseStream.Position - paramDataPos);
                        nv.name = Encoding.UTF8.GetString(r.ReadBytes(nameLength));
                        break;
                }
            }

            return ret;
        }
    }
}

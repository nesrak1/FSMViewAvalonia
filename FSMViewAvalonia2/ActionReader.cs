

namespace FSMViewAvalonia2
{
    public static class ActionReader
    {
        public static Dictionary<string, ParamDataType> ParamDataTypes = new()
        {

        };
        static ActionReader()
        {
            foreach (var v in Enum.GetNames(typeof(ParamDataType)))
            {
                ParamDataTypes.Add($"HutongGames.PlayMaker.{v}", (ParamDataType)Enum.Parse(typeof(ParamDataType), v));
            }
        }
        public static object GetFsmArray(this ActionData actionData, ref int index, int dataVersion)
        {
            var type = actionData.arrayParamTypes[actionData.paramDataPos[index]];
            var size = actionData.arrayParamSizes[actionData.paramDataPos[index]];
            if (!ParamDataTypes.TryGetValue(type, out var pdt))
            {
                return $"[{size}]({type}[])";
            }
            var result = new FsmArray2
            {
                type = pdt
            };
            var array = result.array = new object[size];
            for (int i = 0; i < size; i++)
            {
                index++;
                array[i] = GetFsmObject(actionData, ref index, dataVersion, pdt);
            }
            return result;
        }
        public static object GetFsmObject(this ActionData actionData, ref int index, int dataVersion)
        {
            //string actionName = actionData.actionNames[index];
            ParamDataType paramDataType = actionData.paramDataType[index];
            object ret = GetFsmObject(actionData, ref index, dataVersion, paramDataType);
            return ret;
        }

        private static object GetFsmObject(ActionData actionData, ref int index, int dataVersion,
            ParamDataType paramDataType)
        {
            try
            {
                int paramDataPos = actionData.paramDataPos[index];
                int paramByteDataSize = actionData.paramByteDataSize[index];
                BinaryReader r = new(new MemoryStream(actionData.byteData.ToArray()));
                r.BaseStream.Position = paramDataPos;
                object ret = paramDataType switch
                {
                    ParamDataType.Integer => r.ReadInt32(),
                    ParamDataType.FsmInt when dataVersion == 1 => new FsmInt() { value = r.ReadInt32() },
                    ParamDataType.Enum => r.ReadInt32(),
                    ParamDataType.Boolean => r.ReadBoolean(),
                    ParamDataType.FsmBool when dataVersion == 1 => new FsmBool { value = r.ReadBoolean() },
                    ParamDataType.Float => r.ReadSingle(),
                    ParamDataType.FsmFloat when dataVersion == 1 => new FsmFloat { value = r.ReadSingle() },
                    ParamDataType.String => Encoding.UTF8.GetString(r.ReadBytes(paramByteDataSize)),
                    ParamDataType.FsmEvent when dataVersion == 1 => new FsmEvent { name = Encoding.UTF8.GetString(r.ReadBytes(paramByteDataSize)) },
                    ParamDataType.Vector2 => new Vector2 { x = r.ReadSingle(), y = r.ReadSingle() },
                    ParamDataType.FsmVector2 when dataVersion == 1 => new FsmVector2 { value = new Vector2 { x = r.ReadSingle(), y = r.ReadSingle() } },
                    ParamDataType.Vector3 => new Vector3 { x = r.ReadSingle(), y = r.ReadSingle(), z = r.ReadSingle() },
                    ParamDataType.FsmVector3 when dataVersion == 1 => new FsmVector3 { value = new Vector3 { x = r.ReadSingle(), y = r.ReadSingle(), z = r.ReadSingle() } },
                    ParamDataType.Quaternion => new Quaternion { x = r.ReadSingle(), y = r.ReadSingle(), z = r.ReadSingle(), w = r.ReadSingle() },
                    ParamDataType.FsmQuaternion when dataVersion == 1 => new FsmQuaternion { value = new Quaternion { x = r.ReadSingle(), y = r.ReadSingle(), z = r.ReadSingle(), w = r.ReadSingle() } },
                    ParamDataType.Color => new UnityColor { r = r.ReadSingle(), g = r.ReadSingle(), b = r.ReadSingle(), a = r.ReadSingle() },
                    ParamDataType.FsmColor when dataVersion == 1 => new FsmColor { value = new UnityColor { r = r.ReadSingle(), g = r.ReadSingle(), b = r.ReadSingle(), a = r.ReadSingle() } },
                    ParamDataType.Rect => new UnityRect { x = r.ReadSingle(), y = r.ReadSingle(), width = r.ReadSingle(), height = r.ReadSingle() },
                    ParamDataType.FsmRect when dataVersion == 1 => new FsmRect { value = new UnityRect { x = r.ReadSingle(), y = r.ReadSingle(), width = r.ReadSingle(), height = r.ReadSingle() } },
                    /////////////////////////////////////////////////////////
                   
                    ParamDataType.FsmBool when dataVersion > 1 => actionData.fsmBoolParams[paramDataPos],
                    ParamDataType.FsmInt when dataVersion > 1 => actionData.fsmIntParams[paramDataPos],
                    ParamDataType.FsmFloat when dataVersion > 1 => actionData.fsmFloatParams[paramDataPos],
                    ParamDataType.FsmVector2 when dataVersion > 1 => actionData.fsmVector2Params[paramDataPos],
                    ParamDataType.FsmVector3 when dataVersion > 1 => actionData.fsmVector3Params[paramDataPos],
                    ParamDataType.FsmQuaternion when dataVersion > 1 => actionData.fsmQuaternionParams[paramDataPos],
                    ParamDataType.FsmColor when dataVersion > 1 => actionData.fsmColorParams[paramDataPos],
                    ParamDataType.FsmRect when dataVersion > 1 => actionData.fsmRectParams[paramDataPos],
                    ///////////////////////////////////////////////////////// 
                    ParamDataType.FsmEnum => actionData.fsmEnumParams[paramDataPos],
                    ParamDataType.FsmGameObject => actionData.fsmGameObjectParams[paramDataPos],
                    ParamDataType.FsmOwnerDefault => actionData.fsmOwnerDefaultParams[paramDataPos],
                    ParamDataType.FsmObject => actionData.fsmObjectParams[paramDataPos],
                    ParamDataType.FsmVar => actionData.fsmVarParams[paramDataPos],
                    ParamDataType.FsmString => actionData.fsmStringParams[paramDataPos],
                    ParamDataType.FsmEvent => actionData.stringParams[paramDataPos],
                    ParamDataType.FsmEventTarget => actionData.fsmEventTargetParams[paramDataPos],
                    ParamDataType.FsmArray => actionData.fsmArrayParams[paramDataPos],
                    ParamDataType.ObjectReference => $"ObjRef([{actionData.unityObjectParams[paramDataPos]}])",
                    ParamDataType.FunctionCall => actionData.functionCallParams[paramDataPos],
                    ParamDataType.Array => actionData.GetFsmArray(ref index, dataVersion),
                    _ => $"[{paramDataType} not implemented]",
                };
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
            catch (Exception e)
            {
                return $"An exception was encountered: {e}";
            }
        }
    }
}

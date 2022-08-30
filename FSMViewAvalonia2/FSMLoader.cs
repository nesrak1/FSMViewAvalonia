using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FSMViewAvalonia2
{
    public class FSMLoader
    {
        private MainWindow window;
        private AssetsManager am;
        private AssetsFileInstance curFile;
        public FSMLoader(MainWindow window, AssetsManager am)
        {
            this.window = window;
            this.am = am;
        }

        public List<AssetInfo> LoadAllFSMsFromFile(string path)
        {
            curFile = am.LoadAssetsFile(path, true);
            am.UpdateDependencies();

            am.LoadClassDatabaseFromPackage(curFile.file.typeTree.unityVersion);

            AssetsFile file = curFile.file;
            AssetsFileTable table = curFile.table;

            //we read manually with binaryreader to speed up reading, but sometimes the dataVersion field won't exist
            //so here we read the template manually to see whether it exists and back up a bit if it doesn't
            string assemblyPath = Path.Combine(Path.GetDirectoryName(curFile.path), "Managed", "PlayMaker.dll");
            MonoDeserializer deserializer = new MonoDeserializer();
            deserializer.Read("PlayMakerFSM", MonoDeserializer.GetAssemblyWithDependencies(assemblyPath), file.header.format);
            bool hasDataField = deserializer.children[0].children[0].name == "dataVersion";
            
            return GetFSMInfos(file, table, hasDataField);
        }

        public FsmDataInstance LoadFSM(long id)
        {
            AssetFileInfoEx info = curFile.table.GetAssetInfo(id);
            AssetTypeValueField baseField = am.GetMonoBaseFieldCached(curFile, info, Path.Combine(Path.GetDirectoryName(curFile.path), "Managed"));
            AssetNameResolver namer = new AssetNameResolver(am, curFile);

            FsmDataInstance dataInstance = new FsmDataInstance();

            AssetTypeValueField fsm = baseField.Get("fsm");
            AssetTypeValueField states = fsm.Get("states");
            AssetTypeValueField events = fsm.Get("events");
            AssetTypeValueField variables = fsm.Get("variables");
            AssetTypeValueField globalTransitions = fsm.Get("globalTransitions");
            AssetTypeValueField dataVersionField = fsm.Get("dataVersion");

            dataInstance.fsmName = fsm.Get("name").GetValue().AsString();

            AssetTypeInstance goAti = am.GetExtAsset(curFile, baseField.Get("m_GameObject")).instance;
            if (goAti != null)
            {
                string m_Name = goAti.GetBaseField().Get("m_Name").GetValue().AsString();
                dataInstance.goName = m_Name;
            }

            if (dataVersionField.IsDummy())
            {
                dataInstance.dataVersion = fsm.Get("version").GetValue().AsInt() + 1;
            }
            else
            {
                dataInstance.dataVersion = dataVersionField.GetValue().AsInt();
            }

            dataInstance.states = new List<FsmStateData>();
            for (int i = 0; i < states.GetChildrenCount(); i++)
            {
                FsmStateData stateData = new FsmStateData();
                stateData.ActionData = new List<ActionScriptEntry>();
                stateData.state = new FsmState(namer, states[i]);
                stateData.node = new FsmNodeData(stateData.state);

                GetActionData(stateData.ActionData, stateData.state.actionData, dataInstance.dataVersion);

                dataInstance.states.Add(stateData);
            }

            dataInstance.events = new List<FsmEventData>();
            for (int i = 0; i < events.GetChildrenCount(); i++)
            {
                FsmEventData eventData = new FsmEventData();
                eventData.Global = events[i].Get("isGlobal").GetValue().AsBool();
                eventData.Name = events[i].Get("name").GetValue().AsString();

                dataInstance.events.Add(eventData);
            }

            dataInstance.variables = new List<FsmVariableData>();
            GetVariableValues(dataInstance.variables, namer, variables);

            dataInstance.globalTransitions = new List<FsmNodeData>();
            for (int i = 0; i < globalTransitions.GetChildrenCount(); i++)
            {
                AssetTypeValueField globalTransitionField = globalTransitions[i];
                FsmGlobalTransition globalTransition = new FsmGlobalTransition()
                {
                    fsmEvent = new FsmEvent(globalTransitionField.Get("fsmEvent")),
                    toState = globalTransitionField.Get("toState").GetValue().AsString(),
                    linkStyle = globalTransitionField.Get("linkStyle").GetValue().AsInt(),
                    linkConstraint = globalTransitionField.Get("linkConstraint").GetValue().AsInt(),
                    colorIndex = (byte)globalTransitionField.Get("colorIndex").GetValue().AsInt()
                };

                FsmNodeData node = new FsmNodeData(dataInstance, globalTransition);
                dataInstance.globalTransitions.Add(node);
            }

            //dataInstance.events = new List<FsmEventData>();
            //for (int i = 0; i < events.GetChildrenCount(); i++)
            //{
            //    FsmEventData eventData = new FsmEventData();
            //    AssetTypeValueField evt = events[i];
            //    eventData.Name = evt.Get("name").GetValue().AsString();
            //    eventData.Global = evt.Get("isGlobal").GetValue().AsBool();
            //}
            //
            //dataInstance.variables = new List<FsmVariableData>();
            //for (int i = 0; i < variables.GetChildrenCount(); i++)
            //{
            //    FsmVariableData variableData = new FsmVariableData();
            //    AssetTypeValueField vars = events[i];
            //}

            return dataInstance;
        }

        private List<AssetInfo> GetFSMInfos(AssetsFile file, AssetsFileTable table, bool hasDataField)
        {
            List<AssetInfo> assetInfos = new List<AssetInfo>();
            uint assetCount = table.assetFileInfoCount;

            foreach (AssetFileInfoEx info in table.assetFileInfo)
            {
                if (AssetHelper.GetScriptIndex(file, info) != 0xffff)
                {
                    AssetTypeValueField monoBf = am.GetTypeInstance(file, info).GetBaseField();
                    AssetExternal ext = am.GetExtAsset(curFile, monoBf.Get("m_Script"));
                    AssetTypeInstance scriptAti = am.GetExtAsset(curFile, monoBf.Get("m_Script")).instance;
                    AssetTypeInstance goAti = am.GetExtAsset(curFile, monoBf.Get("m_GameObject")).instance;

                    string m_Name = "Unnamed";
                    if (goAti != null)
                    {
                        m_Name = goAti.GetBaseField().Get("m_Name").GetValue().AsString();
                    }
                    string m_ClassName = scriptAti.GetBaseField().Get("m_ClassName").GetValue().AsString();

                    if (m_ClassName == "PlayMakerFSM")
                    {
                        AssetsFileReader reader = file.reader;

                        long oldPos = reader.BaseStream.Position;
                        reader.BaseStream.Position = info.absoluteFilePos;
                        reader.BaseStream.Position += 28;
                        uint length = reader.ReadUInt32();
                        reader.ReadBytes((int)length);

                        reader.Align();

                        reader.BaseStream.Position += 16;

                        if (!hasDataField)
                        {
                            reader.BaseStream.Position -= 4;
                        }

                        uint length2 = reader.ReadUInt32();
                        string fsmName = Encoding.UTF8.GetString(reader.ReadBytes((int)length2));
                        reader.BaseStream.Position = oldPos;

                        assetInfos.Add(new AssetInfo()
                        {
                            id = info.index,
                            size = info.curFileSize,
                            name = m_Name + "-" + fsmName
                        });
                    }
                    else if (m_ClassName == "FsmTemplate")
                    {
                        string m_MonoName = monoBf.Get("m_Name").GetValue().AsString();
                        AssetsFileReader reader = file.reader;

                        long oldPos = reader.BaseStream.Position;
                        reader.BaseStream.Position = info.absoluteFilePos;
                        reader.BaseStream.Position += 28;

                        // m_Name
                        reader.ReadCountStringInt32();
                        reader.Align();
                        // category
                        reader.ReadCountStringInt32();
                        reader.Align();

                        reader.BaseStream.Position += 16;

                        if (!hasDataField)
                        {
                            reader.BaseStream.Position -= 4;
                        }

                        string fsmName = reader.ReadCountStringInt32();
                        reader.BaseStream.Position = oldPos;

                        assetInfos.Add(new AssetInfo()
                        {
                            id = info.index,
                            size = info.curFileSize,
                            name = m_MonoName + " - " + fsmName + " (template)"
                        });
                    }
                }
            }

            assetInfos.Sort((x, y) => x.name.CompareTo(y.name));

            return assetInfos;
        }

        private void GetActionData(List<ActionScriptEntry> list, ActionData actionData, int dataVersion)
        {
            for (int i = 0; i < actionData.actionNames.Count; i++)
            {
                string actionName = actionData.actionNames[i];
                if (actionName.Contains("."))
                    actionName = actionName.Substring(actionName.LastIndexOf(".") + 1);

                int startIndex = actionData.actionStartIndex[i];
                int endIndex;
                if (i == actionData.actionNames.Count - 1)
                    endIndex = actionData.paramDataType.Count;
                else
                    endIndex = actionData.actionStartIndex[i + 1];

                ActionScriptEntry entry = new ActionScriptEntry();
                entry.Values = new List<Tuple<string, object>>();
                for (int j = startIndex; j < endIndex; j++)
                {
                    string paramName = actionData.paramName[j];
                    object obj = ActionReader.GetFsmObject(actionData, j, dataVersion);

                    entry.Values.Add(new Tuple<string, object>(paramName, obj));
                }

                entry.Name = actionName;
                entry.Enabled = actionData.actionEnabled[i];

                list.Add(entry);
            }
        }

        private void GetVariableValues(List<FsmVariableData> varData, AssetNameResolver namer, AssetTypeValueField variables)
        {
            AssetTypeValueField floatVariables = variables.Get("floatVariables");
            AssetTypeValueField intVariables = variables.Get("intVariables");
            AssetTypeValueField boolVariables = variables.Get("boolVariables");
            AssetTypeValueField stringVariables = variables.Get("stringVariables");
            AssetTypeValueField vector2Variables = variables.Get("vector2Variables");
            AssetTypeValueField vector3Variables = variables.Get("vector3Variables");
            AssetTypeValueField colorVariables = variables.Get("colorVariables");
            AssetTypeValueField rectVariables = variables.Get("rectVariables");
            AssetTypeValueField quaternionVariables = variables.Get("quaternionVariables");
            AssetTypeValueField gameObjectVariables = variables.Get("gameObjectVariables");
            AssetTypeValueField objectVariables = variables.Get("objectVariables");
            AssetTypeValueField materialVariables = variables.Get("materialVariables");
            AssetTypeValueField textureVariables = variables.Get("textureVariables");
            AssetTypeValueField arrayVariables = variables.Get("arrayVariables");
            AssetTypeValueField enumVariables = variables.Get("enumVariables");
            FsmVariableData floats = new FsmVariableData() { Type = "Floats", Values = new List<Tuple<string, object>>() };
            varData.Add(floats);
            for (int i = 0; i < floatVariables.GetValue().AsArray().size; i++)
            {
                string name = floatVariables.Get(i).Get("name").GetValue().AsString();
                object value = floatVariables.Get(i).Get("value").GetValue().AsFloat();
                floats.Values.Add(new Tuple<string, object>(name, value));
            }
            FsmVariableData ints = new FsmVariableData() { Type = "Ints", Values = new List<Tuple<string, object>>() };
            varData.Add(ints);
            for (int i = 0; i < intVariables.GetValue().AsArray().size; i++)
            {
                string name = intVariables.Get(i).Get("name").GetValue().AsString();
                object value = intVariables.Get(i).Get("value").GetValue().AsInt();
                ints.Values.Add(new Tuple<string, object>(name, value));
            }
            FsmVariableData bools = new FsmVariableData() { Type = "Bools", Values = new List<Tuple<string, object>>() };
            varData.Add(bools);
            for (int i = 0; i < boolVariables.GetValue().AsArray().size; i++)
            {
                string name = boolVariables.Get(i).Get("name").GetValue().AsString();
                object value = boolVariables.Get(i).Get("value").GetValue().AsBool().ToString();
                bools.Values.Add(new Tuple<string, object>(name, value));
            }
            FsmVariableData strings = new FsmVariableData() { Type = "Strings", Values = new List<Tuple<string, object>>() };
            varData.Add(strings);
            for (int i = 0; i < stringVariables.GetValue().AsArray().size; i++)
            {
                string name = stringVariables.Get(i).Get("name").GetValue().AsString();
                object value = stringVariables.Get(i).Get("value").GetValue().AsString();
                strings.Values.Add(new Tuple<string, object>(name, value));
            }
            FsmVariableData vector2s = new FsmVariableData() { Type = "Vector2s", Values = new List<Tuple<string, object>>() };
            varData.Add(vector2s);
            for (int i = 0; i < vector2Variables.GetValue().AsArray().size; i++)
            {
                string name = vector2Variables.Get(i).Get("name").GetValue().AsString();
                AssetTypeValueField vector2 = vector2Variables.Get(i).Get("value");
                object value = new Vector2(vector2);
                vector2s.Values.Add(new Tuple<string, object>(name, value));
            }
            FsmVariableData vector3s = new FsmVariableData() { Type = "Vector3s", Values = new List<Tuple<string, object>>() };
            varData.Add(vector3s);
            for (int i = 0; i < vector3Variables.GetValue().AsArray().size; i++)
            {
                string name = vector3Variables.Get(i).Get("name").GetValue().AsString();
                AssetTypeValueField vector3 = vector3Variables.Get(i).Get("value");
                object value = new Vector2(vector3);
                vector3s.Values.Add(new Tuple<string, object>(name, value));
            }
            FsmVariableData colors = new FsmVariableData() { Type = "Colors", Values = new List<Tuple<string, object>>() };
            varData.Add(colors);
            for (int i = 0; i < colorVariables.GetValue().AsArray().size; i++)
            {
                string name = colorVariables.Get(i).Get("name").GetValue().AsString();
                AssetTypeValueField color = colorVariables.Get(i).Get("value");
                object value = new UnityColor(color);
                colors.Values.Add(new Tuple<string, object>(name, value));
            }
            FsmVariableData rects = new FsmVariableData() { Type = "Rects", Values = new List<Tuple<string, object>>() };
            varData.Add(rects);
            for (int i = 0; i < rectVariables.GetValue().AsArray().size; i++)
            {
                string name = rectVariables.Get(i).Get("name").GetValue().AsString();
                AssetTypeValueField rect = rectVariables.Get(i).Get("value");
                object value = new UnityRect(rect);
                rects.Values.Add(new Tuple<string, object>(name, value));
            }
            FsmVariableData quaternions = new FsmVariableData() { Type = "Quaternions", Values = new List<Tuple<string, object>>() };
            varData.Add(quaternions);
            for (int i = 0; i < quaternionVariables.GetValue().AsArray().size; i++)
            {
                string name = quaternionVariables.Get(i).Get("name").GetValue().AsString();
                AssetTypeValueField quaternion = quaternionVariables.Get(i).Get("value");
                object value = new Quaternion(quaternion);
                quaternions.Values.Add(new Tuple<string, object>(name, value));
            }
            string[] pptrTypeHeaders = new[] { "GameObjects", "Objects", "Materials", "Textures" };
            AssetTypeValueField[] pptrTypeFields = new[] { gameObjectVariables, objectVariables, materialVariables, textureVariables };
            for (int j = 0; j < pptrTypeHeaders.Length; j++)
            {
                string header = pptrTypeHeaders[j];
                AssetTypeValueField field = pptrTypeFields[j];

                if (field.IsDummy())
                    continue;

                FsmVariableData genericData = new FsmVariableData() { Type = header, Values = new List<Tuple<string, object>>() };
                varData.Add(genericData);
                for (int i = 0; i < field.GetValue().AsArray().size; i++)
                {
                    string name = field.Get(i).Get("name").GetValue().AsString();
                    AssetTypeValueField valueField = field.Get(i).Get("value");
                    NamedAssetPPtr pptr = StructUtil.ReadNamedAssetPPtr(namer, valueField);
                    object value;
                    if (pptr.pathID == 0)
                        value = "[null]";
                    else
                        value = pptr;
                    genericData.Values.Add(new Tuple<string, object>(name, value));
                }
            }
        }

        public List<SceneInfo> LoadSceneList(string folder)
        {
            string ggmPath = Path.Combine(folder, "globalgamemanagers");
            AssetsFileInstance ggm = am.LoadAssetsFile(ggmPath, false);
            am.LoadClassDatabaseFromPackage(ggm.file.typeTree.unityVersion);

            AssetFileInfoEx buildSettingsInfo = ggm.table.GetAssetsOfType(0x8D)[0];
            AssetTypeValueField buildSettings = am.GetTypeInstance(ggm, buildSettingsInfo).GetBaseField();
            AssetTypeValueField scenes = buildSettings.Get("scenes").Get("Array");
            int sceneCount = scenes.GetValue().AsArray().size;

            List<SceneInfo> sceneInfos = new List<SceneInfo>();
            for (int i = 0; i < sceneCount; i++)
            {
                sceneInfos.Add(new SceneInfo()
                {
                    id = i,
                    name = scenes[i].GetValue().AsString(),
                    level = true
                });
            }
            for (int i = 0; i < sceneCount; i++)
            {
                sceneInfos.Add(new SceneInfo()
                {
                    id = i,
                    name = scenes[i].GetValue().AsString(),
                    level = false
                });
            }
            return sceneInfos;
        }
    }
}

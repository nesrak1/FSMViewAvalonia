using Path = System.IO.Path;

namespace FSMViewAvalonia2
{
    public class FSMLoader
    {
        public static AssemblyDefinition mainAssembly;
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

            if(mainAssembly == null)
            {
                var map = Path.Combine(Path.GetDirectoryName(curFile.path), "Managed", "Assembly-CSharp.dll");
                mainAssembly = AssemblyDefinition.ReadAssembly(map, new()
                {
                    AssemblyResolver = new AssemblyResolver(Path.Combine(Path.GetDirectoryName(curFile.path), "Managed"))
                });
            }
            //we read manually with binaryreader to speed up reading, but sometimes the dataVersion field won't exist
            //so here we read the template manually to see whether it exists and back up a bit if it doesn't
            string assemblyPath = Path.Combine(Path.GetDirectoryName(curFile.path), "Managed", "PlayMaker.dll");
            MonoDeserializer deserializer = new();
            deserializer.Read("PlayMakerFSM", MonoDeserializer.GetAssemblyWithDependencies(assemblyPath),
                new(file.typeTree.unityVersion));
            bool hasDataField = deserializer.children[0].children[0].name == "dataVersion";
            
            return GetFSMInfos(file, table, curFile, hasDataField);
        }
        public FsmDataInstance LoadFSMWithAssets(long id, AssetInfo assetInfo)
        {
            AssetFileInfoEx info = curFile.table.GetAssetInfo(id);
            AssetTypeValueField baseField = am.GetMonoBaseFieldCached(curFile, info, Path.Combine(Path.GetDirectoryName(curFile.path), "Managed"));
            AssetNameResolver namer = new(am, curFile);
            AssetTypeValueField fsm = baseField.Get("fsm");
            return LoadFSM(assetInfo, new AssetsDataProvider(fsm, namer));
        }
        public FsmDataInstance LoadJsonFSM(string text, AssetInfo assetInfo)
        {
            assetInfo.providerType = AssetInfo.DataProviderType.Json;
            return LoadFSM(assetInfo, new JsonDataProvider(JToken.Parse(text)));
        }
        public FsmDataInstance LoadFSM(AssetInfo assetInfo, IDataProvider fsm)
        {
            FsmDataInstance dataInstance = new()
            {
                info = assetInfo
            };


            var states = fsm.Get<IDataProvider[]>("states");
            var events = fsm.Get<IDataProvider[]>("events");
            var variables = fsm.Get<IDataProvider>("variables");
            var globalTransitions = fsm.Get<IDataProvider[]>("globalTransitions");
            var dataVersionField = fsm.Get<IDataProvider>("dataVersion");

			string startState = fsm.Get<string>("startState");

            dataInstance.fsmName = fsm.Get<string>("name");

            dataInstance.goName = assetInfo.nameBase;

            if (dataVersionField == null)
            {
                dataInstance.dataVersion = fsm.Get<int>("version") + 1;
            }
            else
            {
                dataInstance.dataVersion = dataVersionField.As<int>();
            }

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
            foreach(var v in dataInstance.variables.SelectMany(x => x.Values).Select(x => x.Item1))
            {
                dataInstance.variableNames.Add(v);
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
					dataInstance.startState = stateData;

                GetActionData(stateData.ActionData, stateData.state.actionData, dataInstance.dataVersion, stateData.state, dataInstance);

                dataInstance.states.Add(stateData);
            }


            dataInstance.globalTransitions = new List<FsmNodeData>();
            for (int i = 0; i < globalTransitions.Length; i++)
            {
                var globalTransitionField = globalTransitions[i];
                FsmGlobalTransition globalTransition = new()
                {
                    fsmEvent = new FsmEvent(globalTransitionField.Get<IDataProvider>("fsmEvent")),
                    toState = globalTransitionField.Get<string>("toState"),
                    linkStyle = globalTransitionField.Get<int>("linkStyle"),
                    linkConstraint = globalTransitionField.Get<int>("linkConstraint"),
                    colorIndex = (byte)globalTransitionField.Get<int>("colorIndex")
                };

                FsmNodeData node = new(dataInstance, globalTransition);
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
        public string GetGameObjectPath(AssetTypeInstance goAti)
        {
            try
            {
                StringBuilder pathBuilder = new();

                AssetTypeInstance c_Transform = am.GetExtAsset(curFile, goAti.GetBaseField().Get("m_Component").Get(0).Get(0).Get(0)).instance;
                while (true)
                {
                    var father = c_Transform.GetBaseField().children.FirstOrDefault(x => x.GetName() == "m_Father");
                    if (father == null) break;
                    c_Transform = am.GetExtAsset(curFile, father).instance;
                    if (c_Transform == null) break;
                    var m_GameObject = am.GetExtAsset(curFile, c_Transform.GetBaseField().Get("m_GameObject")).instance;
                    string name = m_GameObject.GetBaseField().Get("m_Name").GetValue().AsString();
                    pathBuilder.Insert(0, name + "/");
                }
                return pathBuilder.ToString();
            }catch(Exception)
            {
                return "";
            }
        }

        private List<AssetInfo> GetFSMInfos(AssetsFile file, AssetsFileTable table, AssetsFileInstance assetsFile, bool hasDataField)
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
                    var goAE = am.GetExtAsset(curFile, monoBf.Get("m_GameObject"));
                    AssetTypeInstance goAti = goAE.instance;

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
                            name = m_Name + "-" + fsmName,
                            path = GetGameObjectPath(goAti),
                            goId = goAE.info.index,
                            fsmId = info.index,
                            assetFile = assetsFile.path,
                            nameBase = m_Name
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
                            name = m_MonoName + "-" + fsmName + " (template)",
                            path = "",
                            goId = 0,
                            fsmId = info.index,
                            assetFile = assetsFile.path,
                            nameBase = m_Name
                        });
                    }
                }
            }

            assetInfos.Sort((x, y) => x.name.CompareTo(y.name));

            return assetInfos;
        }

        private static void GetActionData(List<IActionScriptEntry> list, ActionData actionData, int dataVersion, FsmState state, FsmDataInstance inst)
        {
            for (int i = 0; i < actionData.actionNames.Count; i++)
            {
                list.Add(new FsmStateAction(actionData, i, dataVersion, state, inst));
            }
        }

        private static void GetVariableValues(List<FsmVariableData> varData, IDataProvider variables)
        {
            var floatVariables = variables.Get<IDataProvider[]>("floatVariables");
            var intVariables = variables.Get<IDataProvider[]>("intVariables");
            var boolVariables = variables.Get<IDataProvider[]>("boolVariables");
            var stringVariables = variables.Get<IDataProvider[]>("stringVariables");
            var vector2Variables = variables.Get<IDataProvider[]>("vector2Variables");
            var vector3Variables = variables.Get<IDataProvider[]>("vector3Variables");
            var colorVariables = variables.Get<IDataProvider[]>("colorVariables");
            var rectVariables = variables.Get<IDataProvider[]>("rectVariables");
            var quaternionVariables = variables.Get<IDataProvider[]>("quaternionVariables");
            var gameObjectVariables = variables.Get<IDataProvider[]>("gameObjectVariables");
            var objectVariables = variables.Get<IDataProvider[]>("objectVariables");
            var materialVariables = variables.Get<IDataProvider[]>("materialVariables");
            var textureVariables = variables.Get<IDataProvider[]>("textureVariables");
            var arrayVariables = variables.Get<IDataProvider[]>("arrayVariables");
            var enumVariables = variables.Get<IDataProvider[]>("enumVariables");

            FsmVariableData enums = new() { Type = "Enums", Values = new List<Tuple<string, object>>() };
            varData.Add(enums);
            for (int i = 0; i < enumVariables.Length; i++)
            {
                string name = enumVariables[i].Get<string>("name");
                object value = enumVariables[i].Get<int>("value");
                enums.Values.Add(new Tuple<string, object>(name, value));
            }

            FsmVariableData arrays = new() { Type = "Arrays", Values = new List<Tuple<string, object>>() };
            varData.Add(arrays);
            for (int i = 0; i < arrayVariables.Length; i++)
            {
                var arr = new FsmArray(arrayVariables[i]);
                arrays.Values.Add(new(arr.name, arr));
            }
            FsmVariableData floats = new() { Type = "Floats", Values = new List<Tuple<string, object>>() };
            varData.Add(floats);
            for (int i = 0; i < floatVariables.Length; i++)
            {
                string name = floatVariables[i].Get<string>("name");
                object value = floatVariables[i].Get<float>("value");
                floats.Values.Add(new Tuple<string, object>(name, value));
            }
            FsmVariableData ints = new() { Type = "Ints", Values = new List<Tuple<string, object>>() };
            varData.Add(ints);
            for (int i = 0; i < intVariables.Length; i++)
            {
                string name = intVariables[i].Get<string>("name");
                object value = intVariables[i].Get<int>("value");
                ints.Values.Add(new Tuple<string, object>(name, value));
            }
            FsmVariableData bools = new() { Type = "Bools", Values = new List<Tuple<string, object>>() };
            varData.Add(bools);
            for (int i = 0; i < boolVariables.Length; i++)
            {
                string name = boolVariables[i].Get<string>("name");
                object value = boolVariables[i].Get<bool>("value");
                bools.Values.Add(new Tuple<string, object>(name, value));
            }
            FsmVariableData strings = new() { Type = "Strings", Values = new List<Tuple<string, object>>() };
            varData.Add(strings);
            for (int i = 0; i < stringVariables.Length; i++)
            {
                string name = stringVariables[i].Get<string>("name");
                object value = stringVariables[i].Get<string>("value");
                strings.Values.Add(new Tuple<string, object>(name, value));
            }
            FsmVariableData vector2s = new() { Type = "Vector2s", Values = new List<Tuple<string, object>>() };
            varData.Add(vector2s);
            for (int i = 0; i < vector2Variables.Length; i++)
            {
                string name = vector2Variables[i].Get<string>("name");
                var vector2 = vector2Variables[i].Get<IDataProvider>("value");
                object value = new Vector2(vector2);
                vector2s.Values.Add(new Tuple<string, object>(name, value));
            }
            FsmVariableData vector3s = new() { Type = "Vector3s", Values = new List<Tuple<string, object>>() };
            varData.Add(vector3s);
            for (int i = 0; i < vector3Variables.Length; i++)
            {
                string name = vector3Variables[i].Get<string>("name");
                var vector3 = vector3Variables[i].Get<IDataProvider>("value");
                object value = new Vector2(vector3);
                vector3s.Values.Add(new Tuple<string, object>(name, value));
            }
            FsmVariableData colors = new() { Type = "Colors", Values = new List<Tuple<string, object>>() };
            varData.Add(colors);
            for (int i = 0; i < colorVariables.Length; i++)
            {
                string name = colorVariables[i].Get<string>("name");
                var color = colorVariables[i].Get<IDataProvider>("value");
                object value = new UnityColor(color);
                colors.Values.Add(new Tuple<string, object>(name, value));
            }
            FsmVariableData rects = new() { Type = "Rects", Values = new List<Tuple<string, object>>() };
            varData.Add(rects);
            for (int i = 0; i < rectVariables.Length; i++)
            {
                string name = rectVariables[i].Get<string>("name");
                var rect = rectVariables[i].Get<IDataProvider>("value");
                object value = new UnityRect(rect);
                rects.Values.Add(new Tuple<string, object>(name, value));
            }
            FsmVariableData quaternions = new() { Type = "Quaternions", Values = new List<Tuple<string, object>>() };
            varData.Add(quaternions);
            for (int i = 0; i < quaternionVariables.Length; i++)
            {
                string name = quaternionVariables[i].Get<string>("name");
                var quaternion = quaternionVariables[i].Get<IDataProvider>("value");
                object value = new Quaternion(quaternion);
                quaternions.Values.Add(new Tuple<string, object>(name, value));
            }
            string[] pptrTypeHeaders = new[] { "GameObjects", "Objects", "Materials", "Textures" };
            IDataProvider[][] pptrTypeFields = new[] { gameObjectVariables, objectVariables, materialVariables, textureVariables };
            for (int j = 0; j < pptrTypeHeaders.Length; j++)
            {
                string header = pptrTypeHeaders[j];
                var field = pptrTypeFields[j];

                if (field == null)
                    continue;

                FsmVariableData genericData = new() { Type = header, Values = new List<Tuple<string, object>>() };
                varData.Add(genericData);
                for (int i = 0; i < field.Length; i++)
                {
                    string name = field[i].Get<string>("name");
                    var valueField = field[i].Get<IDataProvider>("value");
                    INamedAssetProvider pptr = valueField?.As<INamedAssetProvider>();
                    object value;
                    if (pptr?.isNull ?? true)
                        value = "[null]";
                    else
                        value = header == "GameObjects" ? (object)(new GameObjectPPtrHolder() { pptr = pptr }) : pptr;
                    genericData.Values.Add(new Tuple<string, object>(name, value));
                }
            }
        }

        public List<SceneInfo> LoadSceneList()
        {
            var buildSettings = GlobalGameManagers.instance.GetAsset(AssetClassID.BuildSettings).GetBaseField();
            //AssetFileInfoEx buildSettingsInfo = ggm.table.GetAssetsOfType(0x8D)[0];
            //AssetTypeValueField buildSettings = am.GetTypeInstance(ggm, buildSettingsInfo).GetBaseField();
            AssetTypeValueField scenes = buildSettings.Get("scenes").Get("Array");
            int sceneCount = scenes.GetValue().AsArray().size;

            List<SceneInfo> sceneInfos = new();
            for (int i = 0; i < sceneCount; i++)
            {
                sceneInfos.Add(new SceneInfo()
                {
                    id = i,
                    name = scenes[i].GetValue().AsString() + " (level" + i + ")",
                    level = true
                });
            }
            for (int i = 0; i < sceneCount; i++)
            {
                sceneInfos.Add(new SceneInfo()
                {
                    id = i,
                    name = scenes[i].GetValue().AsString()  + " (sharedassets" + i + ".assets)",
                    level = false
                });
            }
            return sceneInfos;
        }
    }
}

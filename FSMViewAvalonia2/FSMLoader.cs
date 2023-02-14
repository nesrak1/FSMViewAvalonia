using Path = System.IO.Path;

namespace FSMViewAvalonia2;

public class FSMLoader
{
    public static AssemblyDefinition mainAssembly;
    private readonly MainWindow window;
    private readonly AssetsManager am;
    public FSMLoader(MainWindow window, AssetsManager am)
    {
        this.window = window;
        this.am = am;
    }
    public List<AssetInfo> LoadAllFSMsFromBundle(string path, bool loadAsDep = false)
    {
        BundleFileInstance file = am.LoadBundleFile(path, true);
        _ = am.LoadClassDatabaseFromPackage(file.file.Header.EngineVersion);
        if (file.file.DataIsCompressed)
        {
            Stream stream = Config.config.option_extraLAMZABOnTempFile ?
                new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite,
                FileShare.None, 4096, FileOptions.DeleteOnClose) :
                new MemoryStream();
            file.file.Unpack(new(stream));
            file.file.Close();
            stream.Position = 0;
            file.file = new();
            file.file.Read(new(stream));
        }
        return file.file.GetAllFileNames()
            .Select(x =>
            {
                try
                {
                    return am.LoadAssetsFileFromBundle(file, x, true);
                } catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                    throw;
                }
            }
                )
            .Where(x => x != null)
            .SelectMany(x => GetFSMInfos(x, false))
            .ToList();
    }
    public List<AssetInfo> LoadAllFSMsFromFile(string path, bool loadAsDep = false)
    {
        bool isLevel = Path.GetFileNameWithoutExtension(path).StartsWith("level");
        AssetsFileInstance curFile = am.LoadAssetsFile(path, true);
        am.LoadDependencies(curFile);

        _ = am.LoadClassDatabaseFromPackage(curFile.file.Metadata.UnityVersion);

        AssetsFile file = curFile.file;

        List<AssetInfo> result = GetFSMInfos(curFile, loadAsDep);
        if(isLevel && Config.config.option_includeSharedassets)
        {
            foreach(AssetsFileExternal dep in curFile.file.Metadata.Externals.OrderBy(
                a =>
                {
                    string fn = Path.GetFileNameWithoutExtension(a.PathName);
                    return !fn.StartsWith("sharedassets") ? 0 : int.TryParse(fn.AsSpan("sharedassets".Length), out int id) ? id : 0;
                }
                )
                )
            {
                if (Path.GetFileNameWithoutExtension(dep.PathName) == "resources")
                {
                    continue;
                }

                result.AddRange(LoadAllFSMsFromFile(
                    Path.Combine(Path.GetDirectoryName(curFile.path), dep.PathName), true));
            }
        }

        return result;
    }
    public FsmDataInstance LoadFSMWithAssets(long id, AssetInfoUnity assetInfo)
    {
        //AssetFileInfo info = assetInfo.assetFI.file.GetAssetInfo(id);
        //AssetTypeValueField baseField = am.GetBaseField(assetInfo.assetFI, info);
        AssetNameResolver namer = new(am, assetInfo.assetFI);
        AssetTypeValueField fsm = assetInfo.templateField.MakeValue(assetInfo.assetFI.file.Reader,
            assetInfo.assetInfo.AbsoluteByteStart)["fsm"];
        return LoadFSM(assetInfo, new AssetsDataProvider(fsm, namer));
    }
    public FsmDataInstance LoadJsonFSM(string text, AssetInfo assetInfo) => assetInfo.ProviderType != AssetInfo.DataProviderType.Json
            ? throw new NotSupportedException()
            : LoadFSM(assetInfo, new JsonDataProvider(JToken.Parse(text)));
    public FsmDataInstance LoadFSM(AssetInfo assetInfo, IDataProvider fsm)
    {
        FsmDataInstance dataInstance = new()
        {
            info = assetInfo
        };


        IDataProvider[] states = fsm.Get<IDataProvider[]>("states");
        IDataProvider[] events = fsm.Get<IDataProvider[]>("events");
        IDataProvider variables = fsm.Get<IDataProvider>("variables");
        IDataProvider[] globalTransitions = fsm.Get<IDataProvider[]>("globalTransitions");
        IDataProvider dataVersionField = fsm.Get<IDataProvider>("dataVersion");

        string startState = fsm.Get<string>("startState");

        dataInstance.fsmName = fsm.Get<string>("name");

        dataInstance.goName = assetInfo.nameBase;

        dataInstance.dataVersion = dataVersionField == null ? fsm.Get<int>("version") + 1 : dataVersionField.As<int>();

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
        foreach (string v in dataInstance.variables.SelectMany(x => x.Values).Select(x => x.Item1))
        {
            _ = dataInstance.variableNames.Add(v);
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
            {
                dataInstance.startState = stateData;
            }

            GetActionData(stateData.ActionData, stateData.state.actionData, dataInstance.dataVersion, stateData.state, dataInstance);

            dataInstance.states.Add(stateData);
        }


        dataInstance.globalTransitions = new List<FsmNodeData>();
        for (int i = 0; i < globalTransitions.Length; i++)
        {
            IDataProvider globalTransitionField = globalTransitions[i];
            FsmGlobalTransition globalTransition = new()
            {
                fsmEvent = new FsmEvent(globalTransitionField.Get<IDataProvider>("fsmEvent")),
                toState = globalTransitionField.Get<string>("toState"),
                linkStyle = globalTransitionField.Get<int>("linkStyle"),
                linkConstraint = globalTransitionField.Get<int>("linkConstraint"),
                colorIndex = (byte) globalTransitionField.Get<int>("colorIndex")
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
    public string GetGameObjectPath(AssetTypeValueField goAti, AssetsFileInstance curFile)
    {
        try
        {
            StringBuilder pathBuilder = new();

            AssetTypeValueField c_Transform = am.GetExtAsset(curFile, goAti.Get("m_Component").Get(0).Get(0).Get(0)).baseField;
            while (true)
            {
                AssetTypeValueField father = c_Transform.Children.FirstOrDefault(x => x.FieldName == "m_Father");
                if (father == null)
                {
                    break;
                }

                c_Transform = am.GetExtAsset(curFile, father).baseField;
                if (c_Transform == null)
                {
                    break;
                }

                AssetTypeValueField m_GameObject = am.GetExtAsset(curFile, c_Transform.Get("m_GameObject")).baseField;
                string name = m_GameObject.Get("m_Name").AsString;
                _ = pathBuilder.Insert(0, name + "/");
            }

            return pathBuilder.ToString();
        } catch (Exception)
        {
            return "";
        }
    }

    private List<AssetInfo> GetFSMInfos(
        AssetsFileInstance assetsFile, bool loadAsDep
        )
    {
        AssetTypeTemplateField t_pmBS = null;
        AssetTypeTemplateField t_pmBS_data = null;
        AssetTypeTemplateField t_fsmTemplateBS = null;
        AssetTypeTemplateField t_fsmTemplateBS_data = null;
        AssetTypeTemplateField t_fsm = null;


        AssetsFile file = assetsFile.file;
        var assetInfos = new List<AssetInfo>();
        int assetCount = file.AssetInfos.Count;

        foreach (AssetFileInfo info in file.GetAssetsOfType(AssetClassID.MonoBehaviour))
        {
            AssetTypeTemplateField t_monoBF = am.GetTemplateBaseField(assetsFile, info);
            AssetTypeValueField monoBf = am.GetBaseField(assetsFile, info);
            AssetExternal ext = am.GetExtAsset(assetsFile, monoBf.Get("m_Script"));
            if (ext.baseField == null)
            {
                continue;
            }

            AssetTypeValueField scriptAti = ext.baseField;
            string m_ClassName = scriptAti.Get("m_ClassName").AsString;

            if (m_ClassName != "PlayMakerFSM" && m_ClassName != "FsmTemplate")
            {
                continue;
            }

            AssetExternal goAE = am.GetExtAsset(assetsFile, monoBf.Get("m_GameObject"));
            AssetTypeValueField goAti = goAE.baseField;

            string m_Name = "Unnamed";
            if (goAti != null)
            {
                m_Name = goAti.Get("m_Name").AsString;
            }

            if(t_fsm == null)
            {
                t_fsm = FSMAssetHelper.mono.GetTemplateField(new()
                {
                    Children = new()
                }, "PlayMaker", "HutongGames.PlayMaker", "Fsm", new(file.Metadata.UnityVersion));
                AssetTypeTemplateField name = t_fsm.Children.First(x => x.Name == "name");
                t_fsm.Children = t_fsm.Children.TakeWhile(x => x.Name != "name").ToList();
                t_fsm.Children.Add(name);
                t_fsm.Name = "fsm";
            }

            if (m_ClassName == "PlayMakerFSM")
            {
                if (t_pmBS == null)
                {
                    t_pmBS = FSMAssetHelper.mono.GetTemplateField(new()
                    {
                        Children = t_monoBF.Children.ToArray().ToList()
                    }, "PlayMaker", "",
                                       "PlayMakerFSM", new(file.Metadata.UnityVersion));
                    t_pmBS.Children = t_pmBS.Children.TakeWhile(x => x.Name != "fsm")
                        .Append(t_fsm).ToList();
                }

                t_pmBS_data ??= FSMAssetHelper.mono.GetTemplateField(new()
                    {
                        Children = t_monoBF.Children.ToArray().ToList()
                    }, "PlayMaker", "",
                                       "PlayMakerFSM", new(file.Metadata.UnityVersion));

                AssetTypeValueField pmBf = t_pmBS.MakeValue(file.Reader, info.AbsoluteByteStart);
                string fsmName = pmBf["fsm"]["name"].AsString;

                assetInfos.Add(new AssetInfoUnity()
                {
                    id = info.PathId,
                    templateField = t_pmBS_data,
                    assetInfo = info,
                    size = info.ByteSize,
                    name = m_Name + "-" + fsmName,
                    path = GetGameObjectPath(goAti, assetsFile),
                    goId = goAE.info.PathId,
                    fsmId = info.PathId,
                    assetFile = assetsFile.path,
                    nameBase = m_Name,
                    assetFI = assetsFile,
                    loadAsDep = loadAsDep
                });
            } else if (m_ClassName == "FsmTemplate") //TODO
            {
                if (t_fsmTemplateBS == null)
                {
                    t_fsmTemplateBS = FSMAssetHelper.mono.GetTemplateField(new()
                    {
                        Children = t_monoBF.Children.ToArray().ToList()
                    }, "PlayMaker", "",
                                       "FsmTemplate", new(file.Metadata.UnityVersion));
                    t_fsmTemplateBS.Children = t_fsmTemplateBS.Children.TakeWhile(x => x.Name != "fsm")
                        .Append(t_fsm).ToList();
                }

                t_fsmTemplateBS_data ??= FSMAssetHelper.mono.GetTemplateField(new()
                    {
                        Children = t_monoBF.Children.ToArray().ToList()
                    }, "PlayMaker", "",
                                       "FsmTemplate", new(file.Metadata.UnityVersion));

                AssetTypeValueField pmBf = t_fsmTemplateBS.MakeValue(file.Reader, info.AbsoluteByteStart);
                string fsmName = pmBf["fsm"]["name"].AsString;
                string m_MonoName = monoBf["m_Name"].AsString;

                assetInfos.Add(new AssetInfoUnity()
                {
                    id = info.PathId,
                    templateField = t_fsmTemplateBS_data,
                    assetInfo = info,
                    size = info.ByteSize,
                    name = m_MonoName + "-" + fsmName + " (template)",
                    path = "",
                    goId = 0,
                    fsmId = info.PathId,
                    assetFile = assetsFile.path,
                    nameBase = m_Name,
                    assetFI = assetsFile,
                    loadAsDep = loadAsDep
                });
            }

        }

        assetInfos.Sort((x, y) => x.Name.CompareTo(y.Name));

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
            IDataProvider vector2 = vector2Variables[i].Get<IDataProvider>("value");
            object value = new Vector2(vector2);
            vector2s.Values.Add(new Tuple<string, object>(name, value));
        }

        FsmVariableData vector3s = new() { Type = "Vector3s", Values = new List<Tuple<string, object>>() };
        varData.Add(vector3s);
        for (int i = 0; i < vector3Variables.Length; i++)
        {
            string name = vector3Variables[i].Get<string>("name");
            IDataProvider vector3 = vector3Variables[i].Get<IDataProvider>("value");
            object value = new Vector2(vector3);
            vector3s.Values.Add(new Tuple<string, object>(name, value));
        }

        FsmVariableData colors = new() { Type = "Colors", Values = new List<Tuple<string, object>>() };
        varData.Add(colors);
        for (int i = 0; i < colorVariables.Length; i++)
        {
            string name = colorVariables[i].Get<string>("name");
            IDataProvider color = colorVariables[i].Get<IDataProvider>("value");
            object value = new UnityColor(color);
            colors.Values.Add(new Tuple<string, object>(name, value));
        }

        FsmVariableData rects = new() { Type = "Rects", Values = new List<Tuple<string, object>>() };
        varData.Add(rects);
        for (int i = 0; i < rectVariables.Length; i++)
        {
            string name = rectVariables[i].Get<string>("name");
            IDataProvider rect = rectVariables[i].Get<IDataProvider>("value");
            object value = new UnityRect(rect);
            rects.Values.Add(new Tuple<string, object>(name, value));
        }

        FsmVariableData quaternions = new() { Type = "Quaternions", Values = new List<Tuple<string, object>>() };
        varData.Add(quaternions);
        for (int i = 0; i < quaternionVariables.Length; i++)
        {
            string name = quaternionVariables[i].Get<string>("name");
            IDataProvider quaternion = quaternionVariables[i].Get<IDataProvider>("value");
            object value = new Quaternion(quaternion);
            quaternions.Values.Add(new Tuple<string, object>(name, value));
        }

        string[] pptrTypeHeaders = new[] { "GameObjects", "Objects", "Materials", "Textures" };
        IDataProvider[][] pptrTypeFields = new[] { gameObjectVariables, objectVariables, materialVariables, textureVariables };
        for (int j = 0; j < pptrTypeHeaders.Length; j++)
        {
            string header = pptrTypeHeaders[j];
            IDataProvider[] field = pptrTypeFields[j];

            if (field == null)
            {
                continue;
            }

            FsmVariableData genericData = new() { Type = header, Values = new List<Tuple<string, object>>() };
            varData.Add(genericData);
            for (int i = 0; i < field.Length; i++)
            {
                string name = field[i].Get<string>("name");
                IDataProvider valueField = field[i].Get<IDataProvider>("value");
                INamedAssetProvider pptr = valueField?.As<INamedAssetProvider>();
                object value = pptr?.isNull ?? true ? "[null]" : header == "GameObjects" ? new GameObjectPPtrHolder() { pptr = pptr } : pptr;
                genericData.Values.Add(new Tuple<string, object>(name, value));
            }
        }
    }

    public List<SceneInfo> LoadSceneList()
    {
        AssetTypeValueField buildSettings = GlobalGameManagers.instance.GetAsset(AssetClassID.BuildSettings);
        //AssetFileInfoEx buildSettingsInfo = ggm.table.GetAssetsOfType(0x8D)[0];
        //AssetTypeValueField buildSettings = am.GetTypeInstance(ggm, buildSettingsInfo).GetBaseField();
        AssetTypeValueField scenes = buildSettings.Get("scenes").Get("Array");
        int sceneCount = scenes.AsArray.size;

        List<SceneInfo> sceneInfos = new();
        for (int i = 0; i < sceneCount; i++)
        {
            sceneInfos.Add(new SceneInfo()
            {
                id = i,
                name = scenes[i].AsString + " (level" + i + ")",
                level = true
            });
        }

        for (int i = 0; i < sceneCount; i++)
        {
            sceneInfos.Add(new SceneInfo()
            {
                id = i,
                name = scenes[i].AsString + " (sharedassets" + i + ".assets)",
                level = false
            });
        }

        return sceneInfos;
    }
}

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

            AssetsFile file = curFile.file;
            AssetsFileTable table = curFile.table;
            
            return GetFSMInfos(file, table);
        }

        public FsmDataInstance LoadFSM(long id)
        {
            AssetFileInfoEx info = curFile.table.GetAssetInfo(id);
            AssetTypeValueField baseField = am.GetMonoBaseFieldCached(curFile, info, Path.Combine(Path.GetDirectoryName(curFile.path), "Managed"));

            FsmDataInstance dataInstance = new FsmDataInstance();

            AssetTypeValueField fsm = baseField.Get("fsm");
            AssetTypeValueField states = fsm.Get("states");
            AssetTypeValueField globalTransitions = fsm.Get("globalTransitions");
            AssetTypeValueField dataVersionField = fsm.Get("dataVersion");

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
                stateData.Values = new List<Tuple<string, string>>();
                stateData.state = new FsmState(states[i]);
                stateData.node = new FsmNodeData(stateData.state, false);
                dataInstance.states.Add(stateData);
            }

            return dataInstance;
        }

        private List<AssetInfo> GetFSMInfos(AssetsFile file, AssetsFileTable table)
        {
            List<AssetInfo> assetInfos = new List<AssetInfo>();
            uint assetCount = table.assetFileInfoCount;
            uint fsmTypeId = 0;
            foreach (AssetFileInfoEx info in table.assetFileInfo)
            {
                bool isMono = false;
                if (fsmTypeId == 0)
                {
                    ushort monoType;
                    if (file.header.format <= 0x10)
                        monoType = info.scriptIndex;
                    else
                        monoType = file.typeTree.unity5Types[info.curFileTypeOrIndex].scriptIndex;

                    if (monoType != 0xFFFF)
                    {
                        isMono = true;
                    }
                }
                else if (info.curFileType == fsmTypeId)
                {
                    isMono = true;
                }
                if (isMono)
                {
                    AssetTypeInstance monoAti = am.GetATI(file, info);
                    AssetTypeInstance scriptAti = am.GetExtAsset(curFile, monoAti.GetBaseField().Get("m_Script")).instance;
                    AssetTypeInstance goAti = am.GetExtAsset(curFile, monoAti.GetBaseField().Get("m_GameObject")).instance;
                    if (goAti == null) //found a scriptable object, oops
                    {
                        fsmTypeId = 0;
                        continue;
                    }
                    string m_Name = goAti.GetBaseField().Get("m_Name").GetValue().AsString();
                    string m_ClassName = scriptAti.GetBaseField().Get("m_ClassName").GetValue().AsString();

                    if (m_ClassName == "PlayMakerFSM")
                    {
                        if (fsmTypeId == 0)
                            fsmTypeId = info.curFileType;

                        BinaryReader reader = file.reader;

                        long oldPos = reader.BaseStream.Position;
                        reader.BaseStream.Position = info.absoluteFilePos;
                        reader.BaseStream.Position += 28;
                        uint length = reader.ReadUInt32();
                        reader.ReadBytes((int)length);

                        long pad = 4 - (reader.BaseStream.Position % 4);
                        if (pad != 4) reader.BaseStream.Position += pad;

                        reader.BaseStream.Position += 16;

                        if (file.header.format <= 0x10)
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
                }
            }

            assetInfos.Sort((x, y) => x.name.CompareTo(y.name));

            return assetInfos;
        }
    }
}

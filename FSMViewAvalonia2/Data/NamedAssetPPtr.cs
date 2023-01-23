namespace FSMViewAvalonia2.Data;
public class NamedAssetPPtr : AssetPPtr, INamedAssetProvider
{
    public string file { get; set; }
    public string name { get; set; }
    public long fileId { get; set; }
    public NamedAssetPPtr(int fileID, long pathID) : base(fileID, pathID)
    {
        fileId = pathID;
        file = string.Empty;
        name = string.Empty;
    }
    public NamedAssetPPtr(int fileID, long pathID, string name) : base(fileID, pathID)
    {
        fileId = pathID;
        file = string.Empty;
        this.name = name;
    }
    public NamedAssetPPtr(int fileID, long pathID, string name, string file) : base(fileID, pathID)
    {
        fileId = pathID;
        this.file = file;
        this.name = name;
    }


    bool INamedAssetProvider.isNull => pathID == 0;


    public override string ToString() => file == string.Empty ? name : $"{name} ({file})";
}
public class GameObjectPPtrHolder
{
    public INamedAssetProvider pptr;
    public override string ToString() => pptr.ToString();
}

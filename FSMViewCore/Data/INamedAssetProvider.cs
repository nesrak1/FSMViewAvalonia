

namespace FSMViewAvalonia2.Data;

public interface INamedAssetProvider
{
    bool isNull { get; }
    string name { get; }
    string file { get; }
    long fileId { get; }
}
public class GameObjectPPtrHolder
{
    public INamedAssetProvider pptr;
    public override string ToString() => pptr.ToString();
}
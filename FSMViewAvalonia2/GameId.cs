using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMViewAvalonia2;
public struct GameId
{
    public string Id { get; private set; }
    public readonly bool IsNone => string.IsNullOrEmpty(Id);
    public readonly override bool Equals([NotNullWhen(true)] object obj)
    {
        if(obj is not GameId)
        {
            return false;
        }
        var gi = (GameId)obj;
        return gi.Id.Equals(Id);
    }
    public readonly override int GetHashCode() => Id.GetHashCode();
    public readonly override string ToString() => Id;

  
    private static string GetGameIdFromPath(string gamePath)
    {
        string path = Path.GetDirectoryName(Path.GetFullPath(gamePath));
        if (string.IsNullOrEmpty(path) || !Directory.Exists(Path.Combine(path, "Managed"))) //Default as hk
        {
            return null;
        }

        string dataName = Path.GetFileName(path);
        if (!dataName.Contains("_Data", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return dataName.ToLower();
    }
    public static GameId FromName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return default;
        }
        return new()
        {
            Id = name.ToLower() + "_data",
        };
    }
    public static GameId FromPath(string path)
    {
        var id = GetGameIdFromPath(path);
        if (id == null)
        {
            var fp = Path.GetFullPath(path);
            foreach(var v in GameFileHelper.allGameInfos)
            {
                var gp = GameFileHelper.FindGamePath(null, true, v).Result;
                if(string.IsNullOrEmpty(gp))
                {
                    continue;
                }
                if(fp.StartsWith(
                    Path.GetFullPath(gp), StringComparison.OrdinalIgnoreCase
                    ))
                {
                    id = v.Name.ToLower() + "_data";
                    break;
                }
            }
        }

        return new()
        {
            Id = id,
        };
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMViewAvalonia2.Context;
public class GameInfo
{
    [JsonProperty("name")]
    [JsonRequired]
    public string Name { get; set; } = "Hollow Knight";
    [JsonProperty("steamid")]
    [JsonRequired]
    public int SteamId { get; set; } = 367520;
    [JsonProperty("dataDirs")]
    [JsonRequired]
    public List<string> DataDirs { get; set; } = [
        "hollow_knight_Data",
            "Hollow Knight_Data",
            "Hollow_Knight_Data"
    ];
    [JsonIgnore]
    public int Index { get; set; }
    public bool IsNone => SteamId == -1;
    public override string ToString() => $"{Name} ({SteamId})";
}
public class GameInfoCollections : List<GameInfo>
{

}

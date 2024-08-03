using MsBox.Avalonia;

using Path = System.IO.Path;

namespace FSMViewAvalonia2;

public static class GameFileHelper
{
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

    private static readonly Dictionary<GameId, GameInfo> id2infoTable = [];
    public static readonly GameInfoCollections allGameInfos;
    public static GameInfo CurrentGameInfo
    {
        get
        {
            var id = Config.config.currentGame;
            if(id >= allGameInfos.Count)
            {
                id = 0;
                Config.config.currentGame = id;
                Config.Save();
            }
            return allGameInfos[id];
        }
    }
    public static GameId CurrentGameId
    {
        get
        {
            if(CurrentGameInfo.IsNone)
            {
                return default;
            }
            return GameId.FromName(CurrentGameInfo.Name);
        }
    }
    public static GameInfo GetGameInfoFromId(GameId id)
    {
        if(id.IsNone)
        {
            return null;
        }
        if (id2infoTable.TryGetValue(id, out var result))
        {
            return result;
        }
        return null;
    }
    static GameFileHelper()
    {
        if (File.Exists("GameInfos.json"))
        {
            allGameInfos = JsonConvert.DeserializeObject<GameInfoCollections>(File.ReadAllText("GameInfos.json"));
        }

        allGameInfos ??= [];
        allGameInfos.Insert(0, new()
        {
            DataDirs = [],
            Name = "None",
            SteamId = -1
        });
        for(int i = 0; i < allGameInfos.Count; i++)
        {
            var info = allGameInfos[i];
            info.Index = i;
            id2infoTable[GameId.FromName(info.Name)] = info;
            foreach(var v in info.DataDirs)
            {
                id2infoTable[GameId.FromName(v)] = info;
            }
        }
    }

    public static async Task<string> FindGamePath(Window win, bool noUI = false, GameInfo info = null)
    {
        Config.config.gamePaths ??= [];
        info ??= CurrentGameInfo;
        if(info.IsNone)
        {
            return "";
        }
        if (Config.config.gamePaths.TryGetValue(info.SteamId, out var path) && Directory.Exists(path))
        {
            return path;
        }

        path = await FindSteamGamePath(win, info.SteamId, info.Name, noUI);

        if (path != null)
        {
            Config.config.gamePaths[info.SteamId] = path;
        }

        return path;

    }


    public static async Task<string> FindSteamGamePath(Window win, int appid, string gameName, bool noUI)
    {
        string path = null;
        if (ReadRegistrySafe("Software\\Valve\\Steam", "SteamPath") != null)
        {
            string appsPath = Path.Combine((string) ReadRegistrySafe("Software\\Valve\\Steam", "SteamPath"), "steamapps");

            if (File.Exists(Path.Combine(appsPath, $"appmanifest_{appid}.acf")))
            {
                return Path.Combine(Path.Combine(appsPath, "common"), gameName);
            }

            path = SearchAllInstallations(Path.Combine(appsPath, "libraryfolders.vdf"), appid, gameName);
        }

        if (path == null && !noUI)
        {
            _ = await MessageBoxUtil.ShowDialog(win, "Game location", "Couldn't find installation automatically. Please pick the location manually.");
            OpenFolderDialog ofd = new();
            string folder = await ofd.ShowAsync(win);
            if (folder != null && folder != "")
            {
                path = folder;
            }
        }

        return path;
    }

    private static string SearchAllInstallations(string libraryfolders, int appid, string gameName)
    {
        if (!File.Exists(libraryfolders))
        {
            return null;
        }

        StreamReader file = new(libraryfolders);
        string line;
        while ((line = file.ReadLine()) != null)
        {
            line = line.Trim();
            line = Regex.Unescape(line);
            Match regMatch = Regex.Match(line, "\"(.*)\"\\s*\"(.*)\"");
            string key = regMatch.Groups[1].Value;
            string value = regMatch.Groups[2].Value;
            if (key == "path")
            {
                if (File.Exists(Path.Combine(value, "steamapps", $"appmanifest_{appid}.acf")))
                {
                    return Path.Combine(Path.Combine(value, "steamapps", "common"), gameName);
                }
            }
        }

        return null;
    }

    private static object ReadRegistrySafe(string path, string key)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return null;
        }

        using RegistryKey subkey = Registry.CurrentUser.OpenSubKey(path);
        if (subkey != null)
        {
            return subkey.GetValue(key);
        }

        return null;
    }

    public static string FindGameFilePath(string file, bool noUI = false, GameInfo info = null)
    {
        var inMain = Thread.CurrentThread == Program.mainThread;
        Task<string> task = FindGamePath(null, noUI | inMain, info);
        var result = task.Result;
        if (string.IsNullOrEmpty(result))
        {
            if(!noUI)
            {
                throw new InvalidOperationException();
            }
            return null;
        }
        return FindGameFilePath(result, file);
    }
    public static string FindGameFilePath(string hkRootPath, string file)
    {
        foreach (string pathTest in CurrentGameInfo.DataDirs)
        {
            string dataPath = Path.Combine(hkRootPath, pathTest);
            string filePath = Path.Combine(dataPath, file);
            if (File.Exists(filePath) || Directory.Exists(filePath))
            {
                return filePath;
            }
        }

        return null;
    }
}

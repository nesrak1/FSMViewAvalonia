using MsBox.Avalonia;

using Path = System.IO.Path;

namespace FSMViewAvalonia2;

public static class GameFileHelper
{
    public class GameInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "Hollow Knight";
        [JsonProperty("steamid")]
        public int SteamId { get; set; } = 367520;
        [JsonProperty("dataDirs")]
        public List<string> DataDirs { get; set; } = [
            "hollow_knight_Data",
            "Hollow Knight_Data",
            "Hollow_Knight_Data"
        ];
    }
    public static readonly GameInfo gameInfo;

    static GameFileHelper()
    {
        if (!File.Exists("GameInfo.json"))
        {
            MessageBoxManager
                    .GetMessageBoxStandard("No game info",
                    "You're missing GameInfo.json next to the executable. Please make sure it exists.")
                    .ShowAsync().Wait();
            Environment.Exit(0);
        }

        gameInfo = JsonConvert.DeserializeObject<GameInfo>(File.ReadAllText("GameInfo.json"));
    }

    public static async Task<string> FindHollowKnightPath(Window win)
    {
        if (!string.IsNullOrEmpty(Config.config.hkPath))
        {
            return Config.config.hkPath;
        }

        string path = await FindSteamGamePath(win, gameInfo.SteamId, gameInfo.Name);

        if (path != null)
        {
            Config.config.hkPath = path;
        }

        return path;

    }


    public static async Task<string> FindSteamGamePath(Window win, int appid, string gameName)
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

        if (path == null)
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

    public static string FindGameFilePath(string file)
    {
        Task<string> task = FindHollowKnightPath(null);
        task.Wait();
        return FindGameFilePath(task.Result, file);
    }
    public static string FindGameFilePath(string hkRootPath, string file)
    {
        foreach (string pathTest in gameInfo.DataDirs)
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

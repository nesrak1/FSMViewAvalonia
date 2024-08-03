

using System.Xml.Linq;

namespace FSMViewAvalonia2;

internal class Config
{
    public static readonly string ConfigPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Environment.ProcessPath), "Config.json");
    public static readonly Config config;
    static Config()
    {
        if (File.Exists(ConfigPath))
        {
            try
            {
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }
            catch (Exception)
            {
                config = new();
            }
        }
        else
        {
            config = new();
        }

        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
    }
    public static void Save()
    {
        File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(config, Formatting.Indented));
    }
    private static void CurrentDomain_ProcessExit(object sender, EventArgs e) => Save();
    public bool option_includeSharedassets = false;
    public bool option_extraLAMZABOnTempFile = false;
    public Dictionary<int, string> gamePaths = [];
    public string SpyPath = "";
    public int currentGame = 0;
    public bool option_enableFSMListCache = true;
}

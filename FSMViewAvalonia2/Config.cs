

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
            } catch (Exception)
            {
                config = new();
            }
        } else
        {
            config = new();
        }

        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
    }

    private static void CurrentDomain_ProcessExit(object sender, EventArgs e) => File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(config, Formatting.Indented));

    public string hkPath = "";
    public string SpyPath = "";
}

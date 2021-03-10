using Avalonia.Controls;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FSMViewAvalonia2
{
    public static class SteamHelper
    {
        public static readonly int HOLLOWKNIGHT_APP_ID = 367520;
        public static readonly string HOLLOWKNIGHT_GAME_NAME = "Hollow Knight";

        public static async Task<string> FindHollowKnightPath(Window mbWindow)
        {
            //won't work on linux, todo
            return await FindSteamGamePathRegistry(mbWindow, HOLLOWKNIGHT_APP_ID, HOLLOWKNIGHT_GAME_NAME);
        }

        public static async Task<string> FindSteamGamePathRegistry(Window mbWindow, int appid, string gameName)
        {
            if (ReadRegistrySafe("Software\\Valve\\Steam", "SteamPath") == null)
            {
                await MessageBoxUtil.ShowDialog(mbWindow, "Error", "You either don't have steam installed or your registry variable isn't set.");
                return "";
            }
            
            string appsPath = Path.Combine((string)ReadRegistrySafe("Software\\Valve\\Steam", "SteamPath"), "steamapps");
            
            if (File.Exists(Path.Combine(appsPath, $"appmanifest_{appid}.acf")))
            {
                return Path.Combine(Path.Combine(appsPath, "common"), gameName);
            }
            
            string path = SearchAllInstallations(Path.Combine(appsPath, "libraryfolders.vdf"), appid, gameName);
            if (path == null)
            {
                await MessageBoxUtil.ShowDialog(mbWindow, "Error", "Couldn't find installation automatically. Please pick the location manually.");
                OpenFolderDialog ofd = new OpenFolderDialog();
                string openPath = await ofd.ShowAsync(mbWindow);
                if (openPath != null && openPath != string.Empty)
                {
                    return openPath;
                }
            }
            else
            {
                return path;
            }

            return "";
        }

        private static string SearchAllInstallations(string libraryfolders, int appid, string gameName)
        {
            if (!File.Exists(libraryfolders))
            {
                return null;
            }
            StreamReader file = new StreamReader(libraryfolders);
            string line;
            while ((line = file.ReadLine()) != null)
            {
                line = line.Trim();
                line = Regex.Unescape(line);
                Match regMatch = Regex.Match(line, "\"(.*)\"\\s*\"(.*)\"");
                string key = regMatch.Groups[1].Value;
                string value = regMatch.Groups[2].Value;
                if (int.TryParse(key, out int _))
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
            using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(path))
            {
                if (subkey != null)
                {
                    return subkey.GetValue(key);
                }
            }

            return null;
        }
    }
}

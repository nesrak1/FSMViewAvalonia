using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSMViewAvalonia2;
partial class MainWindow
{
    public class LookupTable
    {
        public string gameId;
        public class FsmItem
        {
            public string goName;
            public string fsmName;
            public long goId;
            public long fsmId;
            public string assetFileName;
            public bool isTemplate;

            public string Name => $"{goName}-{fsmName} [G:{goId}][F:{fsmId}]({assetFileName})";
            public override string ToString() => Name;
        }
        public List<FsmItem> fsms = [];
    }
    private void InitFind()
    {
        findInAllScenes.Click += FindInAllScenes_Click;
        generateFsmList.Click += GenerateFsmList_Click;
    }
    private LookupTable fsmsLookupCache;
    private async void GenerateFsmList_Click(object sender, RoutedEventArgs e)
    {
        var view = await GenerateLookupTable(false, true);
        view?.Hide();
    }
    private async void FindInAllScenes_Click(object sender, RoutedEventArgs e)
    {
        var view = await GenerateLookupTable(false, false);
        if(view is null)
        {
            return;
        }
        view.Closed += async (_, _1) =>
        {
            var info = view.selectedAssetInfo;
            if (info is null)
            {
                return;
            }
            _ = await LoadFsm(GameFileHelper.FindGameFilePath(info.assetFileName), info.goName + "-" + info.fsmName, false);
        };
    }


    private async Task<FindFSMSelectionDialog> GenerateLookupTable(bool noUI = false, bool force = false)
    {
        DispatcherTimer timer = null;
        try
        {
            FindFSMSelectionDialog findFSMSelection = null;
            var curGameId = FSMAssetHelper.GetGameId(GameFileHelper.FindGameFilePath("Managed"));
            var cachePath = $"fsmLookupTable-{curGameId}.json";
            int currentFile = 0;
            int totalFile = 9999;
            CancellationTokenSource cancel = new();
            
            Dispatcher.UIThread.Invoke(() =>
            {
                fileMenuRoot.IsEnabled = false;
                findMenuRoot.IsEnabled = false;
                if(!noUI)
                {
                    findFSMSelection = new();
                    findFSMSelection.Closed += (_, _1) =>
                    {
                        cancel.Cancel();
                    };
                    findFSMSelection.Show();
                    timer = new(TimeSpan.FromSeconds(0.1), DispatcherPriority.Normal, (_, _1) =>
                    {
                        findFSMSelection.UpdateProgress(currentFile, totalFile);
                    });
                    timer.Start();
                }
            });
            
            if (!force)
            {
                if(fsmsLookupCache is null)
                {
                    if(File.Exists(cachePath))
                    {
                        fsmsLookupCache = JsonConvert.DeserializeObject<LookupTable>(File.ReadAllText(cachePath));
                    }
                }
                if (fsmsLookupCache != null && !force)
                {
                    findFSMSelection?.Finish(fsmsLookupCache.fsms);
                    return findFSMSelection;
                }
            }
            
            await Task.Run(async() =>
            {
                var result = new LookupTable()
                {
                    gameId = curGameId,
                };
                var root = Path.GetDirectoryName(GameFileHelper.FindGameFilePath("Managed"));
                List<string> files = [];

                foreach(var p in Directory.EnumerateFiles(root, "*", SearchOption.TopDirectoryOnly))
                {
                    var pname = Path.GetFileName(p);
                    if(pname.StartsWith("level", StringComparison.OrdinalIgnoreCase) ||
                        pname.EndsWith("assets", StringComparison.OrdinalIgnoreCase)
                        )
                    {
                        files.Add(p);
                    }
                }
                totalFile = files.Count;
                var loader = new FSMLoader(this);
                var l = result.fsms;
                foreach(var v in files)
                {
                    cancel.Token.ThrowIfCancellationRequested();
                    var assetName = Path.GetFileName(v);
                    foreach(var a in loader.LoadAllFSMsFromFile(v, false,true).OfType<AssetInfoUnity>())
                    {
                        l.Add(new()
                        {
                            assetFileName = assetName,
                            fsmId = a.fsmId,
                            goId = a.goId,
                            fsmName = a.name,
                            goName = a.path + a.goName,
                            isTemplate = false
                        });
                    }
                    
                    currentFile++;
                }
                loader = null;
                fsmsLookupCache = result;
                await File.WriteAllTextAsync(cachePath,
                    JsonConvert.SerializeObject(result));


                GC.Collect(2, GCCollectionMode.Forced);
            });
            findFSMSelection?.Finish(fsmsLookupCache.fsms);
            return findFSMSelection;
        }
        catch(OperationCanceledException)
        {
            return null;
        }
        finally
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                timer?.Stop();
                fileMenuRoot.IsEnabled = true;
                findMenuRoot.IsEnabled = true;
            });
        }
    }
}

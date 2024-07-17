

namespace FSMViewAvalonia2;

public partial class FindFSMSelectionDialog : Window
{
    public IEnumerable<MainWindow.LookupTable.FsmItem> AssetInfos { get; private set; }


    public MainWindow.LookupTable.FsmItem selectedAssetInfo;
    public FindFSMSelectionDialog()
    {
        InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        //generated items
        //generated events
        selectButton.Click += SelectButton_Click;
        listBox.DoubleTapped += SelectButton_Click;

        AutoCompleteBox tbox = this.FindControl<AutoCompleteBox>("searchBox");

        tbox.TextChanged += OnInput;
        
    }
    public void UpdateProgress(int val, int total)
    {
        loadProgressBar.Value = val;
        loadProgressBar.Maximum = total;
    }
    public void Finish(IEnumerable<MainWindow.LookupTable.FsmItem> items)
    {
        inputRoot.IsVisible = true;
        loadRoot.IsVisible = false;
        AssetInfos = items;

    }
    private void OnInput(object sender, EventArgs eventArgs)
    {
        string query = ((AutoCompleteBox) sender)?.Text;

        RefreshFilter(query);
    }

    public void RefreshFilter(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            listBox.ItemsSource = AssetInfos;
            return;
        }

        listBox.ItemsSource = AssetInfos.Where(x => x.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    private void SelectButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (listBox.SelectedItem is MainWindow.LookupTable.FsmItem assetInfo)
        {
            selectedAssetInfo = assetInfo;
            Close();
        }
    }
}

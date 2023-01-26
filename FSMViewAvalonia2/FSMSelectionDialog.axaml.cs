

namespace FSMViewAvalonia2;

public partial class FSMSelectionDialog : Window
{
    public List<AssetInfo> AssetInfos { get; private set; }

    public AssetInfo selectedAssetInfo;
    public FSMSelectionDialog()
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

    private void OnInput(object sender, EventArgs eventArgs)
    {
        string query = ((AutoCompleteBox) sender)?.Text;

        RefreshFilter(query);
    }

    public void RefreshFilter(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            listBox.Items = AssetInfos;
            return;
        }

        listBox.Items = AssetInfos.Where(x => x.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    public FSMSelectionDialog(List<AssetInfo> assetInfos, string name) : this()
    {
        AssetInfos = assetInfos;

        listBox.Items = AssetInfos;

        Title = $"Select FSM({name})";
    }

    private void SelectButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (listBox.SelectedItem is AssetInfo assetInfo)
        {
            selectedAssetInfo = assetInfo;
            Close();
        }
    }
}

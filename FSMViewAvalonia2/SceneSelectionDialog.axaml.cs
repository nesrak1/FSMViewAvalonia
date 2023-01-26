namespace FSMViewAvalonia2;

public partial class SceneSelectionDialog : Window
{
    public List<SceneInfo> AssetInfos { get; private set; }

    public long selectedID = -1;
    public bool selectedLevel = true;
    public SceneSelectionDialog()
    {
        InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        //generated events
        selectButton.Click += SelectButton_Click;
        listBox.DoubleTapped += SelectButton_Click;

        this.FindControl<AutoCompleteBox>("searchBox").TextChanged += TextChanged;
    }

    private void TextChanged(object sender, EventArgs e)
    {
        var box = (AutoCompleteBox) sender;
        string text = box.Text;

        if (string.IsNullOrEmpty(text))
        {
            listBox.Items = AssetInfos;
            return;
        }

        listBox.Items = AssetInfos
                        .Select(x => new { FSM = x, Trimmed = x.Name[("Assets/Scenes/".Length - 1)..] })
                        .Where(x => text.Split().All(part => x.Trimmed.Contains(part, StringComparison.OrdinalIgnoreCase)))
                        .Select(x => x.FSM);
    }

    public SceneSelectionDialog(List<SceneInfo> assetInfos) : this()
    {
        AssetInfos = assetInfos;

        listBox.Items = AssetInfos;
    }

    private void SelectButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (listBox.SelectedItem is SceneInfo sceneInfo)
        {
            selectedID = sceneInfo.id;
            selectedLevel = sceneInfo.level;
            Close();
        }
    }
}

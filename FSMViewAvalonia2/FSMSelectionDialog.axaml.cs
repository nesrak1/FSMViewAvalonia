

namespace FSMViewAvalonia2
{
    public class FSMSelectionDialog : Window
    {
        public List<AssetInfo> AssetInfos { get; private set; }
        public ListBox listBox;
        public Button selectButton;

        public AssetInfo selectedAssetInfo;
        public FSMSelectionDialog()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            //generated items
            listBox = this.FindControl<ListBox>("listBox");
            selectButton = this.FindControl<Button>("selectButton");
            //generated events
            selectButton.Click += SelectButton_Click;
            listBox.DoubleTapped += SelectButton_Click;

            var tbox = this.FindControl<AutoCompleteBox>("searchBox");

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

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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
}

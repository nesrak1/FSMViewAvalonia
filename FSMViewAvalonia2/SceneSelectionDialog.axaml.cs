using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;

namespace FSMViewAvalonia2
{
    public class SceneSelectionDialog : Window
    {
        public List<SceneInfo> AssetInfos { get; private set; }
        public ListBox listBox;
        public Button selectButton;

        public long selectedID = -1;
        public SceneSelectionDialog()
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
        }

        public SceneSelectionDialog(List<SceneInfo> assetInfos) : this()
        {
            AssetInfos = assetInfos;

            listBox.Items = AssetInfos;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void SelectButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            selectedID = AssetInfos[listBox.SelectedIndex].id;
            Close();
        }
    }
}

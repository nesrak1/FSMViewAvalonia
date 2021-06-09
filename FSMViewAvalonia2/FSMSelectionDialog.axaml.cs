using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;

namespace FSMViewAvalonia2
{
    public class FSMSelectionDialog : Window
    {
        public List<AssetInfo> AssetInfos { get; private set; }
        public ListBox listBox;
        public Button selectButton;

        public long selectedID;
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

            if (string.IsNullOrEmpty(query))
            {
                listBox.Items = AssetInfos;
                return;
            }

            listBox.Items = AssetInfos.Where(x => x.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        public FSMSelectionDialog(List<AssetInfo> assetInfos) : this()
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
            selectedID = ((AssetInfo) listBox.SelectedItem).id;
            Close();
        }
    }
}

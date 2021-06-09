﻿using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System.Linq;

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
                            .Select(x => new { FSM = x, Trimmed = x.Name.Substring("Assets/Scenes/".Length - 1) })
                            .Where(x => text.Split().All(part => x.Trimmed.Contains(part, StringComparison.OrdinalIgnoreCase)))
                            .Select(x => x.FSM);
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
            selectedID = ((SceneInfo) listBox.SelectedItem).id;
            Close();
        }
    }
}

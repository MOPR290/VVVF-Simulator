using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Util;

namespace VvvfSimulator.GUI.Mascon
{
    /// <summary>
    /// Generation_Mascon_Control_Midi.xaml の相互作用ロジック
    /// </summary>
    public partial class MasconControlMidi : Window
    {

        bool IgnoreUpdate = true;
        string InitialPath;
        public MasconControlMidi(string? initial_path)
        {
            InitializeComponent();
            IgnoreUpdate = false;
            this.InitialPath = initial_path == null ? "" : initial_path;
        }
        public class LoadData
        {
            public int track = 1;
            public int priority = 1;
            public double division = 1;
            public String path = "a";
        }
        public LoadData loadData = new();

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IgnoreUpdate) return;
            TextBox tb = (TextBox)sender;
            Object tag = tb.Tag;

            if (tag.Equals("Track"))
            {
                int track = ParseTextBox.ParseInt(tb, 1);
                loadData.track = track;
            }
            else if (tag.Equals("Priority"))
            {
                int priority = ParseTextBox.ParseInt(tb, 1);
                loadData.priority = priority;
            }else if (tag.Equals("Division"))
            {
                double d = ParseTextBox.ParseDouble(tb, 1);
                loadData.division = d;
            }
        }

        private void Select_Path_Button_Click(object sender, RoutedEventArgs e)
        {
            if (IgnoreUpdate) return;

            var dialog = new OpenFileDialog
            {
                Filter = "Midi (*.mid)|*.mid|All (*.*)|*.*",
                InitialDirectory = InitialPath
            };
            if (dialog.ShowDialog() == false) return;

            String path = dialog.FileName;
            loadData.path = path;
        }
    }
}

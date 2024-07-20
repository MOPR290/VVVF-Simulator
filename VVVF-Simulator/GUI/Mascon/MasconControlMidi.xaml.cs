using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;

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

        private static double ParseDouble(TextBox tb, double minimum)
        {
            try
            {
                VisualStateManager.GoToState(tb, "Success", false);
                double d = double.Parse(tb.Text);
                if (d < minimum) throw new Exception();
                return d;
            }
            catch
            {
                VisualStateManager.GoToState(tb, "Error", false);
                return 0;
            }
        }

        private static int ParseInt(TextBox tb, int minimum)
        {
            try
            {
                VisualStateManager.GoToState(tb, "Success", false);
                int i = int.Parse(tb.Text);
                if(i < minimum) throw new Exception();
                return i;
            }
            catch
            {
                VisualStateManager.GoToState(tb, "Error", false);
                return 0;
            }
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
                int track = ParseInt(tb, 1);
                loadData.track = track;
            }
            else if (tag.Equals("Priority"))
            {
                int priority = ParseInt(tb, 1);
                loadData.priority = priority;
            }else if (tag.Equals("Division"))
            {
                double d = ParseDouble(tb, 1);
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

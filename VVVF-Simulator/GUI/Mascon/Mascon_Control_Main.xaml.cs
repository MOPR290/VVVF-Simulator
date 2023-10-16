using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VvvfSimulator.Yaml.MasconControl;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze.YamlMasconData;

namespace VvvfSimulator.GUI.Mascon
{
    public class SmallTitleConverter : IValueConverter
    {
        // 2.Convertメソッドを実装
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not YamlMasconDataPoint)
                return DependencyProperty.UnsetValue;

            YamlMasconDataPoint val = (YamlMasconDataPoint)value;

            return "Rate : " + String.Format("{0:F2}", val.rate) + " , Duration : " + String.Format("{0:F2}", val.duration) + " , Mascon : " + val.mascon_on.ToString() + " , Brake : " + val.brake;
        }

        // 3.ConvertBackメソッドを実装
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("Wow!");
        }
    }

    public class BigTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not YamlMasconDataPoint)
                return DependencyProperty.UnsetValue;

            YamlMasconDataPoint val = (YamlMasconDataPoint)value;

            return val.order;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("Wow!");
        }
    }

    /// <summary>
    /// Generation_Mascon_Control_Window.xaml の相互作用ロジック
    /// </summary>
    public partial class Mascon_Control_Main : Window
    {
        public Mascon_Control_Main()
        {
            InitializeComponent();

            mascon_control_list.ItemsSource = Yaml_Mascon_Manage.CurrentData.points;


        }

        private String load_path = "";
        private String load_midi_path = "";
        private void File_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            Object? tag = button.Tag;
            if (tag == null) return;
            if (tag.Equals("Load"))
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml|All (*.*)|*.*"
                };
                if (dialog.ShowDialog() == false) return;

                if (Yaml_Mascon_Manage.LoadYaml(dialog.FileName))
                    MessageBox.Show("Load OK.", "Great", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Invalid yaml or path.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                load_path = dialog.FileName;

                Refresh_ItemList();

            }
            else if (tag.Equals("Save"))
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml",
                    FileName = Path.GetFileName(load_path)
                };

                // ダイアログを表示する
                if (dialog.ShowDialog() == false) return;

                if (Yaml_Mascon_Manage.SaveYaml(dialog.FileName))
                    MessageBox.Show("Save OK.", "Great", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Error occurred on saving.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (tag.Equals("Midi"))
            {
                MasconControlMidi gmcm = new(Path.GetDirectoryName(load_midi_path));
                gmcm.ShowDialog();
                var load_data = gmcm.loadData;
                try
                {
                    load_midi_path = load_data.path;
                    YamlMasconData? data = YamlMasconMidi.Convert(load_data);
                    if (data == null) return;
                    Yaml_Mascon_Manage.CurrentData = data;
                    Refresh_ItemList();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void Refresh_ItemList()
        {
            mascon_control_list.ItemsSource = Yaml_Mascon_Manage.CurrentData.points;
            mascon_control_list.Items.Refresh();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            Object? tag = mi.Tag;

            if (tag.Equals("sort"))
            {
                Yaml_Mascon_Manage.CurrentData.points.Sort((a, b) => Math.Sign(a.order - b.order));
                Refresh_ItemList();


            }
            else if (tag.Equals("copy"))
            {
                var selected_item = mascon_control_list.SelectedItem;
                if (selected_item == null) return;

                YamlMasconDataPoint data = (YamlMasconDataPoint)selected_item;
                Yaml_Mascon_Manage.CurrentData.points.Add(data.Clone());

                Refresh_ItemList();
            }
        }

        private void mascon_control_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected_item = mascon_control_list.SelectedItem;
            if (selected_item == null) return;

            edit_view_frame.Navigate(new Mascon_Control_Edit(this, (YamlMasconDataPoint)selected_item));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Object tag = btn.Tag;

            if (tag.Equals("Add"))
            {
                Yaml_Mascon_Manage.CurrentData.points.Add(new());
                Refresh_ItemList();
            }else if (tag.Equals("Remove"))
            {
                int selected = mascon_control_list.SelectedIndex;
                if (selected < 0) return;
                Yaml_Mascon_Manage.CurrentData.points.RemoveAt(selected);
                Refresh_ItemList();
            }else if (tag.Equals("Reset"))
            {
                Yaml_Mascon_Manage.CurrentData = Yaml_Mascon_Manage.DefaultData.Clone();
                Refresh_ItemList();
            }
        }
    }
}

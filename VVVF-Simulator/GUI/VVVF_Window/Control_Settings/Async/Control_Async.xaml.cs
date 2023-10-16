using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using VvvfSimulator;
using VvvfSimulator.GUI.VVVF_Window.Control_Settings.Common;
using VvvfSimulator.VVVF_Window.Control_Settings.Async;
using VvvfSimulator.VVVF_Window.Control_Settings.Async.Random_Range;
using VvvfSimulator.VVVF_Window.Control_Settings.Async.Vibrato;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterCarrierFreq;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterRandom;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterRandom.YamlAsyncParameterRandomValue;

namespace VvvfSimulator.VVVF_Window.Control_Settings
{
    /// <summary>
    /// Control_Async.xaml の相互作用ロジック
    /// </summary>
    public partial class Control_Async : UserControl
    {
        YamlControlData data;
        MainWindow MainWindow;

        bool no_update = true;
        public Control_Async(YamlControlData ycd, MainWindow mainWindow)
        {
            MainWindow = mainWindow;
            data = ycd;

            InitializeComponent();

            apply_data();

            no_update = false;
        }

        private void apply_data()
        {
            Yaml_Async_Carrier_Mode[] modes = (Yaml_Async_Carrier_Mode[])Enum.GetValues(typeof(Yaml_Async_Carrier_Mode));
            carrier_freq_mode.ItemsSource = modes;
            carrier_freq_mode.SelectedItem = data.async_data.carrier_wave_data.carrier_mode;

            Random_Range_Type_Selector.ItemsSource = (Yaml_Async_Parameter_Random_Value_Mode[])Enum.GetValues(typeof(Yaml_Async_Parameter_Random_Value_Mode));
            Random_Range_Type_Selector.SelectedItem = data.async_data.random_data.random_range.value_mode;
            
            Random_Interval_Type_Selector.ItemsSource = (Yaml_Async_Parameter_Random_Value_Mode[])Enum.GetValues(typeof(Yaml_Async_Parameter_Random_Value_Mode));
            Random_Interval_Type_Selector.SelectedItem = data.async_data.random_data.random_interval.value_mode;

            show_selected_carrier_mode(data.async_data.carrier_wave_data.carrier_mode);
            Show_Random_Setting(Random_Range_Setting_Frame, data.async_data.random_data.random_range);
            Show_Random_Setting(Random_Interval_Setting_Frame, data.async_data.random_data.random_interval);
        }

        private void ComboBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (no_update) return;

            ComboBox cb = (ComboBox)sender;
            Object? tag = cb.Tag;
            if (tag == null) return;

            if (tag.Equals("Random_Range"))
            {
                Yaml_Async_Parameter_Random_Value_Mode selected = (Yaml_Async_Parameter_Random_Value_Mode)Random_Range_Type_Selector.SelectedItem;
                data.async_data.random_data.random_range.value_mode = selected;
                Show_Random_Setting(Random_Range_Setting_Frame, data.async_data.random_data.random_range);
            }
            else if (tag.Equals("Random_Interval"))
            {
                Yaml_Async_Parameter_Random_Value_Mode selected = (Yaml_Async_Parameter_Random_Value_Mode)Random_Interval_Type_Selector.SelectedItem;
                data.async_data.random_data.random_interval.value_mode = selected;
                Show_Random_Setting(Random_Interval_Setting_Frame, data.async_data.random_data.random_interval);
            }
            else if (tag.Equals("Param"))
            {
                Yaml_Async_Carrier_Mode selected = (Yaml_Async_Carrier_Mode)carrier_freq_mode.SelectedItem;
                data.async_data.carrier_wave_data.carrier_mode = selected;
                show_selected_carrier_mode(selected);
            }
        }

        private void show_selected_carrier_mode(Yaml_Async_Carrier_Mode selected)
        {
            if (selected == Yaml_Async_Carrier_Mode.Const)
                carrier_setting.Navigate(new Control_Async_Carrier_Const(data, MainWindow));
            else if (selected == Yaml_Async_Carrier_Mode.Moving)
                carrier_setting.Navigate(new Control_Moving_Setting(data.async_data.carrier_wave_data.moving_value));
            else if (selected == Yaml_Async_Carrier_Mode.Vibrato)
                carrier_setting.Navigate(new Control_Async_Vibrato(data, MainWindow));
            else if(selected == Yaml_Async_Carrier_Mode.Table)
                carrier_setting.Navigate(new Control_Async_Carrier_Table(data));
        }

        private void Show_Random_Setting(Frame ShowFrame, YamlAsyncParameterRandomValue SettingValue)
        {
            if (SettingValue.value_mode == Yaml_Async_Parameter_Random_Value_Mode.Const)
                ShowFrame.Navigate(new Control_Async_Random_Const(SettingValue));
            else if (SettingValue.value_mode == Yaml_Async_Parameter_Random_Value_Mode.Moving)
                ShowFrame.Navigate(new Control_Moving_Setting(SettingValue.moving_value));
        }

        private int parse_i(TextBox tb)
        {
            try
            {
                tb.Background = new BrushConverter().ConvertFrom("#FFFFFFFF") as Brush;
                return Int32.Parse(tb.Text);
            }
            catch
            {
                tb.Background = new BrushConverter().ConvertFrom("#FFfed0d0") as Brush;
                return -1;
            }
        }
    }
}

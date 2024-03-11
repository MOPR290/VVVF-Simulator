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
using VvvfSimulator.GUI.Create.Waveform.Common;
using VvvfSimulator.GUI.Create.Waveform.Async;
using VvvfSimulator.GUI.Create.Waveform.Async.Random_Range;
using VvvfSimulator.GUI.Create.Waveform.Async.Vibrato;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterCarrierFreq;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterRandom;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterRandom.YamlAsyncParameterRandomValue;

namespace VvvfSimulator.GUI.Create.Waveform
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
            YamlAsyncCarrierMode[] modes = (YamlAsyncCarrierMode[])Enum.GetValues(typeof(YamlAsyncCarrierMode));
            carrier_freq_mode.ItemsSource = modes;
            carrier_freq_mode.SelectedItem = data.AsyncModulationData.CarrierWaveData.Mode;

            Random_Range_Type_Selector.ItemsSource = (YamlAsyncParameterRandomValueMode[])Enum.GetValues(typeof(YamlAsyncParameterRandomValueMode));
            Random_Range_Type_Selector.SelectedItem = data.AsyncModulationData.RandomData.Range.Mode;
            
            Random_Interval_Type_Selector.ItemsSource = (YamlAsyncParameterRandomValueMode[])Enum.GetValues(typeof(YamlAsyncParameterRandomValueMode));
            Random_Interval_Type_Selector.SelectedItem = data.AsyncModulationData.RandomData.Interval.Mode;

            show_selected_carrier_mode(data.AsyncModulationData.CarrierWaveData.Mode);
            Show_Random_Setting(Random_Range_Setting_Frame, data.AsyncModulationData.RandomData.Range);
            Show_Random_Setting(Random_Interval_Setting_Frame, data.AsyncModulationData.RandomData.Interval);
        }

        private void ComboBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (no_update) return;

            ComboBox cb = (ComboBox)sender;
            Object? tag = cb.Tag;
            if (tag == null) return;

            if (tag.Equals("Random_Range"))
            {
                YamlAsyncParameterRandomValueMode selected = (YamlAsyncParameterRandomValueMode)Random_Range_Type_Selector.SelectedItem;
                data.AsyncModulationData.RandomData.Range.Mode = selected;
                Show_Random_Setting(Random_Range_Setting_Frame, data.AsyncModulationData.RandomData.Range);
            }
            else if (tag.Equals("Random_Interval"))
            {
                YamlAsyncParameterRandomValueMode selected = (YamlAsyncParameterRandomValueMode)Random_Interval_Type_Selector.SelectedItem;
                data.AsyncModulationData.RandomData.Interval.Mode = selected;
                Show_Random_Setting(Random_Interval_Setting_Frame, data.AsyncModulationData.RandomData.Interval);
            }
            else if (tag.Equals("Param"))
            {
                YamlAsyncCarrierMode selected = (YamlAsyncCarrierMode)carrier_freq_mode.SelectedItem;
                data.AsyncModulationData.CarrierWaveData.Mode = selected;
                show_selected_carrier_mode(selected);
            }
        }

        private void show_selected_carrier_mode(YamlAsyncCarrierMode selected)
        {
            if (selected == YamlAsyncCarrierMode.Const)
                carrier_setting.Navigate(new Control_Async_Carrier_Const(data, MainWindow));
            else if (selected == YamlAsyncCarrierMode.Moving)
                carrier_setting.Navigate(new Control_Moving_Setting(data.AsyncModulationData.CarrierWaveData.MovingValue));
            else if (selected == YamlAsyncCarrierMode.Vibrato)
                carrier_setting.Navigate(new Control_Async_Vibrato(data, MainWindow));
            else if(selected == YamlAsyncCarrierMode.Table)
                carrier_setting.Navigate(new Control_Async_Carrier_Table(data));
        }

        private void Show_Random_Setting(Frame ShowFrame, YamlAsyncParameterRandomValue SettingValue)
        {
            if (SettingValue.Mode == YamlAsyncParameterRandomValueMode.Const)
                ShowFrame.Navigate(new Control_Async_Random_Const(SettingValue));
            else if (SettingValue.Mode == YamlAsyncParameterRandomValueMode.Moving)
                ShowFrame.Navigate(new Control_Moving_Setting(SettingValue.MovingValue));
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

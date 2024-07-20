using System;
using System.Windows.Controls;
using System.Windows.Media;
using VvvfSimulator.GUI.Create.Waveform.Common;
using VvvfSimulator.GUI.Create.Waveform.Async;
using VvvfSimulator.GUI.Create.Waveform.Async.Vibrato;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterCarrierFreq;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterRandom;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterRandom.YamlAsyncParameterRandomValue;

namespace VvvfSimulator.GUI.Create.Waveform
{
    /// <summary>
    /// Control_Async.xaml の相互作用ロジック
    /// </summary>
    public partial class ControlAsync : UserControl
    {
        YamlControlData data;
        bool no_update = true;
        public ControlAsync(YamlControlData ycd)
        {
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

            ShowSelectedCarrierMode(data.AsyncModulationData.CarrierWaveData.Mode);
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
                ShowSelectedCarrierMode(selected);
            }
        }

        private void ShowSelectedCarrierMode(YamlAsyncCarrierMode selected)
        {
            if (selected == YamlAsyncCarrierMode.Const)
                carrier_setting.Navigate(new ControlConstSetting(data.AsyncModulationData.CarrierWaveData.GetType(), data.AsyncModulationData.CarrierWaveData));
            else if (selected == YamlAsyncCarrierMode.Moving)
                carrier_setting.Navigate(new ControlMovingSetting(data.AsyncModulationData.CarrierWaveData.MovingValue));
            else if (selected == YamlAsyncCarrierMode.Vibrato)
                carrier_setting.Navigate(new ControlAsyncVibrato(data));
            else if(selected == YamlAsyncCarrierMode.Table)
                carrier_setting.Navigate(new ControlAsyncCarrierTable(data));
        }

        private void Show_Random_Setting(Frame ShowFrame, YamlAsyncParameterRandomValue SettingValue)
        {
            if (SettingValue.Mode == YamlAsyncParameterRandomValueMode.Const)
                ShowFrame.Navigate(new ControlConstSetting(SettingValue.GetType(), SettingValue));
            else if (SettingValue.Mode == YamlAsyncParameterRandomValueMode.Moving)
                ShowFrame.Navigate(new ControlMovingSetting(SettingValue.MovingValue));
        }
    }
}

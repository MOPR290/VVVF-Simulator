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
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncParameterCarrierFreqVibrato.YamlAsyncParameterVibratoValue;

namespace VvvfSimulator.GUI.Create.Waveform.Async.Vibrato
{
    /// <summary>
    /// Control_Async_Vibrato.xaml の相互作用ロジック
    /// </summary>
    public partial class Control_Async_Vibrato : UserControl
    {
        YamlControlData target;
        MainWindow main;

        bool no_update = true;
        public Control_Async_Vibrato(YamlControlData data, MainWindow mainWindow)
        {
            target = data;
            main = mainWindow;

            InitializeComponent();

            apply_data();

            no_update = false;
        }

        private void apply_data()
        {
            YamlAsyncParameterVibratoMode[] modes = (YamlAsyncParameterVibratoMode[])Enum.GetValues(typeof(YamlAsyncParameterVibratoMode));
            highest_mode.ItemsSource = modes;
            lowest_mode.ItemsSource = modes;
            interval_mode.ItemsSource = modes;

            var vibrato_data = target.AsyncModulationData.CarrierWaveData.VibratoData;

            highest_mode.SelectedItem = vibrato_data.Highest.Mode;
            set_Selected(0, vibrato_data.Highest.Mode);

            lowest_mode.SelectedItem = vibrato_data.Lowest.Mode;
            set_Selected(1, vibrato_data.Lowest.Mode);

            interval_mode.SelectedItem = vibrato_data.Interval.Mode;
            set_Selected(2, vibrato_data.Interval.Mode);

            Continuous_CheckBox.IsChecked = vibrato_data.Continuous;
        }

        private void selection_change(object sender, SelectionChangedEventArgs e)
        {
            if (no_update) return;

            ComboBox cb = (ComboBox)sender;
            Object? tag = cb.Tag;

            YamlAsyncParameterVibratoMode mode = (YamlAsyncParameterVibratoMode)cb.SelectedItem;
            if (tag.Equals("Highest"))
            {
                target.AsyncModulationData.CarrierWaveData.VibratoData.Highest.Mode = mode;
                set_Selected(0, mode);
            }
            else if(tag.Equals("Lowest"))
            {
                target.AsyncModulationData.CarrierWaveData.VibratoData.Lowest.Mode = mode;
                set_Selected(1, mode);
            }
            else if (tag.Equals("Interval"))
            {
                target.AsyncModulationData.CarrierWaveData.VibratoData.Interval.Mode = mode;
                set_Selected(2, mode);
            }

        }

        /// <summary>
        /// 0 => highest
        /// 1 => lowest
        /// 2 => interval
        /// </summary>
        /// <param name="cate"></param>
        /// <param name="mode"></param>
        private void set_Selected(int cate, YamlAsyncParameterVibratoMode mode)
        {
            if (cate == 0)
            {
                if(mode == YamlAsyncParameterVibratoMode.Const)
                    highest_param_frame.Navigate(new Control_Async_Vibrato_Const(target.AsyncModulationData.CarrierWaveData.VibratoData.Highest, main));
                else
                    highest_param_frame.Navigate(new Control_Moving_Setting(target.AsyncModulationData.CarrierWaveData.VibratoData.Highest.MovingValue));
            }
            else if(cate == 1)
            {
                if (mode == YamlAsyncParameterVibratoMode.Const)
                    lowest_param_frame.Navigate(new Control_Async_Vibrato_Const(target.AsyncModulationData.CarrierWaveData.VibratoData.Lowest, main));
                else
                    lowest_param_frame.Navigate(new Control_Moving_Setting(target.AsyncModulationData.CarrierWaveData.VibratoData.Lowest.MovingValue));
            }
            else if (cate == 2)
            {
                if (mode == YamlAsyncParameterVibratoMode.Const)
                    interval_mode_frame.Navigate(new Control_Async_Vibrato_Const(target.AsyncModulationData.CarrierWaveData.VibratoData.Interval, main));
                else
                    interval_mode_frame.Navigate(new Control_Moving_Setting(target.AsyncModulationData.CarrierWaveData.VibratoData.Interval.MovingValue));
            }
        }

        private void Continuous_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (no_update) return;

            target.AsyncModulationData.CarrierWaveData.VibratoData.Continuous = Continuous_CheckBox.IsChecked == true;
        }
    }
}

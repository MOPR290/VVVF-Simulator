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
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterCarrierFreq.Yaml_Async_Parameter_Carrier_Freq_Vibrato.Yaml_Async_Parameter_Vibrato_Value;

namespace VvvfSimulator.VVVF_Window.Control_Settings.Async.Vibrato
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
            Yaml_Async_Parameter_Vibrato_Mode[] modes = (Yaml_Async_Parameter_Vibrato_Mode[])Enum.GetValues(typeof(Yaml_Async_Parameter_Vibrato_Mode));
            highest_mode.ItemsSource = modes;
            lowest_mode.ItemsSource = modes;
            interval_mode.ItemsSource = modes;

            var vibrato_data = target.async_data.carrier_wave_data.vibrato_value;

            highest_mode.SelectedItem = vibrato_data.highest.mode;
            set_Selected(0, vibrato_data.highest.mode);

            lowest_mode.SelectedItem = vibrato_data.lowest.mode;
            set_Selected(1, vibrato_data.lowest.mode);

            interval_mode.SelectedItem = vibrato_data.interval.mode;
            set_Selected(2, vibrato_data.interval.mode);

            Continuous_CheckBox.IsChecked = vibrato_data.continuous;
        }

        private void selection_change(object sender, SelectionChangedEventArgs e)
        {
            if (no_update) return;

            ComboBox cb = (ComboBox)sender;
            Object? tag = cb.Tag;

            Yaml_Async_Parameter_Vibrato_Mode mode = (Yaml_Async_Parameter_Vibrato_Mode)cb.SelectedItem;
            if (tag.Equals("Highest"))
            {
                target.async_data.carrier_wave_data.vibrato_value.highest.mode = mode;
                set_Selected(0, mode);
            }
            else if(tag.Equals("Lowest"))
            {
                target.async_data.carrier_wave_data.vibrato_value.lowest.mode = mode;
                set_Selected(1, mode);
            }
            else if (tag.Equals("Interval"))
            {
                target.async_data.carrier_wave_data.vibrato_value.interval.mode = mode;
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
        private void set_Selected(int cate, Yaml_Async_Parameter_Vibrato_Mode mode)
        {
            if (cate == 0)
            {
                if(mode == Yaml_Async_Parameter_Vibrato_Mode.Const)
                    highest_param_frame.Navigate(new Control_Async_Vibrato_Const(target.async_data.carrier_wave_data.vibrato_value.highest, main));
                else
                    highest_param_frame.Navigate(new Control_Moving_Setting(target.async_data.carrier_wave_data.vibrato_value.highest.moving_value));
            }
            else if(cate == 1)
            {
                if (mode == Yaml_Async_Parameter_Vibrato_Mode.Const)
                    lowest_param_frame.Navigate(new Control_Async_Vibrato_Const(target.async_data.carrier_wave_data.vibrato_value.lowest, main));
                else
                    lowest_param_frame.Navigate(new Control_Moving_Setting(target.async_data.carrier_wave_data.vibrato_value.lowest.moving_value));
            }
            else if (cate == 2)
            {
                if (mode == Yaml_Async_Parameter_Vibrato_Mode.Const)
                    interval_mode_frame.Navigate(new Control_Async_Vibrato_Const(target.async_data.carrier_wave_data.vibrato_value.interval, main));
                else
                    interval_mode_frame.Navigate(new Control_Moving_Setting(target.async_data.carrier_wave_data.vibrato_value.interval.moving_value));
            }
        }

        private void Continuous_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (no_update) return;

            target.async_data.carrier_wave_data.vibrato_value.continuous = Continuous_CheckBox.IsChecked == true;
        }
    }
}

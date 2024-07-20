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
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterDipolar;

namespace VvvfSimulator.GUI.Create.Waveform.Dipolar
{
    /// <summary>
    /// Control_Dipolar.xaml の相互作用ロジック
    /// </summary>
    public partial class ControlDipolar : UserControl
    {
        YamlControlData target;
        bool no_update = true;

        public ControlDipolar(YamlControlData ycd)
        {
            target = ycd;
            InitializeComponent();
            apply_data();
            no_update = false;
        }

        private void apply_data()
        {
            YamlAsyncParameterDipolarMode[] modes = (YamlAsyncParameterDipolarMode[])Enum.GetValues(typeof(YamlAsyncParameterDipolarMode));
            dipolar_mode.ItemsSource = modes;
            dipolar_mode.SelectedItem = target.AsyncModulationData.DipolarData.Mode;

            set_Selected(target.AsyncModulationData.DipolarData.Mode);
        }

        private void dipolar_mode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (no_update) return;

            YamlAsyncParameterDipolarMode selected = (YamlAsyncParameterDipolarMode)dipolar_mode.SelectedItem;

            target.AsyncModulationData.DipolarData.Mode = selected;

            set_Selected(selected);
        }

        private void set_Selected(YamlAsyncParameterDipolarMode selected)
        {
            YamlControlData.YamlAsyncParameter.YamlAsyncParameterDipolar value = target.AsyncModulationData.DipolarData;
            if (selected == YamlAsyncParameterDipolarMode.Const)
                dipolar_param.Navigate(new ControlConstSetting(value.GetType(), value));
            else
                dipolar_param.Navigate(new ControlMovingSetting(target.AsyncModulationData.DipolarData.MovingValue));
        }
    }
}

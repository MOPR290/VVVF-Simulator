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
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterDipolar;

namespace VvvfSimulator.GUI.Create.Waveform.Dipolar
{
    /// <summary>
    /// Control_Dipolar.xaml の相互作用ロジック
    /// </summary>
    public partial class Control_Dipolar : UserControl
    {
        YamlControlData target;
        MainWindow main;

        bool no_update = true;

        public Control_Dipolar(YamlControlData ycd, MainWindow mainWindow)
        {
            target = ycd;
            main = mainWindow;

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
            if (selected == YamlAsyncParameterDipolarMode.Const)
                dipolar_param.Navigate(new Control_Dipolar_Const(target, main));
            else
                dipolar_param.Navigate(new Control_Moving_Setting(target.AsyncModulationData.DipolarData.MovingValue));
        }
    }
}

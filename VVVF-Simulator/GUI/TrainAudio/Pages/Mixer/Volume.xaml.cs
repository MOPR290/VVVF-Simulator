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
using static VvvfSimulator.Yaml.TrainAudio_Setting.YamlTrainSoundAnalyze;

namespace VvvfSimulator.GUI.TrainAudio.Pages.Mixer
{
    /// <summary>
    /// Volume.xaml の相互作用ロジック
    /// </summary>
    public partial class Volume : Page
    {
        readonly YamlTrainSoundData data;
        public Volume(YamlTrainSoundData data)
        {
            this.data = data;
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            MasterVolume.Value = data.TotalVolumeDb;
            MotorVolume.Value = data.MotorVolumeDb;
            EnableFrequencyFilter.SetToggled(data.UseFilteres);
            EnableIrFilter.SetToggled(data.UseImpulseResponse);
        }

        private void MasterVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            data.TotalVolumeDb = (double)e.NewValue;
        }

        private void MotorVolue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            data.MotorVolumeDb = (double)e.NewValue;
        }

        private void EnableFrequencyFilter_OnClicked(object sender, EventArgs e)
        {
            data.UseFilteres = EnableFrequencyFilter.IsToggled();
        }

        private void EnableIrFilter_OnClicked(object sender, EventArgs e)
        {
            data.UseImpulseResponse = EnableIrFilter.IsToggled();
        }
    }
}

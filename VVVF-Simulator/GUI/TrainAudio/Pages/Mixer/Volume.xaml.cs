using System;
using System.Windows;
using System.Windows.Controls;
using static VvvfSimulator.Yaml.TrainAudioSetting.YamlTrainSoundAnalyze;

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

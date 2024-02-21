using System;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.Generation.Audio.TrainSound;
using static VvvfSimulator.Yaml.TrainAudio_Setting.YamlTrainSoundAnalyze;

namespace VvvfSimulator.GUI.TrainAudio.Pages.Mixer
{
    /// <summary>
    /// ImpulseResponse.xaml の相互作用ロジック
    /// </summary>
    public partial class ImpulseResponse : Page
    {
        readonly YamlTrainSoundData data;
        public ImpulseResponse(YamlTrainSoundData data)
        {
            this.data = data;
            InitializeComponent();
        }

        private void OnLoadButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Wav (*.wav)|*.wav|All (*.*)|*.*",
            };
            if (dialog.ShowDialog() == false)
            {
                data.ImpulseResponse = Generation.Audio.TrainSound.ImpulseResponse.ReadResourceAudioFileSample(Generation.Audio.TrainSound.ImpulseResponse.SampleIrPath);
                MessageBox.Show("Reset to default.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); return;
            }

            string path = dialog.FileName;
            try
            {
                data.ImpulseResponse = Generation.Audio.TrainSound.ImpulseResponse.ReadAudioFileSample(path);
                MessageBox.Show("Load OK.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); return;
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); return;
            }
        }
    }
}

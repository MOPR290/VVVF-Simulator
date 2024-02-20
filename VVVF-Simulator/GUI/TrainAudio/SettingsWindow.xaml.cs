using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.TrainAudio.Pages.Mixer;
using VvvfSimulator.GUI.TrainAudio.Pages.Gear;
using VvvfSimulator.GUI.TrainAudio.Pages.Motor;
using YamlDotNet.Core;
using static VvvfSimulator.Yaml.TrainAudio_Setting.YamlTrainSoundAnalyze;
using System.Diagnostics;

namespace VvvfSimulator.GUI.TrainAudio
{
    /// <summary>
    /// TrainAudio_Harmonic_Window.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private YamlTrainSoundData soundData;
        public SettingsWindow(YamlTrainSoundData thd)
        {
            soundData = thd;
            InitializeComponent();
            PageFrame.Navigate(new MotorSetting(soundData));
        }
        private void MenuParameterClick(object sender, RoutedEventArgs e)
        {
            MenuItem btn = (MenuItem)sender;
            object tag = btn.Tag;

            if (tag.Equals("Gear"))
                PageFrame.Navigate(new GearSetting(soundData));
            else if (tag.Equals("Motor"))
                PageFrame.Navigate(new MotorSetting(soundData));

        }
        private string load_path = "acoustic.yaml";
        private void MenuFileClick(object sender, RoutedEventArgs e)
        {
            MenuItem btn = (MenuItem)sender;
            object tag = btn.Tag;

            if (tag.Equals("Open"))
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml|All (*.*)|*.*"
                };
                if (dialog.ShowDialog() == false) return;

                try
                {
                    
                    YamlTrainSoundDataManage.CurrentData = YamlTrainSoundDataManage.LoadYaml(dialog.FileName);
                    this.soundData = YamlTrainSoundDataManage.CurrentData;
                    PageFrame.Navigate(new MotorSetting(soundData));
                    MessageBox.Show("Load OK.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (YamlException ex)
                {
                    String error_message = "";
                    error_message += "Invalid yaml\r\n";
                    error_message += "\r\n" + ex.End.ToString() + "\r\n";
                    MessageBox.Show(error_message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }


                load_path = dialog.FileName;
                return;
            }

            if (tag.Equals("Save"))
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml",
                    FileName = Path.GetFileName(load_path)
                };
                if (dialog.ShowDialog() == false) return;

                if (YamlTrainSoundDataManage.SaveYaml(dialog.FileName))
                    MessageBox.Show("Save OK.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Error occurred on saving.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void MenuMixerClick(object sender, RoutedEventArgs e)
        {
            MenuItem btn = (MenuItem)sender;
            object tag = btn.Tag;

            if (tag.Equals("Frequency"))
                PageFrame.Navigate(new FrequencyFilter(soundData));
            else if (tag.Equals("Ir"))
                PageFrame.Navigate(new ImpulseResponse(soundData));
            else if (tag.Equals("Volume"))
                PageFrame.Navigate(new Volume(soundData));
        }
    }
}

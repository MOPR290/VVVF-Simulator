using System;
using System.Windows.Controls;
using static VvvfSimulator.Yaml.TrainAudioSetting.YamlTrainSoundAnalyze;
using static VvvfSimulator.Yaml.TrainAudioSetting.YamlTrainSoundAnalyze.YamlTrainSoundData.SoundFilter;

namespace VvvfSimulator.GUI.TrainAudio.Pages.Mixer
{
    /// <summary>
    /// TrainAudio_Filter_Setting_Page.xaml の相互作用ロジック
    /// </summary>
    public partial class FrequencyFilter : Page
    {
        readonly YamlTrainSoundData data;
        public FrequencyFilter(YamlTrainSoundData train_Harmonic_Data)
        {
            data = train_Harmonic_Data;

            InitializeComponent();

            try
            {
                filterType_Selector.ItemsSource = (FilterType[])Enum.GetValues(typeof(FilterType));
                Filter_DataGrid.ItemsSource = data.Filteres;
            }
            catch
            {

            }
        }
    }
}

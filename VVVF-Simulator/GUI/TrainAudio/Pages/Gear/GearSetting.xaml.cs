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
using static VvvfSimulator.Yaml.TrainAudioSetting.YamlTrainSoundAnalyze;

namespace VvvfSimulator.GUI.TrainAudio.Pages.Gear
{
    /// <summary>
    /// TrainAudio_Gear_Setting_Page.xaml の相互作用ロジック
    /// </summary>
    public partial class GearSetting : Page
    {
        YamlTrainSoundData train_Harmonic_Data;
        public GearSetting(YamlTrainSoundData train_Harmonic_Data)
        {
            this.train_Harmonic_Data = train_Harmonic_Data;
            InitializeComponent();

            Update_ListView();
        }


        private void Update_ListView()
        {
            Gear_Sound_List.ItemsSource = train_Harmonic_Data.GearSound;
            Gear_Sound_List.Items.Refresh();

            var item = (YamlTrainSoundData.HarmonicData)Gear_Sound_List.SelectedItem;
            if (item == null) return;
            Gear_Edit_Frame.Navigate(new HarmonicSetting(item, Gear_Sound_List));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            Object tag = mi.Header;

            if (tag.Equals("Add"))
            {
                train_Harmonic_Data.GearSound.Add(new YamlTrainSoundData.HarmonicData());
                Update_ListView();
            }
            else if (tag.Equals("Remove"))
            {
                if (Gear_Sound_List.SelectedIndex < 0) return;
                Gear_Edit_Frame.Navigate(null);
                train_Harmonic_Data.GearSound.RemoveAt(Gear_Sound_List.SelectedIndex);
                Update_ListView();
            }
            else if (tag.Equals("Copy"))
            {
                if (Gear_Sound_List.SelectedIndex < 0) return;
                YamlTrainSoundData.HarmonicData harmonic_Data = (YamlTrainSoundData.HarmonicData)Gear_Sound_List.SelectedItem;
                train_Harmonic_Data.GearSound.Add(harmonic_Data.Clone());
                Update_ListView();
            }
            else if (tag.Equals("Calculate"))
            {
                GearCalculate taggw = new(16,101);
                taggw.ShowDialog();
                train_Harmonic_Data.SetCalculatedGearHarmonics(taggw.Gear1, taggw.Gear2);
                Update_ListView();
            }
        }

        private void Gear_Sound_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (YamlTrainSoundData.HarmonicData)Gear_Sound_List.SelectedItem;
            if (item == null) return;
            Gear_Edit_Frame.Navigate(new HarmonicSetting(item, Gear_Sound_List));
        }
    }
}

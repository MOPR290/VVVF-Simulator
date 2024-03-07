using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using static VvvfSimulator.VvvfStructs;
using static VvvfSimulator.VvvfStructs.PulseMode;

namespace VvvfSimulator.GUI.Pages.Control_Settings.Basic
{
    /// <summary>
    /// Control_Basic_Harmonic.xaml の相互作用ロジック
    /// </summary>
    public partial class Control_Basic_Harmonic : Window
    {
        View_Data vd = new View_Data();
        public class View_Data : ViewModelBase
        {
            public List<PulseHarmonic> _harmonic_data = new List<PulseHarmonic>();
            public List<PulseHarmonic> harmonic_data { get { return _harmonic_data; } set { _harmonic_data = value; RaisePropertyChanged(nameof(harmonic_data)); } }
        }
        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //
        //Harmonic Presets
        public enum Preset_Harmonics
        {
            THI, SVM, DPM1, DPM2, DPM3, DPM4, Square_Fourier
        }

        public List<PulseHarmonic> Get_Preset_Harmonics(Preset_Harmonics harmonic)
        {
            switch (harmonic)
            {
                case Preset_Harmonics.THI:
                    return new List<PulseHarmonic>() { 
                        new PulseHarmonic() { Amplitude = 0.2, Harmonic = 3 } 
                    };
                case Preset_Harmonics.SVM:
                    return new List<PulseHarmonic>() { 
                        new PulseHarmonic() { Amplitude = 0.25, Harmonic = 3 , Type = PulseHarmonic.PulseHarmonicType.Saw} 
                    };
                case Preset_Harmonics.DPM1:
                    return new List<PulseHarmonic>() {
                        new PulseHarmonic() { Amplitude = -0.05, Harmonic = 3 },
                        new PulseHarmonic() { Amplitude = 0.2, Harmonic = 3, Type = PulseHarmonic.PulseHarmonicType.Square }
                    };
                case Preset_Harmonics.DPM2:
                    return new List<PulseHarmonic>() {
                        new PulseHarmonic() { Amplitude = -0.05, Harmonic = 3, InitialPhase = 1.57079633, Type = PulseHarmonic.PulseHarmonicType.Saw},
                        new PulseHarmonic() { Amplitude = 0.2, Harmonic = 3, Type = PulseHarmonic.PulseHarmonicType.Square }
                    };
                case Preset_Harmonics.DPM3:
                    return new List<PulseHarmonic>() {
                        new PulseHarmonic() { Amplitude = -0.05, Harmonic = 3, InitialPhase = -1.57079633, Type = PulseHarmonic.PulseHarmonicType.Saw},
                        new PulseHarmonic() { Amplitude = 0.2, Harmonic = 3, Type = PulseHarmonic.PulseHarmonicType.Square }
                    };
                case Preset_Harmonics.DPM4: //case Preset_Harmonics.DPM4:
                    return new List<PulseHarmonic>() {
                        new PulseHarmonic() { Amplitude = 0.05, Harmonic = 3, Type = PulseHarmonic.PulseHarmonicType.Saw},
                        new PulseHarmonic() { Amplitude = 0.2, Harmonic = 3, Type = PulseHarmonic.PulseHarmonicType.Square }
                    };
                default:
                    List<PulseHarmonic> harmonics = new();
                    for (int i = 0; i < 10; i++)
                    {
                        harmonics.Add(new PulseHarmonic() { Amplitude = 1.0 / (2 * i + 3), Harmonic = 2 * i + 3 });
                    }
                    return harmonics;


            }
        }


        //
        //
        //


        PulseMode target;
        private bool no_update = true;
        public Control_Basic_Harmonic(PulseMode data)
        {

            vd.harmonic_data = data.PulseHarmonics;
            DataContext = vd;
            target = data;

            InitializeComponent();

            Preset_Selector.ItemsSource = (Preset_Harmonics[])Enum.GetValues(typeof(Preset_Harmonics));
            Preset_Selector.SelectedIndex = 0;

            no_update = false;
        }

        private void DataGrid_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (no_update) return;
            target.PulseHarmonics = vd.harmonic_data;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Harmonic_Editor.CommitEdit();
        }

        private void Harmonic_Editor_Unloaded(object sender, RoutedEventArgs e)
        {
            Harmonic_Editor.CommitEdit();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Object tag = btn.Tag;

            Preset_Harmonics selected = (Preset_Harmonics)Preset_Selector.SelectedItem;
            List<PulseHarmonic> harmonics = Get_Preset_Harmonics(selected);

            if (tag.Equals("Add"))
                vd.harmonic_data.AddRange(harmonics);
            else if (tag.Equals("Set"))
                vd.harmonic_data = harmonics;

            Harmonic_Editor.Items.Refresh();

        }
    }
}

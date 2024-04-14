using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
using VvvfSimulator.Yaml.VvvfSound;
using YamlDotNet.Core.Tokens;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlMasconData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlMasconData.YamlMasconDataPattern;

namespace VvvfSimulator.GUI.Create.Settings
{
    /// <summary>
    /// mascon_off_setting.xaml の相互作用ロジック
    /// </summary>
    public partial class jerk_setting : Page
    {
        private class Controller : INotifyPropertyChanged
        {
            private bool _IsAccelerateActive = true;
            public bool IsAccelerateActive { get { return _IsAccelerateActive; } set { _IsAccelerateActive = value; RaisePropertyChanged(nameof(IsAccelerateActive)); } }

            private bool _IsTurnOnActive = true;
            public bool IsTurnOnActive
            {
                get { return _IsTurnOnActive; }
                set { _IsTurnOnActive = value; RaisePropertyChanged(nameof(IsTurnOnActive)); }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void updateView()
        {
            Controller dc = (Controller)this.DataContext;
            YamlMasconData mascon = YamlVvvfManage.CurrentData.MasconData;
            YamlMasconDataPattern pattern = dc.IsAccelerateActive ? mascon.Accelerating : mascon.Braking;
            YamlMasconDataPatternMode mode = dc.IsTurnOnActive ? pattern.On : pattern.Off;
            MaxVoltageFreqInput.Text = mode.MaxControlFrequency.ToString();
            FreqChangeRateInput.Text = mode.FrequencyChangeRate.ToString();
        }

        public void setValue()
        {
            double _parse(TextBox tb)
            {
                try
                {
                    double b = Double.Parse(tb.Text);
                    tb.Background = new BrushConverter().ConvertFrom("#FFFFFFFF") as Brush;
                    return b;
                }
                catch
                {
                    tb.Background = new BrushConverter().ConvertFrom("#FFfed0d0") as Brush;
                    return 0;
                }                
            }

            Controller dc = (Controller)this.DataContext;
            if(dc == null) return;

            YamlMasconData mascon = YamlVvvfManage.CurrentData.MasconData;
            YamlMasconDataPattern pattern = dc.IsAccelerateActive ? mascon.Accelerating : mascon.Braking;
            YamlMasconDataPatternMode mode = dc.IsTurnOnActive ? pattern.On : pattern.Off;
            mode.MaxControlFrequency = _parse(MaxVoltageFreqInput);
            mode.FrequencyChangeRate = _parse(FreqChangeRateInput);
        }

        public jerk_setting()
        {
            InitializeComponent();
            DataContext = new Controller();
            updateView();
        }

        private void ValueUpdated(object sender, TextChangedEventArgs e)
        {

            setValue();

        }

        private void onClick(object sender, RoutedEventArgs e)
        {
            
            Button button = (Button)sender;
            Controller dc = (Controller)this.DataContext;

            string name = button.Name;
            if (name.Equals("ButtonModeAccelerate")) dc.IsAccelerateActive = true;
            else if (name.Equals("ButtonModeBrake")) dc.IsAccelerateActive = false;
            if (name.Equals("ButtonTurnOn")) dc.IsTurnOnActive = true;
            else if (name.Equals("ButtonTurnOff")) dc.IsTurnOnActive = false;

            updateView();

        }
    }
}

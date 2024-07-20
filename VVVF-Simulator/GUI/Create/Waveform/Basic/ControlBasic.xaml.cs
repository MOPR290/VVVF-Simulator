using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VvvfSimulator.GUI.Create.Waveform.Basic;
using static VvvfSimulator.VvvfStructs;
using static VvvfSimulator.VvvfStructs.PulseMode;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData;

namespace VvvfSimulator.GUI.Create.Waveform.Basic
{
    /// <summary>
    /// Control_Basic.xaml の相互作用ロジック
    /// </summary>
    public partial class ControlBasic : UserControl
    {
        private YamlControlData target;
        private int level;


        private ViewModel viewModel = new();
        private class ViewModel : ViewModelBase
        {
            private bool _Harmonic_Visible = true;
            public bool Harmonic_Visible { get { return _Harmonic_Visible; } set { _Harmonic_Visible = value; RaisePropertyChanged(nameof(Harmonic_Visible)); } }

            private bool _Base_Wave_Selector_Visible = true;
            public bool Base_Wave_Selector_Visible { get { return _Base_Wave_Selector_Visible; } set { _Base_Wave_Selector_Visible = value; RaisePropertyChanged(nameof(Base_Wave_Selector_Visible)); } }

            private bool _Alt_Mode_Selector_Visible = true;
            public bool Alt_Mode_Selector_Visible { get { return _Alt_Mode_Selector_Visible; } set { _Alt_Mode_Selector_Visible = value; RaisePropertyChanged(nameof(Alt_Mode_Selector_Visible)); } }

            private bool _Shifted_Visible = true;
            public bool Shifted_Visible { get { return _Shifted_Visible; } set { _Shifted_Visible = value; RaisePropertyChanged(nameof(Shifted_Visible)); } }

            private bool _Square_Visible = true;
            public bool Square_Visible { get { return _Square_Visible; } set { _Square_Visible = value; RaisePropertyChanged(nameof(Square_Visible)); } }

            private bool _Discrete_Visible = true;
            public bool Discrete_Visible { get { return _Discrete_Visible; } set { _Discrete_Visible = value; RaisePropertyChanged(nameof(Discrete_Visible)); } }

        }
        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private bool IgnoreUpdate = true;
        public ControlBasic(YamlControlData ycd, int level)
        {
            InitializeComponent();

            target = ycd;
            this.level = level;
            DataContext = viewModel;

            Apply_view();

            IgnoreUpdate = false;
        }

        private static double ParseDouble(TextBox tb)
        {
            try
            {
                VisualStateManager.GoToState(tb, "Success", false);
                return double.Parse(tb.Text);
            }
            catch
            {
                VisualStateManager.GoToState(tb, "Error", false);
                return 0;
            }
        }

        private void Apply_view()
        {
            from_text_box.Text = target.ControlFrequencyFrom.ToString();
            sine_from_text_box.Text = target.RotateFrequencyFrom.ToString();
            sine_below_text_box.Text = target.RotateFrequencyBelow.ToString();

            Pulse_Name_Selector.ItemsSource = PulseModeConfiguration.ValidPulseModeNames(level);
            Pulse_Name_Selector.SelectedItem = target.PulseMode.PulseName;

            Shifted_Box.IsChecked = target.PulseMode.Shift;
            Square_Box.IsChecked = target.PulseMode.Square;

            Base_Wave_Selector.ItemsSource = (BaseWaveType[])Enum.GetValues(typeof(BaseWaveType));
            Base_Wave_Selector.SelectedItem = target.PulseMode.BaseWave;

            Alt_Mode_Selector.ItemsSource = PulseModeConfiguration.GetPulseAltModes(target.PulseMode, level);
            Alt_Mode_Selector.SelectedItem = target.PulseMode.AltMode;

            Enable_FreeRun_On_Check.IsChecked = target.EnableFreeRunOn;
            Enable_FreeRun_Off_Check.IsChecked = target.EnableFreeRunOff;
            Enable_Normal_Check.IsChecked = target.EnableNormal;

            Set_Control();
        }

        private void Set_Control()
        {
            PulseMode mode = target.PulseMode;

            viewModel.Harmonic_Visible = PulseModeConfiguration.IsPulseHarmonicBaseWaveChangeAvailable(mode, level);
            viewModel.Shifted_Visible = PulseModeConfiguration.IsPulseShiftedAvailable(mode, level);
            viewModel.Base_Wave_Selector_Visible = PulseModeConfiguration.IsPulseHarmonicBaseWaveChangeAvailable(mode, level);
            viewModel.Square_Visible = PulseModeConfiguration.IsPulseSquareAvail(mode, level);
            viewModel.Discrete_Visible = PulseModeConfiguration.IsDiscreteTimeValid(mode, level);

            List<PulseAlternativeMode> modes = PulseModeConfiguration.GetPulseAltModes(target.PulseMode, level);
            Alt_Mode_Selector.ItemsSource = modes;
            if (!Alt_Mode_Selector.Items.Contains(Alt_Mode_Selector.SelectedItem))
            {
                Alt_Mode_Selector.SelectedIndex = 0;
                PulseAlternativeMode selected = (PulseAlternativeMode)Alt_Mode_Selector.SelectedItem;
                target.PulseMode.AltMode = selected;
            }

            if (modes.Count == 1 && modes[0] == PulseAlternativeMode.Default)
                viewModel.Alt_Mode_Selector_Visible = false;
            else
                viewModel.Alt_Mode_Selector_Visible = true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IgnoreUpdate) return;

            TextBox tb = (TextBox)sender;
            Object? tag = tb.Tag;
            if (tag == null) return;

            if (tag.Equals("From"))
            {
                double parsed = ParseDouble(tb);
                target.ControlFrequencyFrom = parsed;
                MainWindow.GetInstance()?.UpdateControlList();
            }
            else if (tag.Equals("SineFrom"))
            {
                double parsed = ParseDouble(tb);
                target.RotateFrequencyFrom = parsed;
                MainWindow.GetInstance()?.UpdateControlList();
            }
            else if (tag.Equals("SineBelow"))
            {
                double parsed = ParseDouble(tb);
                target.RotateFrequencyBelow = parsed;
                MainWindow.GetInstance()?.UpdateControlList();
            }
        }

        private void enable_checked(object sender, RoutedEventArgs e)
        {
            if (IgnoreUpdate) return;

            CheckBox tb = (CheckBox)sender;
            Object? tag = tb.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;

            bool check = tb.IsChecked != false;

            if (tag_str.Equals("Normal"))
                target.EnableNormal = check;
            else if (tag_str.Equals("FreeRunOn"))
                target.EnableFreeRunOn = check;
            else if (tag_str.Equals("FreeRunOff"))
                target.EnableFreeRunOff = check;
            else if (tag_str.Equals("Shifted"))
                target.PulseMode.Shift = check;
            else if (tag_str.Equals("Square"))
                target.PulseMode.Square = check;

            Set_Control();
            MainWindow.GetInstance()?.UpdateControlList();
            MainWindow.GetInstance()?.UpdateContentSelected();
            return;
        }

        private void Open_Harmonic_Setting_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SetInteractive(false);
            ControlBasicHarmonic cbh = new(MainWindow.GetInstance(), target.PulseMode);
            cbh.ShowDialog();
            MainWindow.SetInteractive(true);
        }

        private void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreUpdate) return;

            ComboBox cb = (ComboBox)sender;
            Object tag = cb.Tag;

            if (tag.Equals("PulseName"))
            {
                PulseModeNames selected = (PulseModeNames)cb.SelectedItem;
                target.PulseMode.PulseName = selected;
                MainWindow.GetInstance()?.UpdateControlList();
                MainWindow.GetInstance()?.UpdateContentSelected();
                return;
            }
            else if(tag.Equals("BaseWave"))
            {
                BaseWaveType selected = (BaseWaveType)cb.SelectedItem;
                target.PulseMode.BaseWave = selected;
            }
            else if (tag.Equals("AltMode"))
            {
                PulseAlternativeMode selected = (PulseAlternativeMode)cb.SelectedItem;
                target.PulseMode.AltMode = selected;
            }

            MainWindow.GetInstance()?.UpdateControlList();
            Set_Control();
        }

        private void Open_Discrete_Setting_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SetInteractive(false);
            DiscreteSettingWindow discreteSetting = new(MainWindow.GetInstance(), target.PulseMode);
            discreteSetting.ShowDialog();
            MainWindow.SetInteractive(true);
        }
    }
}

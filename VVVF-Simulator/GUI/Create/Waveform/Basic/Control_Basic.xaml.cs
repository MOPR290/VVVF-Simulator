using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VvvfSimulator.GUI.Pages.Control_Settings.Basic;
using static VvvfSimulator.VvvfStructs;
using static VvvfSimulator.VvvfStructs.PulseMode;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData;

namespace VvvfSimulator.GUI.Create.Waveform.Basic
{
    /// <summary>
    /// Control_Basic.xaml の相互作用ロジック
    /// </summary>
    public partial class Control_Basic : UserControl
    {
        private YamlControlData target;
        private MainWindow MainWindow;
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
        }
        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private bool no_update = true;
        public Control_Basic(YamlControlData ycd, MainWindow mainWindow, int level)
        {
            InitializeComponent();

            target = ycd;
            MainWindow = mainWindow;
            this.level = level;
            DataContext = viewModel;

            apply_view();

            no_update = false;
        }

        private double parse(TextBox tb)
        {
            try
            {
                tb.Background = new BrushConverter().ConvertFrom("#FFFFFFFF") as Brush;
                return Double.Parse(tb.Text);
            }
            catch
            {
                tb.Background = new BrushConverter().ConvertFrom("#FFfed0d0") as Brush;
                return -1;
            }
        }

        private void apply_view()
        {
            from_text_box.Text = target.ControlFrequencyFrom.ToString();
            sine_from_text_box.Text = target.RotateFrequencyFrom.ToString();
            sine_below_text_box.Text = target.RotateFrequencyBelow.ToString();

            Pulse_Name_Selector.ItemsSource = (PulseModeNames[])Enum.GetValues(typeof(PulseModeNames));
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
            if (no_update) return;

            TextBox tb = (TextBox)sender;
            Object? tag = tb.Tag;
            if (tag == null) return;

            if (tag.Equals("From"))
            {
                double parsed = parse(tb);
                target.ControlFrequencyFrom = parsed;
                MainWindow.UpdateControlList();
            }
            else if (tag.Equals("SineFrom"))
            {
                double parsed = parse(tb);
                target.RotateFrequencyFrom = parsed;
                MainWindow.UpdateControlList();
            }
            else if (tag.Equals("SineBelow"))
            {
                double parsed = parse(tb);
                target.RotateFrequencyBelow = parsed;
                MainWindow.UpdateControlList();
            }
        }

        private void enable_checked(object sender, RoutedEventArgs e)
        {
            if (no_update) return;

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
            MainWindow.UpdateControlList();
            MainWindow.UpdateContentSelected();
            return;
        }

        private void Open_Harmonic_Setting_Button_Click(object sender, RoutedEventArgs e)
        {
            Control_Basic_Harmonic cbh = new(target.PulseMode);
            cbh.Show();
        }

        private void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (no_update) return;

            ComboBox cb = (ComboBox)sender;
            Object tag = cb.Tag;

            if (tag.Equals("PulseName"))
            {
                PulseModeNames selected = (PulseModeNames)cb.SelectedItem;
                target.PulseMode.PulseName = selected;
                MainWindow.UpdateControlList();
                MainWindow.UpdateContentSelected();
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

            MainWindow.UpdateControlList();
            Set_Control();
        }
    }
}

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VvvfSimulator;
using VvvfSimulator.GUI.Util;
using static VvvfSimulator.VvvfCalculate;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlControlDataAmplitudeControl;

namespace VvvfSimulator.GUI.Create.Waveform
{
    /// <summary>
    /// Control_Amplitude.xaml の相互作用ロジック
    /// </summary>
    public partial class ControlAmplitude : UserControl
    {
        private readonly YamlControlDataAmplitude Context;
        private readonly ControlAmplitudeContent ContextType;

        private readonly bool IgnoreUpdate = true;
        private readonly VisibleClass VisibleHandler;

        public class VisibleClass : ViewModelBase
        {
            private bool _start_freq_visible = true;
            public bool start_freq_visible { get { return _start_freq_visible; } set { _start_freq_visible = value; RaisePropertyChanged(nameof(start_freq_visible)); } }
            
            private bool _start_amp_visible = true;
            public bool start_amp_visible { get { return _start_amp_visible; } set { _start_amp_visible = value; RaisePropertyChanged(nameof(start_amp_visible)); } }

            private bool _end_freq_visible = true;
            public bool end_freq_visible { get { return _end_freq_visible; } set { _end_freq_visible = value; RaisePropertyChanged(nameof(end_freq_visible)); } }

            private bool _end_amp_visible = true;
            public bool end_amp_visible { get { return _end_amp_visible; } set { _end_amp_visible = value; RaisePropertyChanged(nameof(end_amp_visible)); } }

            private bool _cut_off_amp_visible = true;
            public bool cut_off_amp_visible { get { return _cut_off_amp_visible; } set { _cut_off_amp_visible = value; RaisePropertyChanged(nameof(cut_off_amp_visible)); } }

            private bool _max_amp_visible = true;
            public bool max_amp_visible { get { return _max_amp_visible; } set { _max_amp_visible = value; RaisePropertyChanged(nameof(max_amp_visible)); } }

            private bool _polynomial_visible = true;
            public bool polynomial_visible { get { return _polynomial_visible; } set { _polynomial_visible = value; RaisePropertyChanged(nameof(polynomial_visible)); } }

            private bool _curve_rate_visible = true;
            public bool curve_rate_visible { get { return _curve_rate_visible; } set { _curve_rate_visible = value; RaisePropertyChanged(nameof(curve_rate_visible)); } }

            private bool _disable_range_visible = true;
            public bool disable_range_visible { get { return _disable_range_visible; } set { _disable_range_visible = value; RaisePropertyChanged(nameof(disable_range_visible)); } }
        };
        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ControlAmplitude(YamlControlDataAmplitude ycd, ControlAmplitudeContent cac)
        {
            Context = ycd;
            ContextType = cac;

            InitializeComponent();

            if (cac == ControlAmplitudeContent.Default)
                title.Content = "Default Amplitude Setting";
                
            else if (cac == ControlAmplitudeContent.Free_Run_On)
                title.Content = "Mascon On Free Run Amplitude Setting";

            else
                title.Content = "Mascon Off Free Run Amplitude Setting";

            VisibleHandler = new VisibleClass();
            DataContext = VisibleHandler;
            ApplySettingToView();

            IgnoreUpdate = false;
        }

        private void ApplySettingToView()
        {
            AmplitudeMode[] modes = (AmplitudeMode[])Enum.GetValues(typeof(AmplitudeMode));
            amplitude_mode_selector.ItemsSource = modes;
            amplitude_mode_selector.SelectedItem = Context.Mode;

            start_freq_box.Text = Context.Parameter.StartFrequency.ToString();
            start_amp_box.Text = Context.Parameter.StartAmplitude.ToString();
            end_freq_box.Text = Context.Parameter.EndFrequency.ToString();
            end_amp_box.Text = Context.Parameter.EndAmplitude.ToString();
            cutoff_amp_box.Text = Context.Parameter.CutOffAmplitude.ToString();
            max_amp_box.Text = Context.Parameter.MaxAmplitude.ToString();
            polynomial_box.Text = Context.Parameter.Polynomial.ToString();
            curve_rate_box.Text = Context.Parameter.CurveChangeRate.ToString();
            disable_range_limit_check.IsChecked = Context.Parameter.DisableRangeLimit;

            SetGridVisibility(Context.Mode, ContextType);
        }

        private void TextboxUpdate(object sender, TextChangedEventArgs e)
        {
            if (IgnoreUpdate) return;

            TextBox tb = (TextBox)sender;
            Object? tag = tb.Tag;
            if (tag == null) return;

            if (tag.Equals("start_freq"))
                Context.Parameter.StartFrequency = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("start_amp"))
                Context.Parameter.StartAmplitude = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("end_freq"))
                Context.Parameter.EndFrequency = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("end_amp"))
                Context.Parameter.EndAmplitude = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("cutoff_amp"))
                Context.Parameter.CutOffAmplitude = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("max_amp"))
                Context.Parameter.MaxAmplitude = ParseTextBox.ParseDouble(tb);
            else if(tag.Equals("curve_rate"))
                Context.Parameter.CurveChangeRate = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("polynomial"))
                Context.Parameter.Polynomial = ParseTextBox.ParseDouble(tb);

            MainWindow.GetInstance()?.UpdateControlList();
        }

        private void CheckBoxUpdate(object sender, RoutedEventArgs e)
        {
            if (IgnoreUpdate) return;

            CheckBox cb = (CheckBox)sender;
            Context.Parameter.DisableRangeLimit = cb.IsChecked != false;
            MainWindow.GetInstance()?.UpdateControlList();
        }

        private void AmplitudeModeSelectorUpdate(object sender, RoutedEventArgs e)
        {
            if (IgnoreUpdate) return;

            AmplitudeMode selected = (AmplitudeMode)amplitude_mode_selector.SelectedItem;
            Context.Mode = selected;
            SetGridVisibility(Context.Mode, ContextType);

            MainWindow.GetInstance()?.UpdateControlList();

            
        }

        private Grid GetGrid(int i)
        {
            return i switch
            {
                0 => start_freq_grid,
                1 => start_amp_grid,
                2 => end_freq_grid,
                3 => end_amp_grid,
                4 => cut_off_amp_grid,
                5 => max_amp_grid,
                6 => polynomial_grid,
                7 => curve_change_grid,
                _ => disable_range_grid,
            };
        }
        private void SetGridVisibility(AmplitudeMode mode , ControlAmplitudeContent cac)
        {
            void _SetVisiblity(int i, bool b)
            {
                if (i == 0) VisibleHandler.start_freq_visible = b;
                else if (i == 1) VisibleHandler.start_amp_visible = b;
                else if (i == 2) VisibleHandler.end_freq_visible = b;
                else if (i == 3) VisibleHandler.end_amp_visible = b;
                else if (i == 4) VisibleHandler.cut_off_amp_visible = b;
                else if (i == 5) VisibleHandler.max_amp_visible = b;
                else if (i == 6) VisibleHandler.polynomial_visible = b;
                else if (i == 7) VisibleHandler.curve_rate_visible = b;
                else VisibleHandler.disable_range_visible = b;
            }

            Boolean[] condition_1, condition_2;

            if (mode == AmplitudeMode.Linear)
                condition_1 = [true, true, true, true, true, true, false, false, true];
            else if (mode == AmplitudeMode.Wide_3_Pulse)
                condition_1 = [true, true, true, true, true, true, false, false, true];
            else if (mode == AmplitudeMode.Inv_Proportional)
                condition_1 = [true, true, true, true, true, true, false, true, true];
            else if (mode == AmplitudeMode.Exponential)
                condition_1 = [false, false, true, true, true, true, false, false, true];
            else if (mode == AmplitudeMode.Linear_Polynomial)
                condition_1 = [false, false, true, true, true, true, true, false, true];
            else
                condition_1 = [false, false, true, true, true, true, false, false, true];

            if (cac == ControlAmplitudeContent.Default)
                condition_2 = [true, true, true, true, true, true, true, true, true];
            else if (cac == ControlAmplitudeContent.Free_Run_On)
                condition_2 = [true, true, true, true, true, true, true, true, true];
            else
                condition_2 = [true, true, true, true, true, true, true, true, true];

            for (int i =0; i < 9; i++)
            {
                _SetVisiblity(i, (condition_1[i] && condition_2[i]));

            }
        }
    }



    public enum ControlAmplitudeContent
    {
        Default,Free_Run_On,Free_Run_Off
    }
}

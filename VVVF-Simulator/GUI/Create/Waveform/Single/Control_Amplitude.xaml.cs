using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VvvfSimulator;
using static VvvfSimulator.VvvfCalculate;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData.YamlControlData.YamlControlDataAmplitudeControl;

namespace VvvfSimulator.GUI.Create.Waveform
{
    /// <summary>
    /// Control_Amplitude.xaml の相互作用ロジック
    /// </summary>
    public partial class Control_Amplitude : UserControl
    {
        private YamlControlDataAmplitude target;
        private ControlAmplitudeContent content;

        private bool no_update = true;
        private Visible_Class visible_Class;

        public class Visible_Class : ViewModelBase
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

        public Control_Amplitude(YamlControlDataAmplitude ycd, ControlAmplitudeContent cac)
        {
            target = ycd;
            content = cac;

            InitializeComponent();

            if (cac == ControlAmplitudeContent.Default)
                title.Content = "Default Amplitude Setting";
                
            else if (cac == ControlAmplitudeContent.Free_Run_On)
                title.Content = "Mascon On Free Run Amplitude Setting";

            else
                title.Content = "Mascon Off Free Run Amplitude Setting";

            visible_Class = new Visible_Class();
            DataContext = visible_Class;
            apply_view();

            no_update = false;
        }

        private void apply_view()
        {
            AmplitudeMode[] modes = (AmplitudeMode[])Enum.GetValues(typeof(AmplitudeMode));
            amplitude_mode_selector.ItemsSource = modes;
            amplitude_mode_selector.SelectedItem = target.Mode;

            start_freq_box.Text = target.Parameter.StartFrequency.ToString();
            start_amp_box.Text = target.Parameter.StartAmplitude.ToString();
            end_freq_box.Text = target.Parameter.EndFrequency.ToString();
            end_amp_box.Text = target.Parameter.EndAmplitude.ToString();
            cutoff_amp_box.Text = target.Parameter.CutOffAmplitude.ToString();
            max_amp_box.Text = target.Parameter.MaxAmplitude.ToString();
            polynomial_box.Text = target.Parameter.Polynomial.ToString();
            curve_rate_box.Text = target.Parameter.CurveChangeRate.ToString();
            disable_range_limit_check.IsChecked = target.Parameter.DisableRangeLimit;

            grid_hider(target.Mode, content);
        }

        private double parse_d(TextBox tb)
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

        private int parse_i(TextBox tb)
        {
            try
            {
                tb.Background = new BrushConverter().ConvertFrom("#FFFFFFFF") as Brush;
                return Int32.Parse(tb.Text);
            }
            catch
            {
                tb.Background = new BrushConverter().ConvertFrom("#FFfed0d0") as Brush;
                return -1;
            }
        }

        private void textbox_change(object sender, TextChangedEventArgs e)
        {
            if (no_update) return;

            TextBox tb = (TextBox)sender;
            Object? tag = tb.Tag;
            if (tag == null) return;

            if (tag.Equals("start_freq"))
                target.Parameter.StartFrequency = parse_d(tb);
            else if (tag.Equals("start_amp"))
                target.Parameter.StartAmplitude = parse_d(tb);
            else if (tag.Equals("end_freq"))
                target.Parameter.EndFrequency = parse_d(tb);
            else if (tag.Equals("end_amp"))
                target.Parameter.EndAmplitude = parse_d(tb);
            else if (tag.Equals("cutoff_amp"))
                target.Parameter.CutOffAmplitude = parse_d(tb);
            else if (tag.Equals("max_amp"))
                target.Parameter.MaxAmplitude = parse_d(tb);
            else if(tag.Equals("curve_rate"))
                target.Parameter.CurveChangeRate = parse_d(tb);
            else if (tag.Equals("polynomial"))
                target.Parameter.Polynomial = parse_d(tb);

            MainWindow.GetInstance()?.UpdateControlList();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (no_update) return;

            CheckBox cb = (CheckBox)sender;
            target.Parameter.DisableRangeLimit = cb.IsChecked != false;
            MainWindow.GetInstance()?.UpdateControlList();
        }

        private void amplitude_mode_selector_Selected(object sender, RoutedEventArgs e)
        {
            if (no_update) return;

            AmplitudeMode selected = (AmplitudeMode)amplitude_mode_selector.SelectedItem;
            target.Mode = selected;
            grid_hider(target.Mode, content);

            MainWindow.GetInstance()?.UpdateControlList();

            
        }

        private Grid get_Grid(int i)
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

        private void set_Visible_Bool(int i, bool b)
        {
            if (i == 0) visible_Class.start_freq_visible = b;
            else if (i == 1) visible_Class.start_amp_visible = b;
            else if (i == 2) visible_Class.end_freq_visible = b;
            else if (i == 3) visible_Class.end_amp_visible = b;
            else if (i == 4) visible_Class.cut_off_amp_visible = b;
            else if (i == 5) visible_Class.max_amp_visible = b;
            else if (i == 6) visible_Class.polynomial_visible = b;
            else if (i == 7) visible_Class.curve_rate_visible = b;
            else visible_Class.disable_range_visible = b;
        }

        private void grid_hider(AmplitudeMode mode , ControlAmplitudeContent cac)
        {
            Boolean[] condition_1, condition_2;

            if (mode == AmplitudeMode.Linear)
                condition_1 = new Boolean[9] { true, true, true, true, true, true, false, false, true };
            else if(mode == AmplitudeMode.Wide_3_Pulse)
                condition_1 = new Boolean[9] { true, true, true, true, true, true, false, false, true };
            else if(mode == AmplitudeMode.Inv_Proportional)
                condition_1 = new Boolean[9] { true, true, true, true, true, true, false, true, true };
            else if(mode == AmplitudeMode.Exponential)
                condition_1 = new Boolean[9] { false, false, true, true, true, true, false, false, true };
            else if(mode == AmplitudeMode.Linear_Polynomial)
                condition_1 = new Boolean[9] { false, false, true, true, true, true, true, false, true };
            else
                condition_1 = new Boolean[9] { false, false, true, true, true, true, false, false, true };

            if (cac == ControlAmplitudeContent.Default)
                condition_2 = new Boolean[9] { true, true, true, true, true, true, true, true, true };
            else if(cac == ControlAmplitudeContent.Free_Run_On)
                condition_2 = new Boolean[9] { true, true, true, true, true, true, true, true, true };
            else
                condition_2 = new Boolean[9] { true, true, true, true, true, true, true, true, true };

            for(int i =0; i < 9; i++)
            {
                set_Visible_Bool(i, (condition_1[i] && condition_2[i]));

            }
        }
    }



    public enum ControlAmplitudeContent
    {
        Default,Free_Run_On,Free_Run_Off
    }
}

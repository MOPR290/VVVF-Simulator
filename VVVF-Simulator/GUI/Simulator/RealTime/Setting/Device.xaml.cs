using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VvvfSimulator.GUI.Simulator.RealTime.Setting
{
    /// <summary>
    /// RealTime_Device_Setting.xaml の相互作用ロジック
    /// </summary>
    public partial class Device : Window
    {
        private readonly ViewModel Model = new();
        public class ViewModel : ViewModelBase
        {

            private Visibility _PortVisibility = Visibility.Visible;
            public Visibility PortVisibility { get { return _PortVisibility; } set { _PortVisibility = value; RaisePropertyChanged(nameof(PortVisibility)); } }

            private Visibility _AccelerateKeySettingVisibility = Visibility.Visible;
            public Visibility AccelerateKeySettingVisibility { get { return _AccelerateKeySettingVisibility; } set { _AccelerateKeySettingVisibility = value; RaisePropertyChanged(nameof(AccelerateKeySettingVisibility)); } }

            private Visibility _NeutralKeySettingVisibility = Visibility.Visible;
            public Visibility NeutralKeySettingVisibility { get { return _NeutralKeySettingVisibility; } set { _NeutralKeySettingVisibility = value; RaisePropertyChanged(nameof(NeutralKeySettingVisibility)); } }

            private Visibility _BrakeKeySettingVisibility = Visibility.Visible;
            public Visibility BrakeKeySettingVisibility { get { return _BrakeKeySettingVisibility; } set { _BrakeKeySettingVisibility = value; RaisePropertyChanged(nameof(BrakeKeySettingVisibility)); } }

            private Visibility _FrequencyRateVisibility = Visibility.Visible;
            public Visibility FrequencyRateVisibility { get { return _FrequencyRateVisibility; } set { _FrequencyRateVisibility = value; RaisePropertyChanged(nameof(FrequencyRateVisibility)); } }
        };
        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private readonly MasconWindow Main;
        public Device(MasconWindow Main)
        {
            this.Main = Main;
            Owner = Main;
            DataContext = Model;

            InitializeComponent();

            ModeSelector.ItemsSource = (DeviceMode[])Enum.GetValues(typeof(DeviceMode));
            ModeSelector.SelectedItem = Main.CurrentMode;
            SetCOMPorts();
            PortSelector.SelectedItem = Main.MasconComPort;

            SetTextBoxKey();
            SetDoubleInputTextBox();

            SetVisibility(Main.CurrentMode);
        }

        public void SetCOMPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            PortSelector.ItemsSource = ports;
        }

        private static Visibility GetVisibility(bool IsVisible)
        {
            return IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }
        public void SetVisibility(DeviceMode mode)
        {
            Model.PortVisibility = GetVisibility(mode == DeviceMode.PicoMascon);
            Model.AccelerateKeySettingVisibility = GetVisibility(mode == DeviceMode.KeyBoard);
            Model.NeutralKeySettingVisibility = GetVisibility(mode == DeviceMode.KeyBoard);
            Model.BrakeKeySettingVisibility = GetVisibility(mode == DeviceMode.KeyBoard);
        }

        private void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            Object tag = cb.Tag;

            if (tag.Equals("Mode"))
            {
                DeviceMode mode = (DeviceMode)cb.SelectedItem;
                Main.CurrentMode = mode;
                SetVisibility(mode);
            }else if (tag.Equals("Port"))
            {
                string port = (string)cb.SelectedItem;
                Main.MasconComPort = port;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Main.SetConfig();
        }

        private void OnWindowControlButtonClick(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            if (btn == null) return;
            string? tag = btn.Tag.ToString();
            if (tag == null) return;

            if (tag.Equals("Close"))
                Close();
            else if (tag.Equals("Maximize"))
            {
                if (WindowState.Equals(WindowState.Maximized))
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;
            }
            else if (tag.Equals("Minimize"))
                WindowState = WindowState.Minimized;
        }

        private void SetTextBoxKey()
        {
            AccelerateKey.Text = ((Key)Properties.Settings.Default.RealTimeMasconAccelerateKey).ToString();
            NeutralKey.Text = ((Key)Properties.Settings.Default.RealTimeMasconNeutralKey).ToString();
            BrakeKey.Text = ((Key)Properties.Settings.Default.RealTimeMasconBrakeKey).ToString();
        }
        private void TextBoxKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox? box = sender as TextBox;
            if (box == null) return;
            string? tag = box.Tag.ToString();
            if (tag == null) return;
            e.Handled = true;
            Key key = e.Key;
            box.Text = key.ToString();
            if (tag.Equals("Accelerate")) Properties.Settings.Default.RealTimeMasconAccelerateKey = (int)key;
            else if (tag.Equals("Neutral")) Properties.Settings.Default.RealTimeMasconNeutralKey = (int)key;
            else if (tag.Equals("Brake")) Properties.Settings.Default.RealTimeMasconBrakeKey = (int)key;
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

        private void SetDoubleInputTextBox()
        {
            FrequencyRateInput.Text = Properties.Settings.Default.RealTimeMasconFrequencyChangeRate.ToString();
        }
        private void DoubleInputTextBoxChanged(object sender, TextChangedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox == null) return;
            string? tag = textBox.Tag.ToString();
            if (tag == null) return;
            double value = ParseDouble(textBox);

            if (tag.Equals("FrequencyRate")) Properties.Settings.Default.RealTimeMasconFrequencyChangeRate = value;
        }
    }

    public enum DeviceMode
    {
        KeyBoard, PicoMascon
    }
}

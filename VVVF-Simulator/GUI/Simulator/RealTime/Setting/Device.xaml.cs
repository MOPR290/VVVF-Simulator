using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;

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

            private Visibility _Port_Visibility = Visibility.Visible;
            public Visibility Port_Visibility { get { return _Port_Visibility; } set { _Port_Visibility = value; RaisePropertyChanged(nameof(Port_Visibility)); } }
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
            DataContext = Model;

            InitializeComponent();

            Mode_Selector.ItemsSource = (DeviceMode[])Enum.GetValues(typeof(DeviceMode));
            Mode_Selector.SelectedItem = Main.current_mode;
            SetCOMPorts();
            Port_Selector.SelectedItem = Main.current_port;
            SetVisibility(Main.current_mode);
        }

        public void SetCOMPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            Port_Selector.ItemsSource = ports;
        }

        public void SetVisibility(DeviceMode mode)
        {
            if(mode == DeviceMode.KeyBoard)
            {
                Model.Port_Visibility = Visibility.Hidden;
            }
            else if(mode == DeviceMode.PicoMascon)
            {
                Model.Port_Visibility = Visibility.Visible;
            }
        }

        private void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            Object tag = cb.Tag;

            if (tag.Equals("Mode"))
            {
                DeviceMode mode = (DeviceMode)cb.SelectedItem;
                Main.current_mode = mode;
                SetVisibility(mode);
            }else if (tag.Equals("Port"))
            {
                string port = (string)cb.SelectedItem;
                Main.current_port = port;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Main.SetConfig();
        }
    }

    public enum DeviceMode
    {
        KeyBoard, PicoMascon
    }
}

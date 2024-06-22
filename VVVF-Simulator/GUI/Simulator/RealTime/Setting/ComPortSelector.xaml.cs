using System.IO.Ports;
using System.Windows;

namespace VvvfSimulator.GUI.Simulator.RealTime.Setting
{
    /// <summary>
    /// ComPortSelector.xaml の相互作用ロジック
    /// </summary>
    public partial class ComPortSelector : Window
    {
        public ComPortSelector()
        {
            InitializeComponent();
            SetComPorts();
        }

        public void SetComPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            PortSelector.ItemsSource = ports;
            if(ports.Length > 0 ) PortSelector.SelectedIndex = 0;
        }

        public string GetComPortName()
        {
            return (string)PortSelector.SelectedValue;
        }
    }
}

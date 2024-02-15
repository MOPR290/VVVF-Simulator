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
using VvvfSimulator;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData;

namespace VvvfSimulator.GUI.Create.Waveform.Async
{
    /// <summary>
    /// Control_Async_Const.xaml の相互作用ロジック
    /// </summary>
    public partial class Control_Async_Carrier_Const : UserControl
    {
        YamlControlData target;
        MainWindow MainWindow;

        bool no_update = true;
        public Control_Async_Carrier_Const(YamlControlData data, MainWindow mainWindow)
        {
            MainWindow = mainWindow;
            target = data;

            InitializeComponent();

            apply_data();

            no_update = false;
        }

        private void apply_data()
        {
            const_box.Text = target.async_data.carrier_wave_data.const_value.ToString();
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
        private void const_box_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (no_update) return;
            TextBox tb = (TextBox)sender;
            double d = parse_d(tb);

            target.async_data.carrier_wave_data.const_value = d;

            MainWindow.UpdateControlList();
        }
    }
}

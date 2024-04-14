using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static VvvfSimulator.VvvfStructs;

namespace VvvfSimulator.GUI.Create.Waveform.Basic
{
    /// <summary>
    /// DiscreteSetting.xaml の相互作用ロジック
    /// </summary>
    public partial class DiscreteSettingWindow : Window
    {
        private readonly PulseMode pulseMode;
        private readonly bool ignoreUpdate = true;
        public DiscreteSettingWindow(PulseMode pulseMode)
        {
            InitializeComponent();
            this.pulseMode = pulseMode;
            MyInitializeComponent();
            SetStatus();
            ignoreUpdate = false;
        }

        private void MyInitializeComponent()
        {
            ModeComboBox.ItemsSource = Enum.GetValues(typeof(PulseMode.DiscreteTimeConfiguration.DiscreteTimeMode));
        }

        private void SetStatus()
        {
            EnabledCheckBox.IsChecked = pulseMode.DiscreteTime.Enabled;
            StepsInput.Text = pulseMode.DiscreteTime.Steps.ToString();
            ModeComboBox.SelectedItem = pulseMode.DiscreteTime.Mode;
        }

        private void EnabledCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (ignoreUpdate) return;
            pulseMode.DiscreteTime.Enabled = EnabledCheckBox.IsChecked ?? false;
        }

        private static int ParseText2Int(TextBox tb)
        {
            try
            {
                tb.Background = new BrushConverter().ConvertFrom("#FFFFFFFF") as Brush;
                return int.Parse(tb.Text);
            }
            catch
            {
                tb.Background = new BrushConverter().ConvertFrom("#FFfed0d0") as Brush;
                return -1;
            }
        }

        private void StepsInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ignoreUpdate) return;
            int i = ParseText2Int(StepsInput);
            pulseMode.DiscreteTime.Steps = i;
        }

        private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ignoreUpdate) return;
            pulseMode.DiscreteTime.Mode = (PulseMode.DiscreteTimeConfiguration.DiscreteTimeMode)ModeComboBox.SelectedItem;
        }
    }
}

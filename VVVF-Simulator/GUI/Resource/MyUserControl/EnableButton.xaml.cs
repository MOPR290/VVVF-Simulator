using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace VvvfSimulator.GUI.Resource.MyUserControl
{
    /// <summary>
    /// EnableButton.xaml の相互作用ロジック
    /// </summary>
    public partial class EnableButton : UserControl
    {

        public event EventHandler? OnClicked;

        public EnableButton()
        {
            InitializeComponent();
            UpdateState();
        }

      
        private bool Enabled = false;
        private bool Activated = false;
        public void SetToggled(bool enabled)
        {
            Enabled = enabled;
        }

        public bool IsToggled()
        {
            return Enabled;
        }

        public void UpdateState()
        {
            Enabled = !Enabled;
            Status.Content = Enabled ? "Enabled" : "Disabled";

            SetBorderStatus();
        }

        public void SetBorderStatus()
        {
            LinearGradientBrush? EnabledBrush = this.FindResource("EnabledBrush") as LinearGradientBrush;
            LinearGradientBrush? EnabledPressedBrush = this.FindResource("EnabledPressedBrush") as LinearGradientBrush;
            LinearGradientBrush? DisabledBrush = this.FindResource("DisabledBrush") as LinearGradientBrush;
            LinearGradientBrush? DisabledPressedBrush = this.FindResource("DisabledPressedBrush") as LinearGradientBrush;

            if(EnabledBrush == null || EnabledPressedBrush == null || DisabledBrush == null || DisabledPressedBrush == null)
                return;

            if (Activated)
                Button.Background = Enabled ? EnabledPressedBrush : DisabledPressedBrush;
            else
                Button.Background = Enabled ? EnabledBrush : DisabledBrush;

        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Activated)
            {
                Activated = false;
                OnClicked?.Invoke(this, EventArgs.Empty);
                Dispatcher.Invoke(() =>
                {
                    UpdateState();
                });
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Activated = true;
            SetBorderStatus();
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Activated = false;
            SetBorderStatus();
        }
    }

    public class FontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = ((double)(value) / 2.0);
            if (d == 0) d = 1;
            return d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

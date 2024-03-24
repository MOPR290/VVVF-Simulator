using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VvvfSimulator.GUI.Util
{
    /// <summary>
    /// Double_Ask_Form.xaml の相互作用ロジック
    /// </summary>
    public partial class DoubleNumberInput : Window
    {
        public DoubleNumberInput(string title)
        {
            InitializeComponent();
            DescriptionBox.Content = title;
            NumberEnterBox.Text = "10.0";
        }

        public double EnteredValue = 0.0;
        private static double ParseDouble(TextBox tb)
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextBox tb = NumberEnterBox;
            double d = ParseDouble(tb);
            EnteredValue = d;
            Close();
        }

        private void NumberEnterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = NumberEnterBox;
            ParseDouble(tb);
        }
    }
}

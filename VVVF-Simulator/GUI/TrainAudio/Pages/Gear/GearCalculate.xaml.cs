using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Util;

namespace VvvfSimulator.GUI.TrainAudio.Pages.Gear
{
    /// <summary>
    /// TrainAudio_Gear_Get_Window.xaml の相互作用ロジック
    /// </summary>
    public partial class GearCalculate : Window
    {
        public int Gear1, Gear2;
        private bool no_update = true;

        public GearCalculate(int initial_gear1 , int initial_gear2)
        {
            Gear1 = initial_gear1;
            Gear2 = initial_gear2;

            InitializeComponent();

            Gear1_Box.Text = Gear1.ToString();
            Gear2_Box.Text = Gear2.ToString();

            no_update = false;
        }

        private void Gear1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (no_update) return;
            Gear1 = ParseTextBox.ParseInt(Gear1_Box);
        }

        private void Gear2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (no_update) return;
            Gear2 = ParseTextBox.ParseInt(Gear2_Box);
        }
    }
}

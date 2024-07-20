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
using System.Windows.Shapes;

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
            Gear1 = ParseInt(Gear1_Box);
        }

        private void Gear2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (no_update) return;
            Gear2 = ParseInt(Gear2_Box);
        }


        private static int ParseInt(TextBox tb)
        {
            try
            {
                VisualStateManager.GoToState(tb, "Success", false);
                return int.Parse(tb.Text);
            }
            catch
            {
                VisualStateManager.GoToState(tb, "Error", false);
                return 0;
            }
        }
    }
}

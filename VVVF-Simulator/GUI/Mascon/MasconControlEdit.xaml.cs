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
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze.YamlMasconData;

namespace VvvfSimulator.GUI.Mascon
{
    /// <summary>
    /// Generation_Mascon_Control_Edit_Page.xaml の相互作用ロジック
    /// </summary>
    public partial class MasconControlEdit : Page
    {
        private YamlMasconDataPoint data;
        private bool no_update = true;
        private MasconControlMain main_viewer;
        public MasconControlEdit(MasconControlMain main,YamlMasconDataPoint ympd)
        {
            InitializeComponent();

            data = ympd;
            main_viewer = main;

            apply_view();

            no_update = false;
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

        private static int ParseInt(TextBox tb, int minimum)
        {
            try
            {
                VisualStateManager.GoToState(tb, "Success", false);
                int i = int.Parse(tb.Text);
                if (i < minimum) throw new Exception();
                return i;
            }
            catch
            {
                VisualStateManager.GoToState(tb, "Error", false);
                return 0;
            }
        }

        private void apply_view()
        {
            order_box.Text = data.order.ToString();
            duration_box.Text = data.duration.ToString();
            rate_box.Text = data.rate.ToString();

            is_brake.IsChecked = data.brake;
            is_mascon_on.IsChecked = data.mascon_on;
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (no_update) return;
            TextBox tb = (TextBox)sender;
            Object tag = tb.Tag;

            if (tag.Equals("Duration"))
            {
                double d = ParseDouble(tb);
                data.duration = d;
                main_viewer.UpdateItemList();
            }
            else if (tag.Equals("Rate"))
            {
                double d = ParseDouble(tb);
                data.rate = d;
                main_viewer.UpdateItemList();
            }
            else if (tag.Equals("Order"))
            {
                int d = ParseInt(tb,0);
                data.order = d;
                main_viewer.UpdateItemList();
            }
        }

        private void Check_Changed(object sender, RoutedEventArgs e)
        {
            if (no_update) return;
            CheckBox cb = (CheckBox)sender;
            Object tag = cb.Tag;

            bool is_checked = cb.IsChecked == true;

            if (tag.Equals("Mascon"))
                data.mascon_on = is_checked;
            else if (tag.Equals("Brake"))
                data.brake = is_checked;
            main_viewer.UpdateItemList();
        }
    }
}

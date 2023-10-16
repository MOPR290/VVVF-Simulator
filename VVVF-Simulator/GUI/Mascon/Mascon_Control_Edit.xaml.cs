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
    public partial class Mascon_Control_Edit : Page
    {
        private YamlMasconDataPoint data;
        private bool no_update = true;
        private Mascon_Control_Main main_viewer;
        public Mascon_Control_Edit(Mascon_Control_Main main,YamlMasconDataPoint ympd)
        {
            InitializeComponent();

            data = ympd;
            main_viewer = main;

            apply_view();

            no_update = false;
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

        private int parse_i(TextBox tb, int minimum)
        {
            try
            {
                tb.Background = new BrushConverter().ConvertFrom("#FFFFFFFF") as Brush;
                int k = Int32.Parse(tb.Text);
                if (k < minimum) throw new Exception();
                return k;
            }
            catch
            {
                tb.Background = new BrushConverter().ConvertFrom("#FFfed0d0") as Brush;
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
                double d = parse_d(tb);
                data.duration = d;
                main_viewer.Refresh_ItemList();
            }
            else if (tag.Equals("Rate"))
            {
                double d = parse_d(tb);
                data.rate = d;
                main_viewer.Refresh_ItemList();
            }
            else if (tag.Equals("Order"))
            {
                int d = parse_i(tb,0);
                data.order = d;
                main_viewer.Refresh_ItemList();
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
            main_viewer.Refresh_ItemList();
        }
    }
}

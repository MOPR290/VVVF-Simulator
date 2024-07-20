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
using VvvfSimulator.Yaml.VvvfSound;

namespace VvvfSimulator.GUI.Create.Settings
{
    /// <summary>
    /// minimum_freq_setting.xaml の相互作用ロジック
    /// </summary>
    public partial class MinimumFrequency : Page
    {
        public MinimumFrequency()
        {
            InitializeComponent();

            accelerate_min_freq_box.Text = YamlVvvfManage.CurrentData.MinimumFrequency.Accelerating.ToString();
            braking_min_freq_box.Text = YamlVvvfManage.CurrentData.MinimumFrequency.Braking.ToString();
        }

        private double ParseDouble(TextBox tb)
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

        private void ValueChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox) sender;
            Object? tag = tb.Tag;
            if (tag == null) return;

            if (tag.Equals("Accelerate"))
                YamlVvvfManage.CurrentData.MinimumFrequency.Accelerating = ParseDouble(tb);
            else if (tag.Equals("Brake"))
                YamlVvvfManage.CurrentData.MinimumFrequency.Braking = ParseDouble(tb);
        }
    }
}

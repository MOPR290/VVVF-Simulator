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
    public partial class minimum_freq_setting : Page
    {
        public minimum_freq_setting()
        {
            InitializeComponent();

            accelerate_min_freq_box.Text = YamlVvvfManage.CurrentData.MinimumFrequency.Accelerating.ToString();
            braking_min_freq_box.Text = YamlVvvfManage.CurrentData.MinimumFrequency.Braking.ToString();
        }

        private void textbox_value_change(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox) sender;
            Object? tag = tb.Tag;
            if (tag == null) return;

            if (tag.Equals("Accelerate"))
            {
                try
                {
                    double d = Double.Parse(accelerate_min_freq_box.Text);
                    accelerate_min_freq_box.Background = new BrushConverter().ConvertFrom("#FFFFFFFF") as Brush;

                    YamlVvvfManage.CurrentData.MinimumFrequency.Accelerating = d;
                }
                catch
                {
                    accelerate_min_freq_box.Background = new BrushConverter().ConvertFrom("#FFfed0d0") as Brush;
                }
            }else if (tag.Equals("Brake"))
            {
                try
                {
                    double d = Double.Parse(braking_min_freq_box.Text);
                    accelerate_min_freq_box.Background = new BrushConverter().ConvertFrom("#FFFFFFFF") as Brush;

                    YamlVvvfManage.CurrentData.MinimumFrequency.Braking = d;
                }
                catch
                {
                    accelerate_min_freq_box.Background = new BrushConverter().ConvertFrom("#FFfed0d0") as Brush;
                }
            }

        }
    }
}

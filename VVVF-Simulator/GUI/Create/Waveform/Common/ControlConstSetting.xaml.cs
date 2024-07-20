using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VvvfSimulator.GUI.Create.Waveform.Common
{
    /// <summary>
    /// Control_Async_Random_Const.xaml の相互作用ロジック
    /// </summary>
    public partial class ControlConstSetting : UserControl
    {
        readonly bool IgnoreUpdate = true;
        readonly Type _type;
        readonly Object _object;

        public ControlConstSetting(Type type, Object value)
        {
            this._type = type;
            this._object = value;
            
            InitializeComponent();

            ValueBox.Text = type.GetProperty("Constant", BindingFlags.Instance | BindingFlags.Public)?.GetValue(value)?.ToString();
            IgnoreUpdate = false;
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

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IgnoreUpdate) return;
            double v = ParseDouble((TextBox)sender);
            _type.GetProperty("Constant")?.SetValue(_object, v);
        }
    }
}

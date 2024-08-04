using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Util;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData;

namespace VvvfSimulator.GUI.Create.Waveform.Common
{
    /// <summary>
    /// Control_Moving_Setting.xaml の相互作用ロジック
    /// </summary>
    public partial class ControlMovingSetting : UserControl
    {
        bool no_update = true;
        private ViewModel view_model = new();

        public class ViewModel : ViewModelBase
        {
            private Visibility _Exponential_Visibility = Visibility.Collapsed;
            public Visibility Exponential_Visibility { get { return _Exponential_Visibility; } set { _Exponential_Visibility = value; RaisePropertyChanged(nameof(Exponential_Visibility)); } }

            private Visibility _CurveRate_Visibility = Visibility.Collapsed;
            public Visibility CurveRate_Visibility { get { return _CurveRate_Visibility; } set { _CurveRate_Visibility = value; RaisePropertyChanged(nameof(CurveRate_Visibility)); } }

            public YamlMovingValue MovingValue { get; set; } = new YamlMovingValue();

        };
        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ControlMovingSetting(YamlMovingValue target)
        {

            view_model.MovingValue = target;
            DataContext = view_model;

            InitializeComponent();

            Move_Mode_Selector.ItemsSource = (YamlMovingValue.MovingValueType[])Enum.GetValues(typeof(YamlMovingValue.MovingValueType));
            set_Visibility();

            no_update = false;
        }

        private void text_changed(object sender, TextChangedEventArgs e)
        {
            if (no_update) return;
            TextBox tb = (TextBox)sender;
            Object? tag = tb.Tag;
            if (tag == null) return;

            if (tag.Equals("start"))
                view_model.MovingValue.Start = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("start_val"))
                view_model.MovingValue.StartValue = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("end"))
                view_model.MovingValue.End = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("end_val"))
                view_model.MovingValue.EndValue = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("degree"))
                view_model.MovingValue.Degree = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("curve_rate"))
                view_model.MovingValue.CurveRate = ParseTextBox.ParseDouble(tb);

        }

        private void Move_Mode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (no_update) return;

            YamlMovingValue.MovingValueType selected = (YamlMovingValue.MovingValueType)Move_Mode_Selector.SelectedItem;
            view_model.MovingValue.Type = selected;
            set_Visibility();


        }

        private void _set_Visibility(int x, Visibility b)
        {
            if (x == 0) view_model.Exponential_Visibility = b;
            else if (x == 1) view_model.CurveRate_Visibility = b;
        }

        private void set_Visibility()
        {
            YamlMovingValue.MovingValueType selected = (YamlMovingValue.MovingValueType)Move_Mode_Selector.SelectedItem;

            Visibility[] visible_list = new Visibility[2] { Visibility.Collapsed, Visibility.Collapsed };

            if (selected == YamlMovingValue.MovingValueType.Proportional)
                visible_list = new Visibility[2] { Visibility.Collapsed, Visibility.Collapsed };
            else if (selected == YamlMovingValue.MovingValueType.Pow2_Exponential)
                visible_list = new Visibility[2] { Visibility.Visible, Visibility.Collapsed };
            else if(selected == YamlMovingValue.MovingValueType.Inv_Proportional)
                visible_list = new Visibility[2] { Visibility.Collapsed, Visibility.Visible };
            else if (selected == YamlMovingValue.MovingValueType.Sine)
                visible_list = new Visibility[2] { Visibility.Collapsed, Visibility.Collapsed };

            for (int i = 0;i  < visible_list.Length; i++)
            {
                _set_Visibility(i, visible_list[i]);
            }
        }
    }
}

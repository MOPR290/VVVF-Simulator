using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VvvfSimulator.GUI.Util;
using Brush = System.Windows.Media.Brush;
using FontFamily = System.Drawing.FontFamily;

namespace VvvfSimulator.GUI.Simulator.RealTime.Setting
{
    /// <summary>
    /// RealTime_Settings.xaml の相互作用ロジック
    /// </summary>
    public partial class Basic : Window
    {
        private readonly bool IgnoreUpdate = true;
        private readonly RealTime_Basic_Setting_Type _SettingType;

        public enum RealTime_Basic_Setting_Type
        {
            VVVF, Train
        }

        public Basic(Window owner,RealTime_Basic_Setting_Type SettingType)
        {
            _SettingType = SettingType;
            Owner = owner;
            InitializeComponent();
            SetControl();
            IgnoreUpdate = false;
        }

        private class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void SetControl()
        {

            SelectorControlDesign.ItemsSource = (RealtimeWindows.ControlStatus.RealTimeControlStatStyle[])Enum.GetValues(typeof(RealtimeWindows.ControlStatus.RealTimeControlStatStyle));
            SelectorHexagonDesign.ItemsSource = (RealtimeWindows.Hexagon.RealTimeHexagonStyle[])Enum.GetValues(typeof(RealtimeWindows.Hexagon.RealTimeHexagonStyle));

            var Prop = Properties.Settings.Default;
            if (_SettingType.Equals(RealTime_Basic_Setting_Type.VVVF))
            {
                TextBuffSize.Text = Prop.RealTime_VVVF_BuffSize.ToString();
                BoxWaveForm.IsChecked = Prop.RealTime_VVVF_WaveForm_Show;
                BoxFFT.IsChecked = Prop.RealTime_VVVF_FFT_Show;
                BoxFS.IsChecked = Prop.RealTime_VVVF_FS_Show;
                BoxRealTimeEdit.IsChecked = Prop.RealTime_VVVF_EditAllow;

                BoxShowControl.IsChecked = Prop.RealTime_VVVF_Control_Show;
                BoxControlPrecise.IsChecked = Prop.RealTime_VVVF_Control_Precise;
                SelectorControlDesign.SelectedItem = (RealtimeWindows.ControlStatus.RealTimeControlStatStyle)Prop.RealTime_VVVF_Control_Style;

                BoxShowHexagon.IsChecked = Prop.RealTime_VVVF_Hexagon_Show;
                SelectorHexagonDesign.SelectedItem = (RealtimeWindows.Hexagon.RealTimeHexagonStyle)Prop.RealTime_VVVF_Hexagon_Style;
                BoxShowZeroVectorCicle.IsChecked = Prop.RealTime_VVVF_Hexagon_ZeroVector;

                SamplingFrequencyInput.Text = Prop.RealtimeVvvfSamplingFrequency.ToString();
                CalculateDivisionInput.Text = Prop.RealtimeVvvfCalculateDivision.ToString();

                WindowTitle.Content = "Realtime Vvvf Simulate Configuration";
            }
            else
            {
                TextBuffSize.Text = Prop.RealTime_Train_BuffSize.ToString();
                BoxWaveForm.IsChecked = Prop.RealTime_Train_WaveForm_Show;
                BoxFFT.IsChecked = Prop.RealTime_Train_FFT_Show;
                BoxFS.IsChecked = Prop.RealTime_Train_FS_Show;
                BoxRealTimeEdit.IsChecked = Prop.RealTime_Train_EditAllow;

                BoxShowControl.IsChecked = Prop.RealTime_Train_Control_Show;
                BoxControlPrecise.IsChecked = Prop.RealTime_Train_Control_Precise;
                SelectorControlDesign.SelectedItem = (RealtimeWindows.ControlStatus.RealTimeControlStatStyle)Prop.RealTime_Train_Control_Style;

                BoxShowHexagon.IsChecked = Prop.RealTime_Train_Hexagon_Show;
                SelectorHexagonDesign.SelectedItem = (RealtimeWindows.Hexagon.RealTimeHexagonStyle)Prop.RealTime_Train_Hexagon_Style;
                BoxShowZeroVectorCicle.IsChecked = Prop.RealTime_Train_Hexagon_ZeroVector;

                SamplingFrequencyInput.Text = Prop.RealtimeTrainSamplingFrequency.ToString();
                CalculateDivisionInput.Text = Prop.RealtimeTrainCalculateDivision.ToString();

                WindowTitle.Content = "Realtime Train Simulate Configuration";
            }

        }
        private void BoxChecked(object sender, RoutedEventArgs e)
        {
            if (IgnoreUpdate) return;
            CheckBox cb = (CheckBox)sender;
            Object tag = cb.Tag;

            Boolean is_checked = cb.IsChecked == true;

            if (tag.Equals("WaveForm"))
            {
                if (_SettingType.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_WaveForm_Show = is_checked;
                else if (_SettingType.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_WaveForm_Show = is_checked;
            }
                
            else if (tag.Equals("Edit"))
            {
                if (_SettingType.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_EditAllow = is_checked;
                else if (_SettingType.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_EditAllow = is_checked;
            }

            else if (tag.Equals("Control"))
            {
                if (_SettingType.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Control_Show = is_checked;
                else if (_SettingType.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_Control_Show = is_checked;
            }

            else if (tag.Equals("Hexagon"))
            {
                if (_SettingType.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Hexagon_Show = is_checked;
                else if (_SettingType.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_Hexagon_Show = is_checked;
            }
            else if (tag.Equals("HexagonZero"))
            {
                if (_SettingType.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Hexagon_ZeroVector = is_checked;
                else if (_SettingType.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_Hexagon_ZeroVector = is_checked;
            }
            else if (tag.Equals("FFT"))
            {
                if (_SettingType.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_FFT_Show = is_checked;
                else if (_SettingType.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_FFT_Show = is_checked;
            }
            else if (tag.Equals("FS"))
            {
                if (_SettingType.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_FS_Show = is_checked;
                else if (_SettingType.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_FS_Show = is_checked;
            }
            else if (tag.Equals("ControlPrecise"))
            {
                if (_SettingType.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Control_Precise = is_checked;
                else if (_SettingType.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_Control_Precise = is_checked;
            }

        }

        private void TextBoxChanged(object sender, TextChangedEventArgs e)
        {
            if (IgnoreUpdate) return;
            TextBox? textBox = sender as TextBox;
            if (textBox == null) return;
            string? tag = textBox.Tag.ToString();
            if (tag == null) return;

            switch (tag)
            {
                case "AudioBuffSize":
                    {
                        int i = ParseTextBox.ParseInt(TextBuffSize);
                        if (_SettingType.Equals(RealTime_Basic_Setting_Type.VVVF))
                            Properties.Settings.Default.RealTime_VVVF_BuffSize = i;
                        else if (_SettingType.Equals(RealTime_Basic_Setting_Type.Train))
                            Properties.Settings.Default.RealTime_Train_BuffSize = i;
                        break;
                    }
                case "SamplingFrequency":
                    {
                        int i = ParseTextBox.ParseInt(SamplingFrequencyInput);
                        if (_SettingType.Equals(RealTime_Basic_Setting_Type.VVVF))
                            Properties.Settings.Default.RealtimeVvvfSamplingFrequency = i;
                        else if (_SettingType.Equals(RealTime_Basic_Setting_Type.Train))
                            Properties.Settings.Default.RealtimeTrainSamplingFrequency = i;
                        break;
                    }
                case "CalculateDivision":
                    {
                        int i = ParseTextBox.ParseInt(CalculateDivisionInput);
                        if (_SettingType.Equals(RealTime_Basic_Setting_Type.VVVF))
                            Properties.Settings.Default.RealtimeVvvfCalculateDivision = i;
                        else if (_SettingType.Equals(RealTime_Basic_Setting_Type.Train))
                            Properties.Settings.Default.RealtimeTrainCalculateDivision = i;
                        break;
                    }
            }            
        }

        private void SelectorChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreUpdate) return;

            ComboBox cb = (ComboBox)sender;
            Object tag = cb.Tag;
            if (tag.Equals("ControlDesign"))
            {
                if (_SettingType.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Control_Style = (int)cb.SelectedItem;
                else if (_SettingType.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_Control_Style = (int)cb.SelectedItem;
            }else if (tag.Equals("Language"))
            {
                if (_SettingType.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Control_Language = (int)cb.SelectedItem;
                else if (_SettingType.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_Control_Language = (int)cb.SelectedItem;
            }

            else if (tag.Equals("HexagonDesign"))
            {
                if (_SettingType.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Hexagon_Style = (int)cb.SelectedItem;
                else if (_SettingType.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_Hexagon_Style = (int)cb.SelectedItem;               
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

            bool font_check = false;
            if (_SettingType.Equals(RealTime_Basic_Setting_Type.VVVF) && Properties.Settings.Default.RealTime_VVVF_Control_Show) font_check = true;
            if (_SettingType.Equals(RealTime_Basic_Setting_Type.Train) && Properties.Settings.Default.RealTime_Train_Control_Show) font_check = true;
            Properties.Settings.Default.Save();

            if (!font_check) return;
            var selected_style = Properties.Settings.Default.RealTime_VVVF_Control_Style;
            if(selected_style == (int)RealtimeWindows.ControlStatus.RealTimeControlStatStyle.Original)
            {
                try
                {
                    Font[] fonts = new Font[]{
                        new(new FontFamily("Fugaz One"), 75, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel), //topic
                    };
                }
                catch
                {
                    MessageBox.Show(
                        "Required font is not installed\r\n\r\n" +
                        "Fugaz One\r\n",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                        );

                }
            }
            else if(selected_style == (int)RealtimeWindows.ControlStatus.RealTimeControlStatStyle.Original_2)
            {
                try
                {
                    Font value_Font = new(new FontFamily("DSEG14 Modern"), 40, System.Drawing.FontStyle.Italic, GraphicsUnit.Pixel);
                    Font unit_font = new(new FontFamily("Fugaz One"), 25, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);
                }
                catch
                {
                    MessageBox.Show(
                        "Required font is not installed\r\n\r\n" +
                        "Fugaz One\r\n" +
                        "DSEG14 Modern Italic\r\n",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                        );
                }
            }
        }

        private void OnWindowControlButtonClick(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            if (btn == null) return;
            string? tag = btn.Tag.ToString();
            if (tag == null) return;

            if (tag.Equals("Close"))
                Close();
            else if (tag.Equals("Maximize"))
            {
                if (WindowState.Equals(WindowState.Maximized))
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;
            }
            else if (tag.Equals("Minimize"))
                WindowState = WindowState.Minimized;
        }

    }
}

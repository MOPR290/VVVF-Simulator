﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VvvfSimulator.Yaml.VVVFSound;
using static VvvfSimulator.Generation.Audio.GenerateRealTimeCommon;
using static VvvfSimulator.Generation.Video.FS.GenerateFourierSeries;
using Brush = System.Windows.Media.Brush;

namespace VvvfSimulator.GUI.Simulator.RealTime.Display
{
    /// <summary>
    /// RealTime_FFT_Window.xaml の相互作用ロジック
    /// </summary>
    public partial class RealTime_FS_Window : Window
    {
        private ViewModel BindingData = new();
        public class ViewModel : ViewModelBase
        {

            private BitmapFrame? _Image;
            public BitmapFrame? Image { get { return _Image; } set { _Image = value; RaisePropertyChanged(nameof(Image)); } }
        };
        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        RealTimeParameter _Parameter;
        public RealTime_FS_Window(RealTimeParameter Parameter)
        {
            _Parameter = Parameter;

            InitializeComponent();
            DataContext = BindingData;
        }

        public void RunTask()
        {
            Task.Run(() => {
                while (!_Parameter.quit)
                {
                    UpdateControl();
                }
                Dispatcher.Invoke(() =>
                {
                    Close();
                });
            });
        }

        private bool Resized = false;
        private int N = 100;
        private string StrCoefficients = "C = [0]";
        private void UpdateControl()
        {
            VvvfValues control = _Parameter.Control.Clone();
            YamlVvvfSoundData ysd = _Parameter.VvvfSoundData;

            control.SetSineTime(0);
            control.SetSawTime(0);

            double[] Coefficients = Get_Fourier_Coefficients(control, ysd, 10000, N);
            StrCoefficients = Get_Desmos_Fourier_Coefficients_Array(ref Coefficients);
            Bitmap image = Get_FS_Image(ref Coefficients);
            /*
            List<double> CoefficientsList = new(Coefficients);
            int remove_pos = 1;
            while (true)
            {
                CoefficientsList.RemoveAt(remove_pos);
                remove_pos++;
                if (remove_pos >= CoefficientsList.Count) break;
            }
            double[] ProcessedArray = CoefficientsList.ToArray();
            */


            if (!Resized)
            {
                Dispatcher.Invoke(() =>
                {
                    Height = image.Height / 2;
                    Width = image.Width / 2;
                });
                Resized = true;
            }

            using (Stream st = new MemoryStream())
            {
                image.Save(st, ImageFormat.Bmp);
                st.Seek(0, SeekOrigin.Begin);
                var data = BitmapFrame.Create(st, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                BindingData.Image = data;
            }

            image.Dispose();

            
        }

        private void Button_CopyCoefficients_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Clipboard.SetText(StrCoefficients);
            });
        }

        private void TextBox_N_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                TextBox_N.Background = new BrushConverter().ConvertFrom("#FFFFFFFF") as Brush;
                N = int.Parse(TextBox_N.Text);
            }
            catch
            {
                TextBox_N.Background = new BrushConverter().ConvertFrom("#FFfed0d0") as Brush;
            }
        }
    }
}

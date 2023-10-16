using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using VvvfSimulator.Yaml.VVVFSound;
using static VvvfSimulator.Generation.Audio.GenerateRealTimeCommon;
using static VvvfSimulator.VvvfStructs;

namespace VvvfSimulator.GUI.Simulator.RealTime.Display
{
    /// <summary>
    /// RealTime_WaveForm_Window.xaml の相互作用ロジック
    /// </summary>
    public partial class RealTime_WaveForm_Window : Window
    {
        private ViewModel BindingData = new();
        private class ViewModel : ViewModelBase
        {
            private BitmapFrame? _Image;
            public BitmapFrame? Image { get { return _Image; } set { _Image = value; RaisePropertyChanged(nameof(Image)); } }
        };
        private class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private RealTimeParameter _Parameter;
        public RealTime_WaveForm_Window(RealTimeParameter Parameter)
        {
            _Parameter = Parameter;
            DataContext = BindingData;
            InitializeComponent();
        }

        public void RunTask()
        {
            Task.Run(() => {
                while (!_Parameter.quit)
                {
                    UpdateControl();
                    System.Threading.Thread.Sleep(16);
                }
                Dispatcher.Invoke((Action)(() =>
                {
                    Close();
                }));
            });
        }

        private void UpdateControl()
        {
            YamlVvvfSoundData Sound = _Parameter.sound_data;
            VvvfValues Control = _Parameter.control_values.Clone();

            Control.set_Saw_Time(0);
            Control.set_Sine_Time(0);

            Control.set_Allowed_Random_Freq_Move(false);

            int image_width = 1200;
            int image_height = 450;
            int calculate_div = 3;
            int wave_height = 100;

            ControlStatus cv = new()
            {
                brake = Control.is_Braking(),
                mascon_on = !Control.is_Mascon_Off(),
                free_run = Control.is_Free_Running(),
                wave_stat = Control.get_Control_Frequency()
            };
            PwmCalculateValues calculated_Values = YamlVVVFWave.CalculateYaml(Control, cv, Sound);
            Bitmap image = Generation.Video.WaveForm.GenerateWaveFormUV.Get_WaveForm_Image(Control, calculated_Values, image_width, image_height, wave_height, 2, calculate_div,0);

            using Stream st = new MemoryStream();
            image.Save(st, ImageFormat.Bmp);
            st.Seek(0, SeekOrigin.Begin);
            var data = BitmapFrame.Create(st, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            BindingData.Image = data;
        }
    }
}

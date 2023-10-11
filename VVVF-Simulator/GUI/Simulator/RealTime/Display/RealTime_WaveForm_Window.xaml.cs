using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.Generation.Audio.Generate_RealTime_Common;
using static VVVF_Simulator.VvvfStructs;

namespace VVVF_Simulator.GUI.Simulator.RealTime.Display
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

        private RealTime_Parameter _Parameter;
        public RealTime_WaveForm_Window(RealTime_Parameter Parameter)
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
            Yaml_VVVF_Sound_Data Sound = _Parameter.sound_data;
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
            PwmCalculateValues calculated_Values = Yaml_VVVF_Wave.calculate_Yaml(Control, cv, Sound);
            Bitmap image = Generation.Video.WaveForm.Generate_WaveForm_UV.Get_WaveForm_Image(Control, calculated_Values, image_width, image_height, wave_height, 2, calculate_div,0);

            using Stream st = new MemoryStream();
            image.Save(st, ImageFormat.Bmp);
            st.Seek(0, SeekOrigin.Begin);
            var data = BitmapFrame.Create(st, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            BindingData.Image = data;
        }
    }
}

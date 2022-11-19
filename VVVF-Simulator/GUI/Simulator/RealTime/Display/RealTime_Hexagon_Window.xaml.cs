using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
using System.Windows.Shapes;
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.Generation.Audio.Generate_RealTime_Common;
using static VVVF_Simulator.VVVF_Calculate;

namespace VVVF_Simulator.GUI.Simulator.RealTime.Display
{
    /// <summary>
    /// RealTime_Hexagon_Window.xaml の相互作用ロジック
    /// </summary>
    public partial class RealTime_Hexagon_Window : Window
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        private RealTime_Hexagon_Style _Style;
        private RealTime_Parameter _Parameter;
        private bool _ZeroVectorCircle;
        private bool Resized = false;
        public RealTime_Hexagon_Window(RealTime_Parameter Parameter,RealTime_Hexagon_Style Style, bool ZeroVectorCircle)
        {
            InitializeComponent();
            _Parameter = Parameter;
            _Style = Style;
            _ZeroVectorCircle = ZeroVectorCircle;
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

        private void UpdateControl()
        {
            Bitmap image = new(100,100);

            VVVF_Values control = _Parameter.control_values.Clone();
            Yaml_VVVF_Sound_Data ysd = _Parameter.sound_data;

            control.set_Sine_Time(0);
            control.set_Saw_Time(0);

            if(_Style == RealTime_Hexagon_Style.Original)
            {
                int image_width = 1000;
                int image_height = 1000;
                int hex_div = 60000;
                control.set_Allowed_Random_Freq_Move(false);
                image = Generation.Video.Hexagon.Generate_Hexagon_Original.Get_Hexagon_Original_Image(
                    control,
                    ysd, 
                    image_width, 
                    image_height,
                    hex_div, 
                    2,
                    _ZeroVectorCircle,
                    false
                );
            }

            if (!Resized)
            {
                Dispatcher.Invoke(() =>
                {
                    Height = image.Height / 2;
                    Width = image.Width / 2;
                });
                Resized = true;
            }

            using Stream st = new MemoryStream();
            image.Save(st, ImageFormat.Bmp);
            st.Seek(0, SeekOrigin.Begin);
            var data = BitmapFrame.Create(st, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            BindingData.Image = data;
        }
    }

    public enum RealTime_Hexagon_Style
    {
        Original
    }
}

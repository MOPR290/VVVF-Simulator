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
using static VVVF_Simulator.Generation.Video.Control_Info.Generate_Control_Common;

namespace VVVF_Simulator.GUI.Simulator.RealTime.Display
{
    /// <summary>
    /// RealTime_ControlStat_Window.xaml の相互作用ロジック
    /// </summary>
    public partial class RealTime_ControlStat_Window : Window
    {
        private ViewModel BindingData = new();
        public class ViewModel : ViewModelBase
        {
            private BitmapFrame? _control_stat;
            public BitmapFrame? ControlImage { get { return _control_stat; } set { _control_stat = value; RaisePropertyChanged(nameof(ControlImage)); } }
        };
        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        RealTime_ControlStat_Style ControlStyle;
        RealTime_Parameter RealTimeParam;
        bool ControlPrecise = false;
        bool ControlSizeSet = false;
        public RealTime_ControlStat_Window(RealTime_Parameter r,RealTime_ControlStat_Style style, bool ControlPrecise)
        {
            RealTimeParam = r;
            this.ControlPrecise = ControlPrecise;

            DataContext = BindingData;
            InitializeComponent();
            ControlStyle = style;
        }

        public void StartTask()
        {
            Task.Run(() => {
                while (!RealTimeParam.quit)
                {
                    UpdateControlImage();

                }
                Dispatcher.Invoke((Action)(() =>
                {
                    Close();
                }));
            });
        }


        private void UpdateControlImage()
        {
            Bitmap image;

            if(ControlStyle == RealTime_ControlStat_Style.Original)
            {
                VVVF_Values control = RealTimeParam.control_values.Clone();
                image = Generation.Video.Control_Info.Generate_Control_Original.Get_Control_Original_Image(
                    control,
                    RealTimeParam.control_values.get_Sine_Freq() == 0
                );
            }
            else
            {
                VVVF_Values control = RealTimeParam.control_values.Clone();
                image = Generation.Video.Control_Info.Generate_Control_Original2.Get_Control_Original2_Image(
                    control,
                    RealTimeParam.sound_data,
                    ControlPrecise
                );
            }

            if (!ControlSizeSet)
            {
                ControlSizeSet = true;
                Dispatcher.Invoke(() =>
                {
                    double DisplayRatio = SystemParameters.WorkArea.Width / SystemParameters.WorkArea.Height;
                    double ControlRatio = image.Width / image.Height;

                    if (ControlRatio > 1)
                    {
                        Width = SystemParameters.WorkArea.Width * 2 / 4;
                        Height = Width / ControlRatio;
                    }
                    else
                    {
                        Height = SystemParameters.WorkArea.Height * 2 / 4;
                        Width = Height * ControlRatio;
                    }

                });
            }

            using Stream st = new MemoryStream();
            image.Save(st, ImageFormat.Bmp);
            st.Seek(0, SeekOrigin.Begin);
            var data = BitmapFrame.Create(st, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            BindingData.ControlImage = data;
        }
    }

    public enum RealTime_ControlStat_Style { 
        Original, Original_2
    }
}

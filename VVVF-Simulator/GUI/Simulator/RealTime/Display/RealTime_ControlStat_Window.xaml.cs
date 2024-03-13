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
using VvvfSimulator.Yaml.VVVFSound;
using static VvvfSimulator.Generation.Audio.GenerateRealTimeCommon;
using static VvvfSimulator.Generation.Video.ControlInfo.GenerateControlCommon;

namespace VvvfSimulator.GUI.Simulator.RealTime.Display
{
    /// <summary>
    /// RealTime_ControlStat_Window.xaml の相互作用ロジック
    /// </summary>
    public partial class RealTime_ControlStat_Window : Window
    {
        private ViewModel BindingData = new();
        private class ViewModel : ViewModelBase
        {
            private BitmapFrame? _ControlImage;
            public BitmapFrame? ControlImage { get { return _ControlImage; } set { _ControlImage = value; RaisePropertyChanged(nameof(ControlImage)); } }
        };
        private class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private RealTime_ControlStat_Style _Style;
        private RealTimeParameter _Paremter;
        private bool _ControlPrecise = false;
        private bool _ControlSizeSet = false;
        public RealTime_ControlStat_Window(RealTimeParameter Parameter,RealTime_ControlStat_Style Style, bool ControlPrecise)
        {
            _Paremter = Parameter;
            _ControlPrecise = ControlPrecise;
            _Style = Style;

            DataContext = BindingData;
            InitializeComponent();
        }

        public void RunTask()
        {
            Task.Run(() => {
                while (!_Paremter.quit)
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
            Bitmap image;

            if(_Style == RealTime_ControlStat_Style.Original)
            {
                VvvfValues control = _Paremter.Control.Clone();
                image = Generation.Video.ControlInfo.GenerateControlOriginal.GetImage(
                    control,
                    _Paremter.Control.GetSineFrequency() == 0
                );
            }
            else
            {
                VvvfValues control = _Paremter.Control.Clone();
                image = Generation.Video.ControlInfo.GenerateControlOriginal2.GetImage(
                    control,
                    _Paremter.VvvfSoundData,
                    _ControlPrecise
                );
            }

            if (!_ControlSizeSet)
            {
                _ControlSizeSet = true;
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

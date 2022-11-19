using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using VVVF_Simulator.Yaml.Mascon_Control;
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.VVVF_Structs;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Analyze;
using Yaml_Mascon_Data = VVVF_Simulator.Yaml.VVVF_Sound.Yaml_VVVF_Sound_Data.Yaml_Mascon_Data;

namespace VVVF_Simulator.Generation
{
    public class Generate_Common
    {
        /// <summary>
        /// この関数は、音声生成や、動画生成時の、マスコンの制御状態等を記述する関数です。
        /// この関数を呼ぶたびに、更新されます。
        /// 
        /// This is a function which will control a acceleration or brake when generating audio or video.
        /// It will be updated everytime this function colled.
        /// </summary>
        /// <returns></returns>
        public static bool Check_For_Freq_Change(VVVF_Values control,Yaml_Mascon_Data_Compiled ymdc, Yaml_Mascon_Data ymd, double add_time)
        {
            return Yaml_Mascon_Control.Check_For_Freq_Change(control, ymdc, ymd, add_time);
        }


        public static void Add_Empty_Frames(int image_width, int image_height,int frames, VideoWriter vr)
        {
            Bitmap image = new(image_width, image_height);
            Graphics g = Graphics.FromImage(image);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
            MemoryStream ms = new();
            image.Save(ms, ImageFormat.Png);
            byte[] img = ms.GetBuffer();
            Mat mat = OpenCvSharp.Mat.FromImageData(img);
            for (int i = 0; i < frames; i++) { vr.Write(mat); }
            g.Dispose();
            image.Dispose();
        }

        public static void Add_Image_Frames(Bitmap image, int frames, VideoWriter vr)
        {
            MemoryStream ms = new();
            image.Save(ms, ImageFormat.Png);
            byte[] img = ms.GetBuffer();
            Mat mat = OpenCvSharp.Mat.FromImageData(img);
            for (int i = 0; i < frames; i++) { vr.Write(mat); }
        }

        

        public class GenerationBasicParameter
        {
            public Yaml_Mascon_Data_Compiled masconData { get; set; }
            public Yaml_VVVF_Sound_Data vvvfData { get; set; }
            public ProgressData progressData { get; set; }

            public GenerationBasicParameter(Yaml_Mascon_Data_Compiled yaml_Mascon_Data_Compiled, Yaml_VVVF_Sound_Data yaml_VVVF_Sound_Data, ProgressData progressData)
            {
                this.masconData = yaml_Mascon_Data_Compiled;
                this.vvvfData = yaml_VVVF_Sound_Data;
                this.progressData = progressData;
            }
            public class ProgressData
            {
                public double Progress = 1;
                public double Total = 1;

                public double RelativeProgress
                {
                    get
                    {
                        return Progress / Total * 100;
                    }
                }

                public bool Cancel = false;
            }

        }

        /// <summary>
        ///  Gets 1 cycle of UVW wave form.
        ///  Division will be auto calculated.
        /// </summary>
        /// <param name="Control"></param>
        /// <param name="Sound"></param>
        /// <param name="Delta"> Normally, 120000 </param>
        /// <param name="Precise"> More precise when Freq < 1 </param>
        /// <returns></returns>
        public static Wave_Values[] Get_UWV_Cycle(VVVF_Values Control, Yaml_VVVF_Sound_Data Sound, double InitialPhase, int Delta, bool Precise)
        {
            double _divSeed = (Control.get_Sine_Freq() > 0.01 && Control.get_Sine_Freq() < 1) ? 1 / Control.get_Sine_Freq() : 1;
            _divSeed = Delta * (Precise ? _divSeed : 1);
            int divSeed = (int)Math.Round(_divSeed);

            Control.set_Sine_Time(0);
            Control.set_Saw_Time(0);

            Control_Values cv = new Control_Values
            {
                brake = Control.is_Braking(),
                mascon_on = !Control.is_Mascon_Off(),
                free_run = Control.is_Free_Running(),
                wave_stat = Control.get_Control_Frequency()
            };
            PWM_Calculate_Values calculated_Values = Yaml_VVVF_Wave.calculate_Yaml(Control, cv, Sound);

            Wave_Values[] PWM_Array = new Wave_Values[divSeed + 1];
            double dt = 1.0 / (divSeed * Control.get_Sine_Freq());
            for (int i = 0; i <= divSeed; i++)
            {
                Control.set_Sine_Time(i * dt);
                Control.set_Saw_Time(i * dt);
                Wave_Values value = VVVF_Calculate.calculate_values(Control, calculated_Values, InitialPhase);
                PWM_Array[i] = value;
            }

            return PWM_Array;
        }


    }
}

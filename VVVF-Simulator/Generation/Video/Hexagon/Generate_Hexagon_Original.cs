﻿using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using static VVVF_Simulator.VVVF_Calculate;
using static VVVF_Simulator.Generation.Generate_Common;
using static VVVF_Simulator.My_Math;
using VVVF_Simulator.Yaml.VVVF_Sound;
using System.Collections.Generic;
using Point = System.Drawing.Point;
using static VVVF_Simulator.VvvfStructs;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Analyze;
using static VVVF_Simulator.Generation.Generate_Common.GenerationBasicParameter;

namespace VVVF_Simulator.Generation.Video.Hexagon
{
    public class Generate_Hexagon_Original
    {


        public static Bitmap Get_Hexagon_Original_Image(
            VvvfValues Control,
            Yaml_VVVF_Sound_Data Sound,
            int Width,
            int Height, 
            int Delta,
            int Thickness,
            bool ZeroVectorCircle,
            bool PreciseDelta
        )
        {
            WaveValues[] PWM_Array = Generate_Basic.Get_UVW_Cycle(Control, Sound, 0, Delta, PreciseDelta);

            if (Control.get_Control_Frequency() == 0)
                return Get_Hexagon_Original_Image(ref PWM_Array, 0, Width, Height, Thickness, ZeroVectorCircle);

            Bitmap image = Get_Hexagon_Original_Image(ref PWM_Array, Control.get_Control_Frequency(), Width, Height, Thickness, ZeroVectorCircle);
            return image;
        }

        private class PointD
        {
            public double X { get; set; } = 0;
            public double Y { get; set; } = 0;

            public PointD(double X, double Y)
            {
                this.X = X;
                this.Y = Y; 
            }

            public bool IsZero()
            {
                return X == 0 && Y == 0;
            }

            public Point ToPoint()
            {
                return new Point((int)X, (int)Y);
            }

            public static PointD operator +(PointD a, PointD b)
            {
                return new PointD(a.X + b.X, a.Y + b.Y);
            }

            public static PointD operator *(double k,PointD a)
            {
                return new PointD(k*a.X, k*a.Y);
            }

            public static PointD Max(PointD a, PointD b)
            {
                return new PointD(a.X > b.X ? a.X : b.X, a.Y > b.Y ? a.Y : b.Y);
            }

            public static PointD Min(PointD a, PointD b)
            {
                return new PointD(a.X < b.X ? a.X : b.X, a.Y < b.Y ? a.Y : b.Y);
            }
            
            

        }

        public static Bitmap Get_Hexagon_Original_Image(
            ref WaveValues[] UVW,
            double ControlFrequency,
            int Width,
            int Height,
            int Thickness,
            bool ZeroVectorCircle
        )
        {
            if (ControlFrequency == 0)
            {
                Bitmap empty_image = new(Width, Height);
                Graphics empty_g = Graphics.FromImage(empty_image);
                empty_g.FillRectangle(new SolidBrush(Color.White), 0, 0, Width, Height);
                empty_g.Dispose();
                return empty_image;
            }

            bool drawn_circle = false;
            PointD CurrentPoint = new(0,0);
            PointD MaxValue = new(double.MinValue, double.MinValue);
            PointD MinValue = new(double.MaxValue, double.MaxValue);
            List<PointD> ZeroPoints = new();
            List<PointD> LinePoints = new() { CurrentPoint };

            WaveValues pre_wave_Values = new();

            for (int i = 0; i < UVW.Length; i++)
            {
                WaveValues value = UVW[i];
                PointD DeltaMove = new(
                    -0.5 * value.W - 0.5 * value.V + value.U,
                    -0.866025403784438646763 * value.W + 0.866025403784438646763 * value.V
                );

                if (!pre_wave_Values.Equals(value))
                {
                    LinePoints.Add(CurrentPoint);
                    pre_wave_Values = value.Clone();
                }

                if(DeltaMove.IsZero() && ZeroVectorCircle && !drawn_circle)
                {
                    drawn_circle = true;
                    ZeroPoints.Add(CurrentPoint);
                }else if (!DeltaMove.IsZero())
                {
                    drawn_circle = false;
                }
                CurrentPoint += DeltaMove;

                MaxValue = PointD.Max(CurrentPoint, MaxValue);
                MinValue = PointD.Min(CurrentPoint, MinValue);
            }

            Bitmap ImResult = new(Width, Height);
            Graphics GResult = Graphics.FromImage(ImResult);
            GResult.FillRectangle(new SolidBrush(Color.White), 0, 0, Width, Height);
            double k = 1200.0 / (UVW.Length - 1);

            PointD CorrectionAmount = new(
                Width / 2.0 - k*(MinValue.X + (MaxValue.X - MinValue.X) / 2.0),
                Height / 2.0 - k * (MinValue.Y + (MaxValue.Y - MinValue.Y) / 2.0)
            );

            for (int i = 0; i < LinePoints.Count - 1; i++)
            {
                Point start = (k * LinePoints[i] + CorrectionAmount).ToPoint();
                Point end = (k  * LinePoints[i+1] + CorrectionAmount).ToPoint();
                GResult.DrawLine(new Pen(Color.Black, Thickness), start, end);
            }

            for (int i = 0; i < ZeroPoints.Count; i++)
            {
                Point point = (k*ZeroPoints[i] + CorrectionAmount).ToPoint();

                double radius = 15 * ((ControlFrequency > 40) ? 1 : (ControlFrequency / 40.0));
                GResult.FillEllipse(new SolidBrush(Color.White),
                    (int)Math.Round(point.X - radius),
                    (int)Math.Round(point.Y - radius),
                    (int)Math.Round(radius * 2),
                    (int)Math.Round(radius * 2)
                );
                GResult.DrawEllipse(new Pen(Color.Black),
                    (int)Math.Round(point.X - radius),
                    (int)Math.Round(point.Y - radius),
                    (int)Math.Round(radius * 2),
                    (int)Math.Round(radius * 2)
                );
            }
            GResult.Dispose();
            return ImResult;
        }

        public static void Generate_Hexagon_Original_Video(GenerationBasicParameter generationBasicParameter, String fileName, bool circle)
        {
            Yaml_VVVF_Sound_Data vvvfData = generationBasicParameter.vvvfData;
            Yaml_Mascon_Data_Compiled masconData = generationBasicParameter.masconData;
            ProgressData progressData = generationBasicParameter.progressData;

            VvvfValues control = new();
            control.reset_control_variables();
            control.reset_all_variables();
            control.set_Allowed_Random_Freq_Move(false);

            Boolean draw_zero_vector_circle = circle;

            int fps = 60;


            int image_width = 1000;
            int image_height = 1000;

            int hex_div = 60000;

            VideoWriter vr = new(fileName, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(image_width, image_height));


            if (!vr.IsOpened())
            {
                return;
            }

            // Progress Initialize
            progressData.Total = masconData.GetEstimatedSteps(1.0 / fps) + 120;

            Boolean START_WAIT = true;
            if (START_WAIT)
            {
                Bitmap image = new(image_width, image_height);
                Graphics g = Graphics.FromImage(image);
                g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
                MemoryStream ms = new();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);

                for (int i = 0; i < 60; i++)
                {
                    // PROGRESS CHANGE
                    progressData.Progress++;

                    vr.Write(mat);
                }
                g.Dispose();
                image.Dispose();
            }

            Boolean loop = true;
            while (loop)
            {


                control.set_Sine_Time(0);
                control.set_Saw_Time(0);

                Bitmap final_image = Get_Hexagon_Original_Image(control, vvvfData, image_width, image_height, hex_div, 2, draw_zero_vector_circle, true);


                MemoryStream ms = new();
                final_image.Save(ms, ImageFormat.Png);
                final_image.Dispose();
                byte[] img = ms.GetBuffer();
                Mat mat = Mat.FromImageData(img);

                Cv2.ImShow("Wave Form View", mat);
                Cv2.WaitKey(1);

                vr.Write(mat);

                loop = Check_For_Freq_Change(control, masconData, vvvfData.mascon_data, 1.0 / fps);
                if (progressData.Cancel) loop = false;

                // PROGRESS CHANGE
                progressData.Progress++;
            }

            Boolean END_WAIT = true;
            if (END_WAIT)
            {
                Bitmap image = new(image_width, image_height);
                Graphics g = Graphics.FromImage(image);
                g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
                MemoryStream ms = new();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);

                for (int i = 0; i < 60; i++)
                {
                    // PROGRESS CHANGE
                    progressData.Progress++;

                    vr.Write(mat);
                }
                g.Dispose();
                image.Dispose();
            }

            vr.Release();
            vr.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"> Path for png file </param>
        /// <param name="sound_data">SOUND DATA</param>
        /// <param name="circle">Setting whether see zero vector circle or not</param>
        /// <param name="d">Frequency you want to see</param>
        public static void Generate_Hexagon_Original_Image(String fileName, Yaml_VVVF_Sound_Data sound_data, Boolean circle, double d)
        {
            VvvfValues control = new();

            control.reset_control_variables();
            control.reset_all_variables();
            control.set_Allowed_Random_Freq_Move(false);

            Boolean draw_zero_vector_circle = circle;

            control.set_Sine_Angle_Freq(d * M_2PI);
            control.set_Control_Frequency(d);

            int image_width = 1000;
            int image_height = 1000;

            int hex_div = 60000;
            Bitmap final_image = Get_Hexagon_Original_Image(control, sound_data, image_width, image_height, hex_div, 2, draw_zero_vector_circle, true);

            MemoryStream ms = new();
            final_image.Save(ms, ImageFormat.Png);
            byte[] img = ms.GetBuffer();
            Mat mat = Mat.FromImageData(img);

            final_image.Save(fileName, ImageFormat.Png);


            Cv2.ImShow("Hexagon", mat);
            Cv2.WaitKey();
            final_image.Dispose();
        }
    }
}

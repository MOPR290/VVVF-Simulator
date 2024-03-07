using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VvvfSimulator.Yaml.VVVFSound;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Generation.GenerateCommon.GenerationBasicParameter;
using static VvvfSimulator.MainWindow;
using static VvvfSimulator.VvvfCalculate;
using static VvvfSimulator.VvvfStructs;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;

namespace VvvfSimulator.Generation.Video.WaveForm
{
    public class GenerateWaveFormUV
    {

        /// <summary>
        /// Do clone before call this!
        /// </summary>
        /// <param name="Control"></param>
        /// <param name="PWM_Data"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="WaveHeight"></param>
        /// <param name="Delta"></param>
        /// <returns></returns>
        public static Bitmap Get_WaveForm_Image(
            VvvfValues Control,
            PwmCalculateValues PWM_Data,
            int Width, 
            int Height, 
            int WaveHeight,
            int WaveWidth,
            int Delta,
            int Spacing
        )
        {
            int Count = (Width - Spacing * 2) * Delta;
            WaveValues[] values = new WaveValues[Count];
            for (int i = 0; i < Count; i++)
            {
                WaveValues value = CalculatePhases(Control, PWM_Data, Math.PI / 6.0);
                values[i] = value;
                Control.AddSawTime(2 / (60.0 * Count));
                Control.AddSineTime(2 / (60.0 * Count));
            }
            return Get_WaveForm_Image(ref values, Width, Height, WaveHeight, WaveWidth, Spacing);
        }

        public static Bitmap Get_WaveForm_Image(
            ref WaveValues[] UVW,
            int Width,
            int Height,
            int WaveHeight,
            int WaveWidth,
            int Spacing
        )
        {
            Bitmap image = new(Width, Height);
            Graphics g = Graphics.FromImage(image);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, Width, Height);

            List<int> points_x = new();
            List<int> points_y = new();

            points_x.Add(Spacing);
            points_y.Add((int)(Height / 2.0));

            int pre_pwm = 0;

            for (int i = 0; i < UVW.Length; i++)
            {
                int pwm = UVW[i].U - UVW[i].V;
                if (pre_pwm != pwm)
                {
                    points_x.Add((int)(i / (double)UVW.Length * (Width - Spacing * 2)) + Spacing);
                    points_y.Add((int)(-pre_pwm * WaveHeight + Height / 2.0));

                    points_x.Add((int)(i / (double)UVW.Length * (Width - Spacing * 2)) + Spacing);
                    points_y.Add((int)(-pwm * WaveHeight + Height / 2.0));
                    pre_pwm = pwm;
                }
            }

            points_x.Add(Width - Spacing);
            points_y.Add((int)(-pre_pwm * WaveHeight + Height / 2.0));

            for (int i = 0; i < points_x.Count - 1; i++)
            {
                int x_1 = points_x[i];
                int x_2 = points_x[i + 1];
                int y_1 = points_y[i];
                int y_2 = points_y[i + 1];
                g.DrawLine(new Pen(Color.Black, WaveWidth), x_1, y_1, x_2, y_2);
            }

            g.Dispose();
            return image;
        }

        public static void Generate_UV_1(GenerationBasicParameter generationBasicParameter, String fileName)
        {
            YamlVvvfSoundData vvvfData = generationBasicParameter.vvvfData;
            YamlMasconDataCompiled masconData = generationBasicParameter.masconData;
            ProgressData progressData = generationBasicParameter.progressData;

            VvvfValues control = new();
            control.ResetControlValues();
            control.ResetMathematicValues();

            control.SetRandomFrequencyMoveAllowed(false);

            int fps = 60;

            int image_width = 2880;
            int image_height = 540;

            int wave_height = 100;
            int calculate_div = 30;

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

                control.SetSineTime(0);
                control.SetSawTime(0);

                ControlStatus cv = new()
                {
                    brake = control.IsBraking(),
                    mascon_on = !control.IsMasconOff(),
                    free_run = control.IsFreeRun(),
                    wave_stat = control.GetControlFrequency()
                };
                PwmCalculateValues calculated_Values = YamlVVVFWave.CalculateYaml(control, cv, vvvfData);

                Bitmap image = Get_WaveForm_Image(control, calculated_Values, image_width, image_height, wave_height, 2, calculate_div, 100);


                MemoryStream ms = new();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);
                vr.Write(mat);
                mat.Dispose();
                ms.Dispose();

                MemoryStream resized_ms = new();
                Bitmap resized = new(image, image_width / 2, image_height / 2);
                resized.Save(resized_ms, ImageFormat.Png);
                byte[] resized_img = resized_ms.GetBuffer();
                Mat resized_mat = OpenCvSharp.Mat.FromImageData(resized_img);
                Cv2.ImShow("Wave Form", resized_mat);
                Cv2.WaitKey(1);
                resized_mat.Dispose();
                resized_ms.Dispose();

                image.Dispose();

                loop = CheckForFreqChange(control, masconData, vvvfData.MasconData, 1.0 / fps);
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

        public static void Generate_UV_2(GenerationBasicParameter generationBasicParameter, String fileName)
        {
            YamlVvvfSoundData vvvfData = generationBasicParameter.vvvfData;
            YamlMasconDataCompiled masconData = generationBasicParameter.masconData;
            ProgressData progressData = generationBasicParameter.progressData;

            VvvfValues control = new();
            control.ResetControlValues();
            control.ResetMathematicValues();
            control.SetRandomFrequencyMoveAllowed(false);

            int fps = 60;

            int image_width = 2000;
            int image_height = 500;

            int wave_height = 100;
            int calculate_div = 10;

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
                g.DrawLine(new Pen(Color.Gray), 0, image_height / 2, image_width, image_height / 2);
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

                control.SetSineTime(0);
                control.SetSawTime(0);

                ControlStatus cv = new()
                {
                    brake = control.IsBraking(),
                    mascon_on = !control.IsMasconOff(),
                    free_run = control.IsFreeRun(),
                    wave_stat = control.GetControlFrequency()
                };
                PwmCalculateValues calculated_Values = YamlVVVFWave.CalculateYaml(control, cv, vvvfData);

                Bitmap image = Get_WaveForm_Image(control, calculated_Values, image_width, image_height, wave_height, 1, calculate_div, 0);

                MemoryStream ms = new();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);

                Cv2.ImShow("Wave Form View", mat);
                Cv2.WaitKey(1);

                vr.Write(mat);

                image.Dispose();
                loop = CheckForFreqChange(control, masconData, vvvfData.MasconData, 1.0 / fps);
                if(progressData.Cancel) loop = false;

                // PROGRESS CHANGE
                progressData.Progress++;

            }

            Boolean END_WAIT = true;
            if (END_WAIT)
            {
                Bitmap image = new(image_width, image_height);
                Graphics g = Graphics.FromImage(image);
                g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
                g.DrawLine(new Pen(Color.Gray), 0, image_height / 2, image_width, image_height / 2);
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
    }
}

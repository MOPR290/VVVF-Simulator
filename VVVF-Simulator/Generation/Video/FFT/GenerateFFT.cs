using NAudio.Dsp;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using static VvvfSimulator.VvvfStructs;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;

namespace VvvfSimulator.Generation.Video.FFT
{
    public class GenerateFFT
    {
        private static readonly int pow = 15;
        private static Complex[] FFTNAudio(ref WaveValues[] WaveForm)
        {
            Complex[] fft = new Complex[WaveForm.Length];
            for (int i = 0; i < WaveForm.Length; i++)
            {
                fft[i].X = (float)((WaveForm[i].U - WaveForm[i].V) * FastFourierTransform.HammingWindow(i, WaveForm.Length));;
                fft[i].Y = 0;
            }
            FastFourierTransform.FFT(true, pow, fft);
            Array.Resize(ref fft, fft.Length/2);
            return fft;
        }
        private static (float R, float θ) ConvertComplex(Complex C)
        {
            float R = C.X * C.X + C.Y * C.Y;
            float θ = (float)Math.Atan2(C.Y, C.X);
            return (R, θ);
        }
        public static Bitmap Get_FFT_Image(VvvfValues control, YamlVvvfSoundData sound)
        {
            control.SetRandomFrequencyMoveAllowed(false);
            WaveValues[] PWM_Array = GenerateBasic.GetUVWSec(control, sound, MyMath.M_PI_6, (int)Math.Pow(2,pow) - 1, false);
            Complex[] FFT = FFTNAudio(ref PWM_Array);

            Bitmap image = new(1000, 1000);
            Graphics g = Graphics.FromImage(image);

            g.FillRectangle(new SolidBrush(Color.White),0,0, 1000, 1000);

            for (int i = 0; i < 1000 - 1; i++)
            {
                var (Ri, θi) = ConvertComplex(FFT[(int)(MyMath.M_PI * i)]);
                var (Rii, θii) = ConvertComplex(FFT[(int)(MyMath.M_PI * (i + 1))]);
                PointF start = new(i, 1000 - Ri * 2000);
                PointF end = new(i + 1, 1000 - Rii * 2000);
                g.DrawLine(new Pen(Color.Black, 2), start, end);
            }

            g.Dispose();

            return image;

        }

        public static void Generate_FFT_Video(GenerationBasicParameter generationBasicParameter, String fileName)
        {
            YamlVvvfSoundData vvvfData = generationBasicParameter.vvvfData;
            YamlMasconDataCompiled masconData = generationBasicParameter.masconData;
            ProgressData progressData = generationBasicParameter.progressData;

            VvvfValues control = new();
            control.ResetControlValues();
            control.ResetMathematicValues();

            control.SetRandomFrequencyMoveAllowed(false);

            int fps = 60;

            int image_width = 1000;
            int image_height = 1000;

            VideoWriter vr = new(fileName, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(image_width, image_height));


            if (!vr.IsOpened())
            {
                return;
            }

            // Progress Initialize
            progressData.Total = masconData.GetEstimatedSteps(1.0 / fps) + 120;

            Boolean START_WAIT = true;
            if (START_WAIT)
                GenerateCommon.AddEmptyFrames(image_width, image_height, 60, vr);

            // PROGRESS CHANGE
            progressData.Progress+=60;

            Boolean loop = true;
            while (loop)
            {

                control.SetSineTime(0);
                control.SetSawTime(0);

                Bitmap image = Get_FFT_Image(control, vvvfData);


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

                loop = GenerateCommon.CheckForFreqChange(control, masconData, vvvfData.MasconData, 1.0 / fps);
                if (progressData.Cancel) loop = false;

                // PROGRESS CHANGE
                progressData.Progress++;
            }

            Boolean END_WAIT = true;
            if (END_WAIT)
                GenerateCommon.AddEmptyFrames(image_width, image_height, 60, vr);

            // PROGRESS CHANGE
            progressData.Progress += 60;

            vr.Release();
            vr.Dispose();
        }

        public static void Generate_FFT_Image(String fileName, YamlVvvfSoundData sound_data, double d)
        {
            VvvfValues control = new();

            control.ResetControlValues();
            control.ResetMathematicValues();
            control.SetRandomFrequencyMoveAllowed(false);

            control.SetSineAngleFrequency(d * MyMath.M_2PI);
            control.SetControlFrequency(d);

            Bitmap image = Get_FFT_Image(control, sound_data);

            MemoryStream ms = new();
            image.Save(ms, ImageFormat.Png);
            byte[] img = ms.GetBuffer();
            Mat mat = Mat.FromImageData(img);

            image.Save(fileName, ImageFormat.Png);


            Cv2.ImShow("FFT", mat);
            Cv2.WaitKey();
            image.Dispose();
        }
    }
}

using System;
using System.Drawing;
using System.Threading.Tasks;
using VvvfSimulator.Yaml.VVVFSound;
using static VvvfSimulator.VvvfStructs;
using static VvvfSimulator.Generation.Video.ControlInfo.GenerateControlCommon;
using static VvvfSimulator.Generation.Video.Hexagon.GenerateHexagonOriginal;
using static VvvfSimulator.Generation.Video.WaveForm.GenerateWaveFormUV;
using static VvvfSimulator.VvvfCalculate;
using OpenCvSharp;
using System.IO;
using System.Drawing.Imaging;
using static VvvfSimulator.Generation.GenerateCommon;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;
using static VvvfSimulator.VvvfStructs.PulseMode;
using static VvvfSimulator.Generation.GenerateCommon.GenerationBasicParameter;

namespace VvvfSimulator.Generation.Video.ControlInfo
{
    public class GenerateControlOriginal2
    {

        private class StringContent {
            public Font font;
            public String content;
            public Point compensation;

            public StringContent(Font f,String l,Point p)
            {
                font = f;
                content = l;
                compensation = p;
            }
        }
        private static void Draw_Topic_Value(Graphics g,Point start, Size size, StringContent topic, StringContent value, StringContent unit, int topic_width)
        {
            SizeF topic_size = g.MeasureString(topic.content, topic.font);
            SizeF val_size = g.MeasureString(value.content, value.font) ;
            SizeF unit_size = g.MeasureString(unit.content, unit.font);

            filled_corner_curved_rectangle(g, new SolidBrush(Color.FromArgb(0x33, 0x35, 0x33)), start, new Point(start.X + size.Width, start.Y + size.Height), 10);
            line_corner_curved_rectangle(g, new Pen(Color.FromArgb(0xED, 0xF2, 0xF4), 5), start, new Point(start.X + size.Width, start.Y + size.Height), 10);

            g.DrawLine(new Pen(Color.White, 2), new Point(start.X + topic_width, start.Y + 10), new Point(start.X + topic_width, start.Y + size.Height - 10));

            float topic_x = start.X + topic_width / 2 - topic_size.Width / 2 + topic.compensation.X;
            float topic_y = start.Y + (size.Height - topic_size.Height) / 2 + topic.compensation.Y;
            g.DrawString(topic.content, topic.font, new SolidBrush(Color.White), new PointF(topic_x , topic_y));


            float value_x = start.X + topic_width + (size.Width - topic_width - val_size.Width - unit_size.Width) / 2 + value.compensation.X;
            float value_y = start.Y + (size.Height - val_size.Height) / 2 + value.compensation.Y;
            g.DrawString(value.content, value.font, new SolidBrush(Color.White), new PointF(value_x , value_y));

            float unit_x = value_x + val_size.Width + unit.compensation.X;
            float unit_y = value_y + val_size.Height - unit_size.Height + unit.compensation.Y;
            g.DrawString(unit.content, unit.font, new SolidBrush(Color.White), new PointF(unit_x, unit_y));
        }

        private static String get_Pulse_Name(VvvfValues control)
        {
            PulseMode mode_p = control.GetVideoPulseMode();
            PulseModeNames mode = mode_p.PulseName;
            //Not in sync
            if (mode == PulseModeNames.Async)
            {
                CarrierFreq carrier_freq_data = control.GetVideoCarrierFrequency();
                String default_s = String.Format(carrier_freq_data.base_freq.ToString("F2"));
                return default_s;
            }

            //Abs
            if (mode == PulseModeNames.P_Wide_3)
                return "W 3";

            if (mode.ToString().StartsWith("CHM"))
            {
                String mode_name = mode.ToString();
                bool contain_wide = mode_name.Contains("Wide");
                mode_name = mode_name.Replace("_Wide", "");

                String[] mode_name_type = mode_name.Split("_");

                String final_mode_name = (contain_wide ? "W " : "") + mode_name_type[1];

                return "CHM " + final_mode_name;
            }
            if (mode.ToString().StartsWith("SHE"))
            {
                String mode_name = mode.ToString();
                bool contain_wide = mode_name.Contains("Wide");
                mode_name = mode_name.Replace("_Wide", "");

                String[] mode_name_type = mode_name.Split("_");

                String final_mode_name = (contain_wide) ? "W " : "" + mode_name_type[1];

                return "SHE " + final_mode_name;
            }
            else
            {
                String[] mode_name_type = mode.ToString().Split("_");
                return mode_name_type[1];
            }
        }

        public static Bitmap Get_Control_Original2_Image(VvvfValues Control, YamlVvvfSoundData Sound, bool Precise)
        {
            int image_width = 1920;
            int image_height = 500;
            double voltage = 0;

            Bitmap image = new(image_width, image_height);
            Bitmap hexagon = new(400, 400), wave_form = new(1520, 400);
            Graphics g = Graphics.FromImage(image);

            VvvfValues CycleControl = Control;
            WaveValues[] CycleUVW = Array.Empty<WaveValues>();

            // CALCULATE ONE CYCLE OF PWM
            Task CycleCalcTask = Task.Run(() =>
            {
                CycleControl.SetRandomFrequencyMoveAllowed(false);
                CycleControl.SetSineTime(0);
                CycleControl.SetSawTime(0);
                CycleUVW = GenerateBasic.Get_UVW_Cycle(CycleControl, Sound, MyMath.M_PI_6, (Precise ? 120000 : 6000), Precise);
            });
            Task WaveFormTask = Task.Run(() => {
                VvvfValues WaveFormControl = Control.Clone();
                WaveFormControl.SetRandomFrequencyMoveAllowed(false);
                WaveFormControl.SetSineTime(0);
                WaveFormControl.SetSawTime(0);
                PwmCalculateValues calculated_Values = YamlVVVFWave.CalculateYaml(WaveFormControl, new ControlStatus()
                {
                    brake = WaveFormControl.IsBraking(),
                    mascon_on = !WaveFormControl.IsMasconOff(),
                    free_run = WaveFormControl.IsFreeRun(),
                    wave_stat = WaveFormControl.GetControlFrequency()
                }, Sound);
                wave_form = Get_WaveForm_Image(WaveFormControl, calculated_Values, 1520, 400, 80, 2, Precise ? 60 : 1 ,50);
            });
            CycleCalcTask.Wait();
            Task HexagonRenderTask = Task.Run(() =>
            {
                hexagon = new(Get_Hexagon_Original_Image(ref CycleUVW, CycleControl.GetControlFrequency(), 1000, 1000, 2, true), 400, 400);
            });
            Task VoltageCalcTask = Task.Run(() =>
            {
                voltage = Math.Abs(FS.GenerateFourierSeries.Get_Fourier_Fast(ref CycleUVW, 1, 0)) * 100;
            });
            HexagonRenderTask.Wait();
            VoltageCalcTask.Wait();
            WaveFormTask.Wait();
            g.DrawImage(wave_form, 400, 100);
            g.DrawImage(hexagon, 0, 100);

            Color stat_color, back_color, stat_str_color;
            String stat_str;
            bool stopping = CycleControl.GetSineAngleFrequency() == 0;
            if (stopping)
            {
                stat_str_color = Color.White;
                stat_color = Color.FromArgb(0x33, 0x35, 0x33);

                back_color = Color.FromArgb(0x81, 0x7F, 0x82);
                stat_str = "Stop";
            }
            else if (CycleControl.IsFreeRun())
            {
                stat_str_color = Color.White;
                stat_color = Color.FromArgb(0x36, 0xd0, 0x36);

                back_color = Color.FromArgb(0x81, 0x7F, 0x82);
                stat_str = "Cruise";
            }
            else if (!CycleControl.IsBraking())
            {
                stat_str_color = Color.White;
                stat_color = Color.FromArgb(0x43,0x92, 0xF1);

                back_color = Color.FromArgb(0x81, 0x7F, 0x82);
                stat_str = "Accelerate";
            }
            else
            {
                stat_str_color = Color.White;
                stat_color = Color.FromArgb(0xe6, 0x7e, 0x00);

                back_color = Color.FromArgb(0x81, 0x7F, 0x82);
                stat_str = "Brake";
            }
            g.FillRectangle(new SolidBrush(stat_color), 0, 0, 400, 100);

            g.FillRectangle(new SolidBrush(back_color), 400, 0, 1520, 100);

            Font stat_Font = new(new FontFamily("Fugaz One"), 40, FontStyle.Regular, GraphicsUnit.Pixel);
            Font topic_Font = new(new FontFamily("Fugaz One"), 40, FontStyle.Regular, GraphicsUnit.Pixel);
            Font value_Font = new(new FontFamily("DSEG14 Modern"), 40, FontStyle.Italic, GraphicsUnit.Pixel);
            Font unit_font = new(new FontFamily("Fugaz One"), 25, FontStyle.Regular, GraphicsUnit.Pixel);

            SizeF stat_str_Size = g.MeasureString(stat_str, stat_Font);
            g.DrawString(stat_str, stat_Font, new SolidBrush(stat_str_color), new PointF((400 - stat_str_Size.Width) / 2 , (100 - stat_str_Size.Height) / 2 + 5));

            // pulse state


            bool is_async = CycleControl.GetVideoPulseMode().PulseName.Equals(PulseModeNames.Async);
            Draw_Topic_Value(
                g, new Point(420, 10), new Size(480, 80),
                new StringContent(topic_Font, "Pulse", new Point(0, 5)),
                new StringContent(value_Font, stopping ? "-----" : get_Pulse_Name(CycleControl), new Point(0, 5)),
                new StringContent(unit_font, is_async ? "Hz" : "", new Point(0, 9)),
                200);

            Draw_Topic_Value(
                g, new Point(920, 10), new Size(480, 80),
                new StringContent(topic_Font, "Voltage", new Point(0, 5)),
                new StringContent(value_Font, stopping ? "---.-" : String.Format("{0:F1}", voltage), new Point(0, 5)),
                new StringContent(unit_font, "%", new Point(0, 9)),
                200);

            Draw_Topic_Value(
                g, new Point(1420, 10), new Size(480, 80),
                new StringContent(topic_Font, "Freq", new Point(0, 5)),
                new StringContent(value_Font, stopping ? "---.-" : String.Format("{0:F1}", CycleControl.GetVideoSineFrequency()), new Point(0, 5)),
                new StringContent(unit_font, "Hz", new Point(0, 9)),
                200);

            g.Dispose();
            return image;
        }

        public static void Generate_Control_Original2_Video(
            GenerationBasicParameter generationBasicParameter,
            String output_path
        )
        {
            YamlVvvfSoundData vvvfData = generationBasicParameter.vvvfData;
            YamlMasconDataCompiled masconData = generationBasicParameter.masconData;
            ProgressData progressData = generationBasicParameter.progressData;

            VvvfValues control = new();
            control.ResetControlValues();
            control.ResetMathematicValues();
            control.SetRandomFrequencyMoveAllowed(false);

            int fps = 60;

            int image_width = 1920;
            int image_height = 500;
            VideoWriter vr = new(output_path, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(image_width, image_height));

            if (!vr.IsOpened())
            {
                return;
            }

            // PROGRESS INITIALIZE
            progressData.Total = masconData.GetEstimatedSteps(1.0 / fps) + 120;

            bool START_FRAMES = true;
            if (START_FRAMES)
            {

                ControlStatus cv = new()
                {
                    brake = true,
                    mascon_on = true,
                    free_run = false,
                    wave_stat = 0
                };
                PwmCalculateValues calculated_Values = YamlVVVFWave.CalculateYaml(control, cv, vvvfData);
                _ = CalculatePhases(control, calculated_Values, 0);
                Bitmap final_image = Get_Control_Original2_Image(control, vvvfData, true);

                AddImageFrames(final_image, 60, vr);

                final_image.Dispose();
            }

            //PROGRESS ADD
            progressData.Progress += 60;

            while (true)
            {
                Bitmap final_image = Get_Control_Original2_Image(control,  vvvfData, true);

                MemoryStream ms = new();
                final_image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);
                vr.Write(mat);
                ms.Dispose();
                mat.Dispose();

                MemoryStream resized_ms = new();
                Bitmap resized = new(final_image, image_width / 2, image_height / 2);
                resized.Save(resized_ms, ImageFormat.Png);
                byte[] resized_img = resized_ms.GetBuffer();
                Mat resized_mat = OpenCvSharp.Mat.FromImageData(resized_img);
                Cv2.ImShow("Generation", resized_mat);
                Cv2.WaitKey(1);
                resized_mat.Dispose();
                resized_ms.Dispose();

                final_image.Dispose();

                if (!CheckForFreqChange(control, masconData, vvvfData.MasconData, 1.0 / fps)) break;
                if (progressData.Cancel) break;
                progressData.Progress++;
            }

            bool END_FRAMES = true;
            if (END_FRAMES)
            {

                ControlStatus cv = new()
                {
                    brake = true,
                    mascon_on = true,
                    free_run = false,
                    wave_stat = 0
                };
                PwmCalculateValues calculated_Values = YamlVVVFWave.CalculateYaml(control, cv, vvvfData);
                _ = CalculatePhases(control, calculated_Values, 0);
                Bitmap final_image = Get_Control_Original2_Image(control, vvvfData, true);
                AddImageFrames(final_image, 60, vr);

                final_image.Dispose();
            }

            //PROGRESS ADD
            progressData.Progress += 60;

            vr.Release();
            vr.Dispose();
        }
    }
}

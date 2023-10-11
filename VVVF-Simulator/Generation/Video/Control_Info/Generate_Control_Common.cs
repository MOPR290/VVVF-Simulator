using System;
using System.Drawing;
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.VVVF_Calculate;
using static VVVF_Simulator.VvvfStructs;

namespace VVVF_Simulator.Generation.Video.Control_Info
{
    public class Generate_Control_Common
    {
   
        /// <summary>
        /// Do clone about control!
        /// </summary>
        /// <param name="Sound"></param>
        /// <param name="Control"></param>
        /// <returns></returns>
        public static double Get_Voltage_Rate(VvvfValues Control, Yaml_VVVF_Sound_Data Sound, bool Precise)
        {
            WaveValues[] PWM_Array = Generate_Basic.Get_UVW_Cycle(Control, Sound, My_Math.M_PI_6, 120000, Precise);
            double result = FS.Generate_FS.Get_Fourier_Fast(ref PWM_Array, 1, 0);
            result = Math.Abs(result);
            return result;
        }
        public static void filled_corner_curved_rectangle(Graphics g, Brush br, Point start, Point end, int round_radius)
        {
            int width = end.X - start.X;
            int height = end.Y - start.Y;

            g.FillRectangle(br, start.X + round_radius, start.Y, width - 2 * round_radius, height);
            g.FillRectangle(br, start.X, start.Y + round_radius, round_radius, height - 2 * round_radius);
            g.FillRectangle(br, end.X - round_radius, start.Y + round_radius, round_radius, height - 2 * round_radius);

            g.FillEllipse(br, start.X, start.Y, round_radius * 2, round_radius * 2);
            g.FillEllipse(br, start.X, start.Y + height - round_radius * 2, round_radius * 2, round_radius * 2);
            g.FillEllipse(br, start.X + width - round_radius * 2, start.Y, round_radius * 2, round_radius * 2);
            g.FillEllipse(br, start.X + width - round_radius * 2, start.Y + height - round_radius * 2, round_radius * 2, round_radius * 2);

        }

        public static Point center_text_with_filled_corner_curved_rectangle(Graphics g, String str, Brush str_br, Font fnt, Brush br,
                                                                 Point start, Point end, int round_radius, Point str_compen)
        {
            SizeF strSize = g.MeasureString(str, fnt);

            int width = end.X - start.X;
            int height = end.Y - start.Y;

            filled_corner_curved_rectangle(g, br, start, end, round_radius);

            Point str_pos = new((int)Math.Round(start.X + width / 2 - strSize.Width / 2 + str_compen.X), (int)Math.Round(start.Y + height / 2 - strSize.Height / 2 + str_compen.Y));

            g.DrawString(str, fnt, str_br, str_pos);

            return str_pos;
        }

        public static void line_corner_curved_rectangle(Graphics g, Pen pen, Point start, Point end, int round_radius)
        {
            int width = (int)(end.X - start.X);
            int height = (int)(end.Y - start.Y);

            g.DrawLine(pen, start.X + round_radius, start.Y, end.X - round_radius + 1, start.Y);
            g.DrawLine(pen, start.X + round_radius, end.Y, end.X - round_radius + 1, end.Y);

            g.DrawLine(pen, start.X, start.Y + round_radius, start.X, end.Y - round_radius + 1);
            g.DrawLine(pen, end.X, start.Y + round_radius, end.X, end.Y - round_radius + 1);

            g.DrawArc(pen, start.X, start.Y, round_radius * 2, round_radius * 2, -90, -90);
            g.DrawArc(pen, start.X, start.Y + height - round_radius * 2, round_radius * 2, round_radius * 2, -180, -90);
            g.DrawArc(pen, start.X + width - round_radius * 2, start.Y, round_radius * 2, round_radius * 2, 0, -90);
            g.DrawArc(pen, start.X + width - round_radius * 2, start.Y + height - round_radius * 2, round_radius * 2, round_radius * 2, 0, 90);

        }

        public static void title_str_with_line_corner_curved_rectangle(Graphics g, String str, Brush str_br, Font fnt, Pen pen,
                                                                 Point start, Point end, int round_radius, Point str_compen)
        {

            SizeF strSize = g.MeasureString(str, fnt);

            int width = end.X - start.X;
            int height = end.Y - start.Y;

            g.DrawLine(pen, start.X + round_radius, start.Y, start.X + width / 2 - strSize.Width / 2 - 10, start.Y);
            g.DrawLine(pen, end.X - round_radius + 1, start.Y, start.X + width / 2 + strSize.Width / 2 + 10, start.Y);

            g.DrawString(str, fnt, str_br, start.X + width / 2 - strSize.Width / 2 + str_compen.X, start.Y - fnt.Height / 2 + str_compen.Y);

            g.DrawLine(pen, start.X + round_radius, end.Y, end.X - round_radius + 1, end.Y);

            g.DrawLine(pen, start.X, start.Y + round_radius, start.X, end.Y - round_radius + 1);
            g.DrawLine(pen, end.X, start.Y + round_radius, end.X, end.Y - round_radius + 1);

            g.DrawArc(pen, start.X, start.Y, round_radius * 2, round_radius * 2, -90, -90);
            g.DrawArc(pen, start.X, start.Y + height - round_radius * 2, round_radius * 2, round_radius * 2, -180, -90);
            g.DrawArc(pen, start.X + width - round_radius * 2, start.Y, round_radius * 2, round_radius * 2, 0, -90);
            g.DrawArc(pen, start.X + width - round_radius * 2, start.Y + height - round_radius * 2, round_radius * 2, round_radius * 2, 0, 90);

        }

        public static Point center_text_with_line_corner_curved_rectangle(Graphics g, String str, Brush str_br, Font fnt, Pen pen,
                                                                 Point start, Point end, int round_radius, Point str_compen)
        {

            SizeF strSize = g.MeasureString(str, fnt);
            int width = end.X - start.X;
            int height = end.Y - start.Y;
            line_corner_curved_rectangle(g, pen, start, end, round_radius);

            Point string_pos = new((int)Math.Round(start.X + width / 2 - strSize.Width / 2 + str_compen.X), (int)Math.Round(start.Y + height / 2 - strSize.Height / 2 + str_compen.Y));
            g.DrawString(str, fnt, str_br, string_pos);

            return string_pos;


        }

    }
}

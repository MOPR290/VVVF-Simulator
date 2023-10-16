using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using VvvfSimulator.Yaml.VVVFSound;
using static VvvfSimulator.Generation.Audio.TrainSound.GenerateTrainAudio;
using static VvvfSimulator.Generation.Motor.GenerateMotorCore;
using static VvvfSimulator.Yaml.TrainAudio_Setting.YamlTrainSoundAnalyze;

namespace VvvfSimulator.Generation.Audio
{
    public class GenerateRealTimeCommon
    {

        // ---------- COMMON ---------------
        public class RealTimeParameter
        {
            public double change_amount { get; set; } = 0;
            public Boolean braking { get; set; } = false;
            public Boolean quit { get; set; } = false;
            public Boolean reselect { get; set; } = false;
            public Boolean free_run { get; set; } = false;

            public VvvfValues control_values { get; set; } = new();
            public YamlVvvfSoundData sound_data { get; set; } = new();

            public MotorData Motor = new();
            public YamlTrainSoundData Train_Sound_Data = YamlTrainSoundDataManage.current_data.Clone();
        }

        public static int RealTime_CheckForFreq(VvvfValues control, RealTimeParameter param, int step)
        {
            control.set_Braking(param.braking);
            control.set_Mascon_Off(param.free_run);

            double change_amo = param.change_amount;

            double sin_new_angle_freq = control.get_Sine_Angle_Freq();
            sin_new_angle_freq += change_amo;
            if (sin_new_angle_freq < 0) sin_new_angle_freq = 0;

            if (!control.is_Free_Running())
            {
                if (control.is_Allowed_Sine_Time_Change())
                {
                    if (sin_new_angle_freq != 0)
                    {
                        double amp = control.get_Sine_Angle_Freq() / sin_new_angle_freq;
                        control.multi_Sine_Time(amp);
                    }
                    else
                        control.set_Sine_Time(0);
                }

                control.set_Control_Frequency(control.get_Sine_Freq());
                control.set_Sine_Angle_Freq(sin_new_angle_freq);
            }


            if (param.quit) return 0;
            else if (param.reselect) return 1;

            if (!control.is_Mascon_Off()) // mascon on
            {
                if (!control.is_Free_Running())
                    control.set_Control_Frequency(control.get_Sine_Freq());
                else
                {
                    double freq_change = control.get_Free_Freq_Change() * 1.0 / 192000 * step;
                    double final_freq = control.get_Control_Frequency() + freq_change;

                    if (control.get_Sine_Freq() <= final_freq)
                    {
                        control.set_Control_Frequency(control.get_Sine_Freq());
                        control.set_Free_Running(false);
                    }
                    else
                    {
                        control.set_Control_Frequency(final_freq);
                        control.set_Free_Running(true);
                    }
                }
            }
            else
            {
                double freq_change = control.get_Free_Freq_Change() * 1.0 / 192000 * step;
                double final_freq = control.get_Control_Frequency() - freq_change;
                control.set_Control_Frequency(final_freq > 0 ? final_freq : 0);
                control.set_Free_Running(true);
            }

            return -1;
        }
    }

}

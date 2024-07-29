using System;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Generation.Motor.GenerateMotorCore;
using static VvvfSimulator.Yaml.TrainAudioSetting.YamlTrainSoundAnalyze;

namespace VvvfSimulator.Generation.Audio
{
    public class GenerateRealTimeCommon
    {

        // ---------- COMMON ---------------
        public class RealTimeParameter
        {
            public double FrequencyChangeRate { get; set; } = 0;
            public Boolean IsBraking { get; set; } = false;
            public Boolean Quit { get; set; } = false;
            public Boolean IsFreeRunning { get; set; } = false;

            public VvvfValues Control { get; set; } = new();
            public YamlVvvfSoundData VvvfSoundData { get; set; } = new();
            public MotorData Motor { get; set; } = new();
            public YamlTrainSoundData TrainSoundData { get; set; } = YamlTrainSoundDataManage.CurrentData.Clone();
            public String AudioDeviceId { get; set; } = new NAudio.CoreAudioApi.MMDeviceEnumerator().GetDefaultAudioEndpoint(NAudio.CoreAudioApi.DataFlow.Render, NAudio.CoreAudioApi.Role.Multimedia).ID;
        }

        public static int RealTimeFrequencyControl(VvvfValues control, RealTimeParameter param, double dt)
        {
            control.SetBraking(param.IsBraking);
            control.SetMasconOff(param.IsFreeRunning);

            double change_amo = param.FrequencyChangeRate;

            double sin_new_angle_freq = control.GetSineAngleFrequency();
            sin_new_angle_freq += change_amo * dt;
            if (sin_new_angle_freq < 0) sin_new_angle_freq = 0;

            if (!control.IsFreeRun())
            {
                if (control.IsSineTimeChangeAllowed())
                {
                    if (sin_new_angle_freq != 0)
                    {
                        double amp = control.GetSineAngleFrequency() / sin_new_angle_freq;
                        control.MultiplySineTime(amp);
                    }
                    else
                        control.SetSineTime(0);
                }

                control.SetControlFrequency(control.GetSineFrequency());
                control.SetSineAngleFrequency(sin_new_angle_freq);
            }


            if (param.Quit) return 0;

            if (!control.IsMasconOff()) // mascon on
            {
                if (!control.IsFreeRun())
                    control.SetControlFrequency(control.GetSineFrequency());
                else
                {
                    double freq_change = control.GetFreeFrequencyChange() * dt;
                    double final_freq = control.GetControlFrequency() + freq_change;

                    if (control.GetSineFrequency() <= final_freq)
                    {
                        control.SetControlFrequency(control.GetSineFrequency());
                        control.SetFreeRun(false);
                    }
                    else
                    {
                        control.SetControlFrequency(final_freq);
                        control.SetFreeRun(true);
                    }
                }
            }
            else
            {
                double freq_change = control.GetFreeFrequencyChange() * dt;
                double final_freq = control.GetControlFrequency() - freq_change;
                control.SetControlFrequency(final_freq > 0 ? final_freq : 0);
                control.SetFreeRun(true);
            }

            return -1;
        }
    }

}

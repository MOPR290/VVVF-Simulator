using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VvvfSimulator.Yaml.VvvfSound
{
    public class YamlVvvfUtil
    {

        private static void Auto_Voltage_Task(YamlVvvfSoundData ysd_x,bool brake,int i,int x, double max_freq)
        {
            List<YamlVvvfSoundData.YamlControlData> ysd = brake ? ysd_x.BrakingPattern : ysd_x.AcceleratePattern;
            var parameter = ysd[i].Amplitude.DefaultAmplitude.Parameter;

            if (ysd[i].Amplitude.DefaultAmplitude.Mode != VvvfCalculate.AmplitudeMode.Linear) return;

            parameter.DisableRangeLimit = false;

            double target_freq;
            if (ysd.Count == i + x)
                target_freq = ysd[i].ControlFrequencyFrom + 0.1 * x;
            else
                target_freq = ysd[i + x].ControlFrequencyFrom - 0.001 * x;
            if (x == 0) parameter.StartFrequency = target_freq;
            else parameter.EndFrequency = target_freq;

            parameter.MaxAmplitude = -1;
            parameter.CutOffAmplitude = -1;

            VvvfValues control = new();
            control.ResetMathematicValues();
            control.ResetControlValues();
            control.SetSineAngleFrequency(target_freq * Math.PI * 2);
            control.SetControlFrequency(target_freq);
            control.SetMasconOff(false);
            control.SetFreeRun(false);
            control.SetBraking(brake);
            control.SetRandomFrequencyMoveAllowed(false);

            double desire_voltage = 1.0 / max_freq * target_freq * 100;

            int same_val_continue = 0; double pre_diff = 0;

            int amplitude_seed = -1;
            while(true)
            {
                amplitude_seed++;

                double try_amplitude = amplitude_seed / 1000.0;
                if (x == 0) parameter.StartAmplitude = try_amplitude;
                else parameter.EndAmplitude = try_amplitude;

                if (desire_voltage == 0) return;

                double voltage = Generation.Video.ControlInfo.GenerateControlCommon.GetVoltageRate(control, ysd_x, true) * 100;
                if (voltage < -1) continue;
                double diff = desire_voltage - voltage;

                if (Math.Abs(diff - pre_diff) > 2)
                    same_val_continue = 0;
                else
                    same_val_continue++;
                if (same_val_continue > 100) break;
                pre_diff = diff;

                Debug.WriteLine(String.Format("{0:F02},{1:F02},{2:F02},{3},{4},{5},{6}", diff, desire_voltage, voltage, amplitude_seed, i, x, target_freq));
                if (diff < 0.01)
                {
                    amplitude_seed -= 10;
                    if (amplitude_seed < 0)
                        amplitude_seed = 1000;
                }
                else if (diff < 0.2)
                {
                    if (x == 0)
                    {
                        for (int l = 0; l < 2; l++)
                        {
                            var free_run_param = l == 0 ? ysd[i].Amplitude.FreeRunAmplitude.Off.Parameter : ysd[i].Amplitude.FreeRunAmplitude.On.Parameter;
                            free_run_param.DisableRangeLimit = false;
                            free_run_param.StartAmplitude = try_amplitude;
                            free_run_param.StartFrequency = target_freq;
                        }
                    }
                    break;
                }
                else
                {
                    if (voltage < 1) continue;
                    if (Math.Abs(diff) < 1) continue;
                    amplitude_seed = (int)Math.Round(desire_voltage / voltage * amplitude_seed);
                }

            }
        }
        public static bool Auto_Voltage(YamlVvvfSoundData data)
        {
            var accel = data.AcceleratePattern;
            bool accel_has_settings = accel.Count > 1;
            bool allow_accel = true;
            for (int i = 0; i < accel.Count; i++)
            {
                bool flg_2 = accel[i].ControlFrequencyFrom >= 0;
                if (flg_2) continue;
                allow_accel = false;
                break;
            }

            var brake = data.BrakingPattern;
            bool brake_has_settings = brake.Count > 1;
            bool allow_brake = true;
            for (int i = 0; i < brake.Count; i++)
            {
                bool flg_2 = brake[i].ControlFrequencyFrom >= 0;
                if (flg_2) continue;
                allow_brake = false;
                break;
            }

            if (!accel_has_settings || !allow_accel || !brake_has_settings || !allow_brake) return false;

            accel.Sort((a, b) => Math.Sign(a.ControlFrequencyFrom - b.ControlFrequencyFrom));
            brake.Sort((a, b) => Math.Sign(a.ControlFrequencyFrom - b.ControlFrequencyFrom));

            double accel_end_freq = accel[accel.Count - 1].ControlFrequencyFrom;
            double brake_end_freq = brake[brake.Count - 1].ControlFrequencyFrom;

            List<Task> tasks = new();

            for (int i = 0; i < accel.Count; i++)
            {
                for (int x = 0; x < 2; x++)
                {
                    int _i = i;
                    int _x = x;
                    Task t = Task.Run(() => Auto_Voltage_Task(data, false, _i,_x, accel_end_freq));
                    tasks.Add(t);
                }
            }

            for (int i = 0; i < brake.Count; i++)
            {
                for (int x = 0; x < 2; x++)
                {
                    int _i = i;
                    int _x = x;
                    Task t = Task.Run(() => Auto_Voltage_Task(data, true, _i, _x, brake_end_freq));
                    tasks.Add(t);
                }
            }



            Task.WaitAll(tasks.ToArray());

            accel.Sort((a, b) => Math.Sign(b.ControlFrequencyFrom - a.ControlFrequencyFrom));
            brake.Sort((a, b) => Math.Sign(b.ControlFrequencyFrom - a.ControlFrequencyFrom));

            return true;
        }

        public static bool Set_All_FreeRunAmp_Zero(YamlVvvfSoundData data)
        {
            var accel = data.AcceleratePattern;
            for(int i = 0; i < accel.Count; i++)
            {
                accel[i].Amplitude.FreeRunAmplitude.Off.Parameter.StartAmplitude = 0;
                accel[i].Amplitude.FreeRunAmplitude.Off.Parameter.StartFrequency = 0;
                accel[i].Amplitude.FreeRunAmplitude.On.Parameter.StartAmplitude = 0;
                accel[i].Amplitude.FreeRunAmplitude.On.Parameter.StartFrequency = 0;
            }

            var brake = data.BrakingPattern;
            for (int i = 0; i < brake.Count; i++)
            {
                brake[i].Amplitude.FreeRunAmplitude.Off.Parameter.StartAmplitude = 0;
                brake[i].Amplitude.FreeRunAmplitude.Off.Parameter.StartFrequency = 0;
                brake[i].Amplitude.FreeRunAmplitude.On.Parameter.StartAmplitude = 0;
                brake[i].Amplitude.FreeRunAmplitude.On.Parameter.StartFrequency = 0;
            }

            return true;
        }
    }
}

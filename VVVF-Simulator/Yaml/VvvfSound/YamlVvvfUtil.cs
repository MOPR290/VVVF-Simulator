using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace VvvfSimulator.Yaml.VvvfSound
{
    public class YamlVvvfUtil
    {
        public class NewtonMethod(NewtonMethod.Function function, double dx)
        {
            public delegate double Function(double x);
            private readonly Function function = function;

            public double Calculate(double begin, double tolerance, int n)
            {
                double x = begin;
                for (int i = 0; i < n; i++)
                {
                    double pre_x = x;
                    x = GetZeroIntersect(x);
                    if (pre_x == x || double.IsNaN(x) || double.IsInfinity(x)) x = pre_x + dx;
                    double fx = Math.Abs(function(x));
                    if (fx < tolerance) return x;
                }
                return x;
            }

            private double GetDerivative(double x)
            {
                double Fxdx = function(x + dx);
                double Fx = function(x);
                double Dy = Fxdx - Fx;
                double Dx = dx;
                double Derivative = Dy / Dx;
                return Derivative;
            }

            private double GetZeroIntersect(double x)
            {
                double zeroX = -function(x) / GetDerivative(x) + x;
                return zeroX;
            }
        }

        private static void AutoModulationIndexTask(YamlVvvfSoundData SoundData,bool IsBrakePattern, bool IsEnd,int Index, double MaxFrequency, double Presicion, int N)
        {
            List<YamlVvvfSoundData.YamlControlData> ysd = IsBrakePattern ? SoundData.BrakingPattern : SoundData.AcceleratePattern;
            var parameter = ysd[Index].Amplitude.DefaultAmplitude.Parameter;
            var parameter_freerun_on = ysd[Index].Amplitude.FreeRunAmplitude.On.Parameter;
            var parameter_freerun_off = ysd[Index].Amplitude.FreeRunAmplitude.Off.Parameter;

            if (ysd[Index].Amplitude.DefaultAmplitude.Mode != VvvfCalculate.AmplitudeMode.Linear) return;

            parameter.DisableRangeLimit = true;
            parameter.MaxAmplitude = -1;
            parameter.CutOffAmplitude = 0;
            parameter_freerun_on.DisableRangeLimit = true;
            parameter_freerun_on.MaxAmplitude = -1;
            parameter_freerun_on.CutOffAmplitude = 0;
            parameter_freerun_off.DisableRangeLimit = true;
            parameter_freerun_off.MaxAmplitude = -1;
            parameter_freerun_off.CutOffAmplitude = 0;

            if (!IsEnd)
            {
                parameter.StartFrequency = ysd[Index].ControlFrequencyFrom;
                parameter_freerun_on.StartFrequency = parameter.StartFrequency;
                parameter_freerun_off.StartFrequency = parameter.StartFrequency;
            }
            else
            {
                parameter.EndFrequency = (Index + 1) == ysd.Count ? MaxFrequency + (ysd[Index].ControlFrequencyFrom == MaxFrequency ? 0.1 : 0) : (ysd[Index + 1].ControlFrequencyFrom - 0.1);
                parameter_freerun_on.EndFrequency = parameter.EndFrequency;
                parameter_freerun_off.EndFrequency = parameter.EndFrequency;
            }
            double TargetFrequency = IsEnd ? parameter.EndFrequency : parameter.StartFrequency;
            double DesireVoltageRate = TargetFrequency / MaxFrequency;
            DesireVoltageRate = DesireVoltageRate > 1 ? 1 : DesireVoltageRate;

            VvvfValues control = new();
            control.ResetMathematicValues();
            control.ResetControlValues();
            control.SetSineAngleFrequency(TargetFrequency * Math.PI * 2);
            control.SetControlFrequency(TargetFrequency);
            control.SetMasconOff(false);
            control.SetFreeRun(false);
            control.SetBraking(IsBrakePattern);
            control.SetRandomFrequencyMoveAllowed(false);

            NewtonMethod.Function CalculateVoltageDifference = delegate (double amplitude)
            {
                if (IsEnd) parameter.EndAmplitude = amplitude;
                else parameter.StartAmplitude = amplitude;
                double difference = Generation.Video.ControlInfo.GenerateControlCommon.GetVoltageRate(control, SoundData, true) - DesireVoltageRate;
                return difference * 100;
            };

            NewtonMethod Calculator = new(CalculateVoltageDifference, 0.05);
            double ProperAmplitude = 0;
            try
            {
                ProperAmplitude = Calculator.Calculate(DesireVoltageRate, Presicion, N);
            }
            catch (Exception ex) {
                MessageBox.Show($"Auto Modulation Index set failed on item,\r\nIndex:{Index}\r\nIsBrake:{IsBrakePattern}\r\nIsEnd:{IsEnd}\r\nStackTrace:{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (IsEnd)
            {
                parameter.EndAmplitude = ProperAmplitude;
                parameter_freerun_on.EndAmplitude = ProperAmplitude;
                parameter_freerun_off.EndAmplitude = ProperAmplitude;
            }
            else {
                parameter.StartAmplitude = ProperAmplitude;
                parameter_freerun_on.StartAmplitude = ProperAmplitude;
                parameter_freerun_off.StartAmplitude = ProperAmplitude;
            }
        }
        public static bool AutoModulationIndex(YamlVvvfSoundData data)
        {
            if(data == null) return false;
            if(data.AcceleratePattern.Count == 0) return false;
            if(data.BrakingPattern.Count == 0) return false;

            List<YamlVvvfSoundData.YamlControlData> accel = data.AcceleratePattern;
            List<YamlVvvfSoundData.YamlControlData> brake = data.BrakingPattern;

            for (int i = 0; i < data.AcceleratePattern.Count; i++)
            {
                if (data.AcceleratePattern[i].ControlFrequencyFrom < 0) return false;
            }
            for (int i = 0; i < data.BrakingPattern.Count; i++)
            {
                if (data.BrakingPattern[i].ControlFrequencyFrom < 0) return false;
            }

            accel.Sort((a, b) => Math.Sign(a.ControlFrequencyFrom - b.ControlFrequencyFrom));
            brake.Sort((a, b) => Math.Sign(a.ControlFrequencyFrom - b.ControlFrequencyFrom));

            double accel_end_freq = accel[^1].ControlFrequencyFrom;
            double brake_end_freq = brake[^1].ControlFrequencyFrom;

            const double Precision = 0.5;
            const int N = 50;

            List<Task> tasks = [];
            for (int i = 0; i < accel.Count; i++)
            {
                int _i = i;
                tasks.Add(Task.Run(() => AutoModulationIndexTask(data, false, false, _i, accel_end_freq, Precision, N)));
                tasks.Add(Task.Run(() => AutoModulationIndexTask(data, false, true, _i, accel_end_freq, Precision, N)));
            }
            for (int i = 0; i < brake.Count; i++)
            {
                int _i = i;
                tasks.Add(Task.Run(() => AutoModulationIndexTask(data, true, false, _i, brake_end_freq, Precision, N)));
                tasks.Add(Task.Run(() => AutoModulationIndexTask(data, true, true, _i, brake_end_freq, Precision, N)));
            }
            Task.WaitAll([.. tasks]);

            accel.Sort((a, b) => Math.Sign(b.ControlFrequencyFrom - a.ControlFrequencyFrom));
            brake.Sort((a, b) => Math.Sign(b.ControlFrequencyFrom - a.ControlFrequencyFrom));

            return true;
        }

        public static bool SetFreeRunModulationIndexToZero(YamlVvvfSoundData data)
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

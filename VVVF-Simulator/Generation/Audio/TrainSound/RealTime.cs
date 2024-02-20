using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using VvvfSimulator.Properties;
using VvvfSimulator.Generation.Audio.TrainSound;
using VvvfSimulator.Yaml.VVVFSound;
using static VvvfSimulator.Generation.Audio.GenerateRealTimeCommon;
using static VvvfSimulator.Generation.Audio.TrainSound.Audio;
using static VvvfSimulator.Generation.Motor.GenerateMotorCore;
using static VvvfSimulator.Yaml.TrainAudio_Setting.YamlTrainSoundAnalyze;
using static VvvfSimulator.Generation.Audio.TrainSound.AudioFilter;
using System.Windows;

namespace VvvfSimulator.Generation.Audio.TrainSound
{
    unsafe public class RealTime
    {
        //---------- TRAIN SOUND --------------
        static readonly int calcCount = 512;
        private static int Calculate(BufferedWaveProvider provider, YamlVvvfSoundData sound_data, VvvfValues control, RealTimeParameter realTime_Parameter)
        {
            while (true)
            {
                int v = RealTime_CheckForFreq(control, realTime_Parameter, calcCount);
                if (v != -1) return v;

                for (int i = 0; i < calcCount; i++)
                {
                    control.add_Sine_Time(1.0 / 192000.0);
                    control.add_Saw_Time(1.0 / 192000.0);
                    control.Add_Generation_Current_Time(1.0 / 192000.0);

                    double value = CalculateTrainSound(control, sound_data , realTime_Parameter.Motor, realTime_Parameter.TrainSoundData);
                    
                    byte[] soundSample = BitConverter.GetBytes((float)value / 512);
                    if (!BitConverter.IsLittleEndian) Array.Reverse(soundSample);
                    provider.AddSamples(soundSample, 0, 4);
                }

                while (provider.BufferedBytes - calcCount > Settings.Default.RealTime_Train_BuffSize) ;
            }
        }

        public static void Generate(YamlVvvfSoundData ysd, RealTimeParameter parameter)
        {
            int SamplingFrequency = 192000;

            parameter.quit = false;
            parameter.VvvfSoundData = ysd;
            parameter.Motor = new MotorData() { 
                SIM_SAMPLE_FREQ = SamplingFrequency ,
                motor_Specification = parameter.TrainSoundData.MotorSpec.Clone(),
            };

            YamlTrainSoundData thd = parameter.TrainSoundData;

            VvvfValues control = new();
            control.reset_all_variables();
            control.reset_control_variables();
            parameter.Control = control;

            while (true)
            {
                var bufferedWaveProvider = new BufferedWaveProvider(WaveFormat.CreateIeeeFloatWaveFormat(SamplingFrequency,1));
                ISampleProvider sampleProvider = bufferedWaveProvider.ToSampleProvider();
                if (thd.UseFilteres) sampleProvider = new MonauralFilter(sampleProvider, thd.GetFilteres(SamplingFrequency));
                if (thd.UseImpulseResponse) sampleProvider = ImpulseResponse.FromSample(sampleProvider, calcCount, thd.ImpulseResponse);

                var mmDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                IWavePlayer wavPlayer = new WasapiOut(mmDevice, AudioClientShareMode.Shared, false, 0);

                wavPlayer.Init(sampleProvider);
                wavPlayer.Play();

                int stat;
                try
                {
                    stat = Calculate(bufferedWaveProvider, ysd, control, parameter);
                }
                catch
                {
                    wavPlayer.Stop();
                    wavPlayer.Dispose();

                    mmDevice.Dispose();
                    bufferedWaveProvider.ClearBuffer();

                    MessageBox.Show("An error occured on Audio processing.");

                    throw;
                }

                wavPlayer.Stop();
                wavPlayer.Dispose();

                mmDevice.Dispose();
                bufferedWaveProvider.ClearBuffer();

                if (stat == 0) break;
            }


        }
    }
}

using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using VvvfSimulator.Properties;
using VvvfSimulator.Generation.Audio.TrainSound;
using VvvfSimulator.Yaml.VVVFSound;
using static VvvfSimulator.Generation.Audio.GenerateRealTimeCommon;
using static VvvfSimulator.Generation.Audio.TrainSound.GenerateTrainAudio;
using static VvvfSimulator.Generation.Motor.GenerateMotorCore;
using static VvvfSimulator.Yaml.TrainAudio_Setting.YamlTrainSoundAnalyze;
using static VvvfSimulator.Generation.Audio.TrainSound.AudioFilter;
using System.Windows;

namespace VvvfSimulator.Generation.Audio.TrainSound
{
    unsafe public class RealTimeTrainAudio
    {
        //---------- TRAIN SOUND --------------
        static readonly int calcCount = 512;
        private static int RealTime_Train_Generation_Calculate(BufferedWaveProvider provider, YamlVvvfSoundData sound_data, VvvfValues control, RealTimeParameter realTime_Parameter)
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

                    double value = CalculateTrainSound(control, sound_data , realTime_Parameter.Motor, realTime_Parameter.Train_Sound_Data);
                    
                    byte[] soundSample = BitConverter.GetBytes((float)value / 512);
                    if (!BitConverter.IsLittleEndian) Array.Reverse(soundSample);
                    provider.AddSamples(soundSample, 0, 4);
                }

                while (provider.BufferedBytes - calcCount > Settings.Default.RealTime_Train_BuffSize) ;
            }
        }

        public static void RealTime_Train_Generation(YamlVvvfSoundData ysd, RealTimeParameter realTime_Parameter)
        {
            int SamplingFrequency = 192000;

            realTime_Parameter.quit = false;
            realTime_Parameter.sound_data = ysd;
            realTime_Parameter.Motor = new MotorData() { 
                SIM_SAMPLE_FREQ = SamplingFrequency ,
                motor_Specification = realTime_Parameter.Train_Sound_Data.Motor_Specification.Clone(),
            };

            YamlTrainSoundData thd = realTime_Parameter.Train_Sound_Data;

            VvvfValues control = new();
            control.reset_all_variables();
            control.reset_control_variables();
            realTime_Parameter.control_values = control;

            while (true)
            {
                var bufferedWaveProvider = new BufferedWaveProvider(WaveFormat.CreateIeeeFloatWaveFormat(SamplingFrequency,1));
                var monauralFilteredProvider = new MonauralFilter(bufferedWaveProvider.ToSampleProvider(), thd.Get_NFilters());
                var convolutionFilteredProvider = ImpulseResponse.FromSample(monauralFilteredProvider, calcCount);

                var mmDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                IWavePlayer wavPlayer = new WasapiOut(mmDevice, AudioClientShareMode.Shared, false, 0);

                wavPlayer.Init(convolutionFilteredProvider);
                wavPlayer.Play();

                int stat;
                try
                {
                    stat = RealTime_Train_Generation_Calculate(bufferedWaveProvider, ysd, control, realTime_Parameter);
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

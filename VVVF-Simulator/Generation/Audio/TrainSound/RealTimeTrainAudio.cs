using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using VVVF_Simulator.Generation.Audio.TrainSound;
using VvvfSimulator.Yaml.VVVFSound;
using static VvvfSimulator.Generation.Audio.GenerateRealTimeCommon;
using static VvvfSimulator.Generation.Audio.TrainSound.GenerateTrainAudio;
using static VvvfSimulator.Generation.Motor.GenerateMotorCore;
using static VvvfSimulator.Yaml.TrainAudio_Setting.YamlTrainSoundAnalyze;
using static VvvfSimulator.Generation.Audio.TrainSound.AudioFilter;
using System.Net;
using System.Windows;

namespace VvvfSimulator.Generation.Audio.TrainSound
{
    public class RealTimeTrainAudio
    {
        //---------- TRAIN SOUND --------------
        static int calcCount = 512;
        unsafe private static int RealTime_Train_Generation_Calculate(BufferedWaveProvider provider, YamlVvvfSoundData sound_data, VvvfValues control, RealTimeParameter realTime_Parameter)
        {

            CppAudioFilter cppAudioFilter = new CppAudioFilter();
            fixed (float* ir_address = &ImpulseResponseSample.dataArray[0])
            {
                cppAudioFilter.Init(calcCount, ir_address, ImpulseResponseSample.dataArray.Length);
            }
            

            while (true)
            {
                int v = RealTime_CheckForFreq(control, realTime_Parameter, calcCount);
                if (v != -1) return v;

                float[] samples = new float[calcCount];
                float[] samples_res = new float[calcCount];

                for (int i = 0; i < calcCount; i++)
                {
                    control.add_Sine_Time(1.0 / 192000.0);
                    control.add_Saw_Time(1.0 / 192000.0);
                    control.Add_Generation_Current_Time(1.0 / 192000.0);

                    double value = Get_Train_Sound(control, sound_data , realTime_Parameter.Motor, realTime_Parameter.Train_Sound_Data);
                    samples[i] = (float)value;
                }

                fixed (float* sample_array_address = &samples[0])
                {
                    fixed (float* sample_res_array_address = &samples_res[0])
                    {
                        cppAudioFilter.Process(sample_array_address, sample_res_array_address, calcCount);
                    }
                }

                for (int i = 0; i < calcCount; i++)
                {
                    byte[] soundSample = BitConverter.GetBytes(samples_res[i] / calcCount);
                    //byte[] soundSample = BitConverter.GetBytes((float)value);
                    if (!BitConverter.IsLittleEndian) Array.Reverse(soundSample);
                    provider.AddSamples(soundSample, 0, 4);
                }

                while (provider.BufferedBytes - calcCount > Properties.Settings.Default.RealTime_Train_BuffSize) ;
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
                var equalizer = new MonauralFilter(bufferedWaveProvider.ToSampleProvider(), thd.Get_NFilters());

                var mmDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                IWavePlayer wavPlayer = new WasapiOut(mmDevice, AudioClientShareMode.Shared, false, 0);

                wavPlayer.Init(equalizer);
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

                    MessageBox.Show("FUCK YOU!");

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

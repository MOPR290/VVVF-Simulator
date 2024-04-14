using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Generation.Audio.GenerateRealTimeCommon;

namespace VvvfSimulator.Generation.Audio.VvvfSound
{
    public class RealTime
    {
        // --------- VVVF SOUND ------------
        private static readonly int calcCount = 20;
        private static readonly int SamplingFrequency = 192000;
        private static int RealTime_VVVF_Generation_Calculate(BufferedWaveProvider provider, YamlVvvfSoundData sound_data, VvvfValues control, RealTimeParameter realTime_Parameter)
        {
            static void AddSample(float value, BufferedWaveProvider provider)
            {
                byte[] soundSample = BitConverter.GetBytes((float)value);
                if (!BitConverter.IsLittleEndian) Array.Reverse(soundSample);
                provider.AddSamples(soundSample, 0, 4);
            }

            while (true)
            {
                int v = RealTime_CheckForFreq(control, realTime_Parameter, calcCount / (double)SamplingFrequency);
                if (v != -1) return v;

                for (int i = 0; i < calcCount; i++)
                {
                    control.AddSineTime(1.0 / SamplingFrequency);
                    control.AddSawTime(1.0 / SamplingFrequency);
                    control.AddGenerationCurrentTime(1.0 / SamplingFrequency);

                    double sound_byte = Audio.CalculateVvvfSound(control, sound_data);
                    sound_byte /= 2.0;
                    sound_byte *= 0.7;

                    AddSample((float)sound_byte, provider);
                }

                while (provider.BufferedBytes > Properties.Settings.Default.RealTime_VVVF_BuffSize) ;
            }
        }
        public static void RealTime_VVVF_Generation(YamlVvvfSoundData ysd, RealTimeParameter realTime_Parameter)
        {
            realTime_Parameter.quit = false;
            realTime_Parameter.VvvfSoundData = ysd;

            VvvfValues control = new();
            control.ResetMathematicValues();
            control.ResetControlValues();
            realTime_Parameter.Control = control;

            while (true)
            {
                BufferedWaveProvider bufferedWaveProvider = new(WaveFormat.CreateIeeeFloatWaveFormat(SamplingFrequency, 1));
                MMDevice mmDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                WasapiOut wavPlayer = new(mmDevice, AudioClientShareMode.Shared, false, 0);

                wavPlayer.Init(bufferedWaveProvider);
                wavPlayer.Play();

                int stat;
                try
                {
                    stat = RealTime_VVVF_Generation_Calculate(bufferedWaveProvider, ysd, control, realTime_Parameter);
                }
                finally
                {
                    wavPlayer.Stop();
                    wavPlayer.Dispose();
                    mmDevice.Dispose();
                    bufferedWaveProvider.ClearBuffer();
                }

                if (stat == 0) break;
            }


        }
    }
}

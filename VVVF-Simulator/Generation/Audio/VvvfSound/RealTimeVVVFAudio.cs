using NAudio.CoreAudioApi;
using NAudio.Wave;
using VvvfSimulator.Yaml.VVVFSound;
using static VvvfSimulator.Generation.Audio.GenerateRealTimeCommon;
using static VvvfSimulator.VvvfStructs;

namespace VvvfSimulator.Generation.Audio.VVVF_Sound
{
    public class RealTimeVVVFAudio
    {


        // --------- VVVF SOUND ------------
        private static int RealTime_VVVF_Generation_Calculate(BufferedWaveProvider provider, YamlVvvfSoundData sound_data, VvvfValues control, RealTimeParameter realTime_Parameter)
        {
            while (true)
            {
                int bufsize = 20;

                int v = RealTime_CheckForFreq(control , realTime_Parameter, bufsize);
                if (v != -1) return v;

                byte[] add = new byte[bufsize];

                ControlStatus cv = new()
                {
                    brake = control.is_Braking(),
                    mascon_on = !control.is_Mascon_Off(),
                    free_run = control.is_Free_Running(),
                    wave_stat = control.get_Control_Frequency()
                };
                PwmCalculateValues calculated_Values = YamlVVVFWave.CalculateYaml(control, cv, sound_data);

                for (int i = 0; i < bufsize; i++)
                {
                    control.add_Sine_Time(1.0 / 192000.0);
                    control.add_Saw_Time(1.0 / 192000.0);
                    control.Add_Generation_Current_Time(1.0 / 192000.0);

                    byte sound_byte = GenerateVVVFAudio.CalculateVvvfSound(control, calculated_Values);

                    add[i] = sound_byte;
                }

                provider.AddSamples(add, 0, bufsize);
                while (provider.BufferedBytes > Properties.Settings.Default.RealTime_VVVF_BuffSize) ;
            }
        }
        public static void RealTime_VVVF_Generation(YamlVvvfSoundData ysd, RealTimeParameter realTime_Parameter)
        {
            realTime_Parameter.quit = false;
            realTime_Parameter.sound_data = ysd;

            VvvfValues control = new();
            control.reset_all_variables();
            control.reset_control_variables();
            realTime_Parameter.control_values = control;

            while (true)
            {
                var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(192000, 8, 1));

                var mmDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                IWavePlayer wavPlayer = new WasapiOut(mmDevice, AudioClientShareMode.Shared, false, 0);

                wavPlayer.Init(bufferedWaveProvider);
                wavPlayer.Play();



                int stat;
                try
                {
                    stat = RealTime_VVVF_Generation_Calculate(bufferedWaveProvider, ysd, control, realTime_Parameter);
                }
                catch
                {
                    wavPlayer.Stop();
                    wavPlayer.Dispose();

                    mmDevice.Dispose();
                    bufferedWaveProvider.ClearBuffer();

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

using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using VvvfSimulator.Yaml.VVVFSound;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Generation.GenerateCommon.GenerationBasicParameter;
using static VvvfSimulator.VvvfStructs;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;

namespace VvvfSimulator.Generation.Audio.VvvfSound
{
    public class GenerateVVVFAudio
    {
        // -------- VVVF SOUND -------------
        public static byte CalculateVvvfSound(VvvfValues control, YamlVvvfSoundData sound_data)
        {
            ControlStatus cv = new()
            {
                brake = control.IsBraking(),
                mascon_on = !control.IsMasconOff(),
                free_run = control.IsFreeRun(),
                wave_stat = control.GetControlFrequency()
            };
            PwmCalculateValues calculated_Values = YamlVVVFWave.CalculateYaml(control, cv, sound_data);
             return CalculateVvvfSound(control, calculated_Values);
        }

        public static byte CalculateVvvfSound(VvvfValues control, PwmCalculateValues calculated_Values)
        {
            WaveValues value = VvvfCalculate.CalculatePhases(control, calculated_Values, 0);

            double pwm_value = value.U - value.V;
            byte sound_byte = 0x80;
            if (pwm_value == 2) sound_byte += 0x40;
            else if (pwm_value == 1) sound_byte += 0x20;
            else if (pwm_value == -1) sound_byte -= 0x20;
            else if (pwm_value == -2) sound_byte -= 0x40;

            return sound_byte;
        }


        // Export Audio
        public static void Export_VVVF_Sound(GenerationBasicParameter generationBasicParameter ,String output_path, Boolean resize, int sample_freq)
        {
            YamlVvvfSoundData vvvfData = generationBasicParameter.vvvfData;
            YamlMasconDataCompiled masconData = generationBasicParameter.masconData;
            ProgressData progressData = generationBasicParameter.progressData;

            DateTime dt = DateTime.Now;
            String gen_time = dt.ToString("yyyy-MM-dd_HH-mm-ss");
            string temp = Path.GetDirectoryName(output_path) + "\\" + "temp-" + gen_time + ".wav";

            VvvfValues control = new();
            control.ResetControlValues();
            control.ResetMathematicValues();

            int sound_block_count = 0;

            BinaryWriter writer = new(new FileStream(resize ? temp : output_path, FileMode.Create));

            //WAV FORMAT DATA
            writer.Write(0x46464952); // RIFF
            writer.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 }); //CHUNK SIZE
            writer.Write(0x45564157); //WAVE
            writer.Write(0x20746D66); //fmt 
            writer.Write(16);
            writer.Write(new byte[] { 0x01, 0x00 }); // LINEAR PCM
            writer.Write(new byte[] { 0x01, 0x00 }); // MONORAL
            writer.Write(sample_freq); // SAMPLING FREQ
            writer.Write(sample_freq); // BYTES IN 1SEC
            writer.Write(new byte[] { 0x01, 0x00 }); // Block Size = 1
            writer.Write(new byte[] { 0x08, 0x00 }); // 1 Sample bits
            writer.Write(0x61746164);
            writer.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 }); //WAVE SIZE

            byte[] temp_bytes = new byte[19200];
            int temp_bytes_count = 0;

            //TASK DATA PREPARE
            progressData.Total = masconData.GetEstimatedSteps(1.0/sample_freq);

            while (true)
            {
                control.AddSineTime(1.00 / sample_freq);
                control.AddSawTime(1.00 / sample_freq);

                temp_bytes[temp_bytes_count] = CalculateVvvfSound(control, vvvfData);
                temp_bytes_count++;
                if (temp_bytes_count == 19200)
                {
                    writer.Write(temp_bytes);
                    temp_bytes_count = 0;
                }

                sound_block_count++;
                progressData.Progress = sound_block_count;

                bool flag_continue = CheckForFreqChange(control, masconData, vvvfData.mascon_data, 1.0 / sample_freq);
                bool flag_cancel = progressData.Cancel;

                if (flag_cancel || !flag_continue) break;

            }

            if (temp_bytes_count > 0)
                writer.Write(temp_bytes);

            writer.Seek(4, SeekOrigin.Begin);
            writer.Write(sound_block_count + 36);

            writer.Seek(40, SeekOrigin.Begin);
            writer.Write(sound_block_count);

            writer.Close();

            if (!resize) return;

            int outRate = 44800;
            using (var reader = new AudioFileReader(temp))
            {
                var resampler = new WdlResamplingSampleProvider(reader, outRate);
                WaveFileWriter.CreateWaveFile16(output_path, resampler);
            }

            File.Delete(temp);
        }
    }
}

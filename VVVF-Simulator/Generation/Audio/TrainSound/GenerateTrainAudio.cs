using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using VVVF_Simulator.Generation.Audio.TrainSound;
using VvvfSimulator.Yaml.VVVFSound;
using static VvvfSimulator.Generation.Audio.TrainSound.AudioFilter;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Generation.GenerateCommon.GenerationBasicParameter;
using static VvvfSimulator.Generation.Motor.GenerateMotorCore;
using static VvvfSimulator.MyMath;
using static VvvfSimulator.VvvfStructs;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;
using static VvvfSimulator.Yaml.TrainAudio_Setting.YamlTrainSoundAnalyze;
using static VvvfSimulator.Yaml.TrainAudio_Setting.YamlTrainSoundAnalyze.YamlTrainSoundData;

namespace VvvfSimulator.Generation.Audio.TrainSound
{
    public class GenerateTrainAudio
    {
        // -------- TRAIN SOUND --------------
        public static double Get_Train_Sound(VvvfValues control, YamlVvvfSoundData sound_data, MotorData motor, YamlTrainSoundData train_Harmonic_Data)
        {

            ControlStatus cv = new()
            {
                brake = control.is_Braking(),
                mascon_on = !control.is_Mascon_Off(),
                free_run = control.is_Free_Running(),
                wave_stat = control.get_Control_Frequency()
            };
            PwmCalculateValues calculated_Values = YamlVVVFWave.CalculateYaml(control, cv, sound_data);
            WaveValues value = VvvfCalculate.CalculatePhases(control, calculated_Values, 0);

            motor.motor_Param.sitamr = control.get_Video_Sine_Freq() * Math.PI * 2 * control.get_Sine_Time();
            motor.AynMotorControler(new WaveValues() { U = value.W, V = value.V, W = value.U });
            motor.Asyn_Moduleabc();

            double motorPwmSound = (motor.motor_Param.Te - motor.motor_Param.TePre) / 2.0;
            double motorSound = 0;

            // MOTOR HARMONICS
            for (int harmonic = 0; harmonic < train_Harmonic_Data.Sine_Harmonics.Count; harmonic++)
            {
                HarmonicData harmonic_data = train_Harmonic_Data.Sine_Harmonics[harmonic];

                if (harmonic_data.range.start > control.get_Sine_Freq()) continue;
                if (harmonic_data.range.end >= 0 && harmonic_data.range.end < control.get_Sine_Freq()) continue;

                var amplitude_data = harmonic_data.amplitude;

                double harmonic_freq = harmonic_data.harmonic * control.get_Sine_Freq();

                if (harmonic_freq > harmonic_data.disappear) continue;
                double sine_val = sin(control.get_Sine_Time() * control.get_Sine_Angle_Freq() * harmonic_data.harmonic);

                double amplitude = amplitude_data.start_val + (amplitude_data.end_val - amplitude_data.start_val) / (amplitude_data.end - harmonic_data.amplitude.start) * (control.get_Sine_Freq() - harmonic_data.amplitude.start);
                if (amplitude > amplitude_data.max_val) amplitude = amplitude_data.max_val;
                if (amplitude < amplitude_data.min_val) amplitude = amplitude_data.min_val;

                double amplitude_disappear = (harmonic_freq + 100.0 > harmonic_data.disappear) ?
                    ((harmonic_data.disappear - harmonic_freq) / 100.0) : 1;
                sine_val *= amplitude * amplitude_disappear;

                motorSound += Math.Round(sine_val);
            }

            // 
            // Gear Sound
            //
            List<HarmonicData> Gear_Harmonics = train_Harmonic_Data.Gear_Harmonics;
            double gearSound = 0;
            for (int harmonic = 0; harmonic < Gear_Harmonics.Count; harmonic++)
            {
                HarmonicData harmonic_data = Gear_Harmonics[harmonic];
                var amplitude_data = harmonic_data.amplitude;

                if (harmonic_data.range.start > control.get_Sine_Freq()) continue;
                if (harmonic_data.range.end >= 0 && harmonic_data.range.end < control.get_Sine_Freq()) continue;

                double harmonic_freq = harmonic_data.harmonic * control.get_Sine_Freq();

                if (harmonic_data.disappear != -1 && harmonic_freq > harmonic_data.disappear) continue;
                double sine_val = sin(control.get_Sine_Time() * control.get_Sine_Angle_Freq() * harmonic_data.harmonic);

                double amplitude = amplitude_data.start_val + (amplitude_data.end_val - amplitude_data.start_val) / (amplitude_data.end - harmonic_data.amplitude.start) * (control.get_Sine_Freq() - harmonic_data.amplitude.start);
                if (amplitude > amplitude_data.max_val) amplitude = amplitude_data.max_val;
                if (amplitude < amplitude_data.min_val) amplitude = amplitude_data.min_val;

                double amplitude_disappear = (harmonic_freq + 100.0 > harmonic_data.disappear) ?
                    ((harmonic_data.disappear - harmonic_freq) / 100.0) : 1;

                sine_val *= amplitude * (harmonic_data.disappear == -1 ? 1 : amplitude_disappear);
                gearSound += sine_val;
            }

            return motorPwmSound + motorSound + gearSound;
        }


        unsafe public static void Export_Train_Sound(GenerationBasicParameter generationBasicParameter, String output_path, Boolean resize, YamlTrainSoundData train_Sound_Data)
        {
            YamlVvvfSoundData vvvfData = generationBasicParameter.vvvfData;
            YamlMasconDataCompiled masconData = generationBasicParameter.masconData;
            ProgressData progressData = generationBasicParameter.progressData;

            DateTime dt = DateTime.Now;
            String gen_time = dt.ToString("yyyy-MM-dd_HH-mm-ss");
            string temp = Path.GetDirectoryName(output_path) + "\\" + "temp-" + gen_time + ".wav";

            VvvfValues control = new();
            control.reset_control_variables();
            control.reset_all_variables();

            int SamplingFrequency = 200000;

            WaveFormat format = WaveFormat.CreateIeeeFloatWaveFormat(SamplingFrequency, 1);
            BufferedWaveProvider waveBuffer = new(format)
            {
                BufferLength = 80000
            };
            MonauralFilter equalizer = new(waveBuffer.ToSampleProvider(), train_Sound_Data.Get_NFilters());
            IWaveProvider equalizerBuffer = equalizer.ToWaveProvider();
            WaveFileWriter writer = new(resize ? temp : output_path, format);

            MotorData motor = new()
            {
                motor_Specification = train_Sound_Data.Motor_Specification.Clone(),
                SIM_SAMPLE_FREQ = SamplingFrequency,
            };
            motor.motor_Param.TL = 0.0;

            progressData.Total = masconData.GetEstimatedSteps(1.0 / SamplingFrequency);

            CppAudioFilter cppAudioFilter = new CppAudioFilter();
            fixed (float* ir_address = &ImpulseResponseSample.dataArray[0])
            {
                cppAudioFilter.Init(4096 * 8, ir_address, ImpulseResponseSample.dataArray.Length);
            }

            float[] soundBuff = new float[waveBuffer.BufferLength / 4];
            int soundBuffIndex = 0;

            while (true)
            {
                control.add_Sine_Time(1.00 / SamplingFrequency);
                control.add_Saw_Time(1.00 / SamplingFrequency);

                double sound = Get_Train_Sound(control, vvvfData, motor , train_Sound_Data);
                soundBuff[soundBuffIndex++] = (float)sound;

                progressData.Progress++;

                bool flag_continue = CheckForFreqChange(control, masconData, vvvfData.mascon_data, 1.0 / SamplingFrequency);
                bool flag_cancel = progressData.Cancel;
                if (!flag_continue || flag_cancel) break;

                if (soundBuffIndex < soundBuff.Length) continue;
                soundBuffIndex = 0;

                float[] result_array = new float[soundBuff.Length];

                fixed (float* sample_array_address = &soundBuff[0])
                {
                    fixed (float* result_array_address = &result_array[0])
                    {
                        cppAudioFilter.Process(sample_array_address, result_array_address, soundBuff.Length);
                    }

                }

                for(int i = 0; i < result_array.Length; i++)
                {
                    byte[] soundBytes = BitConverter.GetBytes(result_array[i] / result_array.Length);
                    if (!BitConverter.IsLittleEndian) Array.Reverse(soundBytes);
                    waveBuffer.AddSamples(soundBytes, 0, 4);

                    if (waveBuffer.BufferedBytes == waveBuffer.BufferLength)
                    {
                        byte[] buffer = new byte[waveBuffer.BufferedBytes];
                        int bytesRead = equalizerBuffer.Read(buffer, 0, buffer.Length);
                        writer.Write(buffer, 0, bytesRead);
                    }
                }

                
            }

            //last 
            if (waveBuffer.BufferedBytes > 0)
            {
                byte[] buffer = new byte[waveBuffer.BufferedBytes];
                int bytesRead = equalizerBuffer.Read(buffer, 0, buffer.Length);
                writer.Write(buffer, 0, bytesRead);
            }

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

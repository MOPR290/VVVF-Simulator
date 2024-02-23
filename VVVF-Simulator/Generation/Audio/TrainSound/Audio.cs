using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using VvvfSimulator.Generation.Audio.TrainSound;
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
    public class Audio
    {
        // -------- TRAIN SOUND --------------
        public static double CalculateMotorSound(VvvfValues control, YamlVvvfSoundData sound_data, MotorData motor)
        {
            ControlStatus cv = new()
            {
                brake = control.IsBraking(),
                mascon_on = !control.IsMasconOff(),
                free_run = control.IsFreeRun(),
                wave_stat = control.GetControlFrequency()
            };
            PwmCalculateValues calculated_Values = YamlVVVFWave.CalculateYaml(control, cv, sound_data);
            WaveValues value = VvvfCalculate.CalculatePhases(control, calculated_Values, 0);

            motor.motor_Param.sitamr = control.GetVideoSineFrequency() * Math.PI * 2 * control.GetSineTime();
            motor.AynMotorControler(new WaveValues() { U = value.W, V = value.V, W = value.U });
            motor.Asyn_Moduleabc();

            double motorTorqueSound = motor.motor_Param.Te - motor.motor_Param.TePre;

            return motorTorqueSound;
        }

        public static double CalculateHarmonicSounds(VvvfValues control, List<HarmonicData> harmonics)
        {
            double sound = 0;
            for (int harmonic = 0; harmonic < harmonics.Count; harmonic++)
            {
                HarmonicData harmonic_data = harmonics[harmonic];
                var amplitude_data = harmonic_data.Amplitude;

                if (harmonic_data.Range.Start > control.GetSineFrequency()) continue;
                if (harmonic_data.Range.End >= 0 && harmonic_data.Range.End < control.GetSineFrequency()) continue;

                double harmonic_freq = harmonic_data.Harmonic * control.GetSineFrequency();

                if (harmonic_data.Disappear != -1 && harmonic_freq > harmonic_data.Disappear) continue;
                double sine_val = sin(control.GetSineTime() * control.GetSineAngleFrequency() * harmonic_data.Harmonic);

                double amplitude = amplitude_data.StartValue + (amplitude_data.EndValue - amplitude_data.StartValue) / (amplitude_data.End - harmonic_data.Amplitude.Start) * (control.GetSineFrequency() - harmonic_data.Amplitude.Start);
                if (amplitude > amplitude_data.MaximumValue) amplitude = amplitude_data.MaximumValue;
                if (amplitude < amplitude_data.MinimumValue) amplitude = amplitude_data.MinimumValue;

                double amplitude_disappear = (harmonic_freq + 100.0 > harmonic_data.Disappear) ?
                    ((harmonic_data.Disappear - harmonic_freq) / 100.0) : 1;

                sine_val *= amplitude * (harmonic_data.Disappear == -1 ? 1 : amplitude_disappear);
                sound += sine_val;
            }
            return sound;
        }

        public static double CalculateTrainSound(VvvfValues control, YamlVvvfSoundData sound_data, MotorData motor, YamlTrainSoundData train_sound_data)
        {
            double motorPwmSound = CalculateMotorSound(control, sound_data, motor) * Math.Pow(10, train_sound_data.MotorVolumeDb);
            double motorSound = CalculateHarmonicSounds(control, train_sound_data.HarmonicSound);
            double gearSound = CalculateHarmonicSounds(control, train_sound_data.GearSound);

            double signal = (motorPwmSound + motorSound + gearSound) * Math.Pow(10, train_sound_data.TotalVolumeDb);

            return signal;
        }


        unsafe public static void Generate(GenerationBasicParameter generationBasicParameter, string output_path, bool resize, YamlTrainSoundData soundData)
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

            int SamplingFrequency = 200000;

            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(SamplingFrequency, 1);
            var bufferedWaveProvider = new BufferedWaveProvider(waveFormat);
            ISampleProvider sampleProvider = bufferedWaveProvider.ToSampleProvider();
            if (soundData.UseFilteres) sampleProvider = new MonauralFilter(sampleProvider, soundData.GetFilteres(SamplingFrequency));
            if (soundData.UseImpulseResponse) sampleProvider = ImpulseResponse.FromSample(sampleProvider, 4096, soundData.ImpulseResponse);
            WaveFileWriter writer = new(resize ? temp : output_path, waveFormat);

            MotorData motor = new()
            {
                motor_Specification = soundData.MotorSpec.Clone(),
                SIM_SAMPLE_FREQ = SamplingFrequency,
            };
            motor.motor_Param.TL = 0.0;

            progressData.Total = masconData.GetEstimatedSteps(1.0 / SamplingFrequency);

            while (true)
            {
                control.AddSineTime(1.00 / SamplingFrequency);
                control.AddSawTime(1.00 / SamplingFrequency);

                float sound = (float)CalculateTrainSound(control, vvvfData, motor , soundData);

                byte[] soundBytes = BitConverter.GetBytes(sound / 512);
                if (!BitConverter.IsLittleEndian) Array.Reverse(soundBytes);
                bufferedWaveProvider.AddSamples(soundBytes, 0, 4);

                if (bufferedWaveProvider.BufferedBytes == bufferedWaveProvider.BufferLength)
                {
                    byte[] buffer = new byte[bufferedWaveProvider.BufferedBytes];
                    int bytesRead = sampleProvider.ToWaveProvider().Read(buffer, 0, buffer.Length);
                    writer.Write(buffer, 0, bytesRead);
                }

                progressData.Progress++;

                bool flag_continue = CheckForFreqChange(control, masconData, vvvfData.mascon_data, 1.0 / SamplingFrequency);
                bool flag_cancel = progressData.Cancel;
                if (!flag_continue || flag_cancel) break;
            }

            //last 
            if (bufferedWaveProvider.BufferedBytes > 0)
            {
                byte[] buffer = new byte[bufferedWaveProvider.BufferedBytes];
                int bytesRead = sampleProvider.ToWaveProvider().Read(buffer, 0, buffer.Length);
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

            System.IO.File.Delete(temp);
        }
    }
}

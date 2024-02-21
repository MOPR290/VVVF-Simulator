using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using OpenCvSharp;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static VvvfSimulator.Generation.Audio.TrainSound.AudioFilter;

namespace VvvfSimulator.Generation.Audio.TrainSound
{
    public class ImpulseResponse
    {
        public static float[] ReadSample(ISampleProvider sampleProvider)
        {
            List<float> samples = [];
            while (true)
            {
                float[] read = new float[1024];
                int read_count = sampleProvider.Read(read, 0, 1024);
                samples.AddRange(read);
                if (read_count < 1024) break;
            }

            float[] samples_float = [.. samples];

            return samples_float;
        }
        public static float[] ReadAudioFileSample(string path)
        {
            // 192000 kHz
            AudioFileReader audioReader = new(path);
            ISampleProvider monoProvider = audioReader.ToMono();
            WdlResamplingSampleProvider resampler = new(monoProvider, 192000);
            return ReadSample(resampler);
        }

        public static string SampleIrPath { get; set; } = "VvvfSimulator.Generation.Audio.TrainSound.IrSample.sample6.wav";
        public static float[] ReadResourceAudioFileSample(string path)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream? resource = assembly.GetManifestResourceStream(path);

            if (resource == null) return [1];

            WaveFileReader waveReader = new(resource);
            ISampleProvider monoProvider = waveReader.ToSampleProvider().ToMono();
            WdlResamplingSampleProvider resampler = new(monoProvider, 192000);
            return ReadSample(resampler);
        }
    
        public static CppConvolutionFilter FromSample(ISampleProvider provider, int block, float[] samples)
        {
            CppConvolutionFilter cppConvolutionFilter = new(provider);
            cppConvolutionFilter.Init(block, samples);
            return cppConvolutionFilter;
        }

        

    }
}

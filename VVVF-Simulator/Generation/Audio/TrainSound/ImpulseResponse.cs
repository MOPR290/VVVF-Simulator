using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static VvvfSimulator.Generation.Audio.TrainSound.AudioFilter;

namespace VvvfSimulator.Generation.Audio.TrainSound
{
    public class ImpulseResponse
    {
    
        public static CppConvolutionFilter FromAudio(ISampleProvider provider, int block, string path)
        {
            // 192000 kHz
            AudioFileReader audioReader = new(path);
            ISampleProvider monoProvider = audioReader.ToMono();
            WdlResamplingSampleProvider resampler = new(monoProvider, 192000);

            List<float> samples = new();
            while (true)
            {
                float[] read = new float[1024];
                int read_count = resampler.Read(read, 0, 1024);
                samples.AddRange(read);
                if (read_count < 1024) break;
            }

            float[] samples_float = samples.ToArray();

            CppConvolutionFilter cppConvolutionFilter = new(provider);
            cppConvolutionFilter.Init(block, samples_float);

            return cppConvolutionFilter;
        }
        
        public static CppConvolutionFilter FromSample(ISampleProvider provider,int block)
        {
            CppConvolutionFilter cppConvolutionFilter = new(provider);
            cppConvolutionFilter.Init(block, ImpulseResponseSample.data);
            return cppConvolutionFilter;
        }
    }
}

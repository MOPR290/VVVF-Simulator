using NAudio.Dsp;
using NAudio.Wave;
using System.Runtime.InteropServices;

namespace VvvfSimulator.Generation.Audio.TrainSound
{
    public class AudioFilter
    {
        public class MonauralFilter : ISampleProvider
        {
            private readonly ISampleProvider sourceProvider;
            private BiQuadFilter[,] filters;
            private int filterCount;
            private bool updated;
            public MonauralFilter(ISampleProvider sourceProvider, BiQuadFilter[,] filters)
            {
                this.sourceProvider = sourceProvider;
                filterCount = filters.Length;
                this.filters = filters;
            }

            public void Update(BiQuadFilter[,] filters)
            {
                this.filters = filters;
                this.filterCount = filters.Length;
                updated = true;
            }
            //       public WaveFormat WaveFormat => sourceProvider.WaveFormat;
            public WaveFormat WaveFormat
            {
                get
                {
                    return sourceProvider.WaveFormat;
                }
            }

            public int Read(float[] buffer, int offset, int count)
            {
                int samplesRead = sourceProvider.Read(buffer, offset, count);

                if (updated)
                {
                    updated = false;
                }

                for (int sample = 0; sample < samplesRead; sample++)
                {
                    for (int band = 0; band < filterCount; band++)
                    {
                        buffer[offset + sample] = filters[0, band].Transform(buffer[offset + sample]);
                    }
                }
                return samplesRead;
            }
        }

        public class CppAudioFilter
        {
            readonly ulong address;

            [DllImport("AudioFilter.dll")]
            private static extern ulong createConvolverInstance();

            [DllImport("AudioFilter.dll")]
            unsafe private static extern bool init(ulong address, long blockSize, float* ir, long irLen);

            [DllImport("AudioFilter.dll")]
            unsafe private static extern void process(ulong address, float* input, float* output, long len);

            [DllImport("AudioFilter.dll")]
            private static extern void reset(ulong address);


            public CppAudioFilter()
            {
                address = createConvolverInstance();
            }

            public void Reset()
            {
                reset(address);
            }

            unsafe public bool Init(long blockSize, float* ir, long irLen)
            {
                return init(address, blockSize, ir, irLen);
            }

            unsafe public void Process(float* input, float* output, long len)
            {
                process(address, input, output, len);
            }
        }

    }
}

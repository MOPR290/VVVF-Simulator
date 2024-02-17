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

        public class CppConvolutionFilter : ISampleProvider
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

            // Audio Handler

            private readonly ISampleProvider sourceProvider;
            private long blockSize;
            public WaveFormat WaveFormat
            {
                get
                {
                    return sourceProvider.WaveFormat;
                }
            }
            public CppConvolutionFilter(ISampleProvider sourceProvider)
            {
                address = createConvolverInstance();
                this.sourceProvider = sourceProvider;
            }
            unsafe public int Read(float[] buffer, int offset, int count)
            {
                if (count < blockSize) return 0;

                int samplesRead = sourceProvider.Read(buffer, offset, count);

                fixed (float* array_address = &buffer[0])
                {
                    Process(array_address, array_address, samplesRead);
                }
                return samplesRead;
            }
            public void Reset()
            {
                reset(address);
            }
            unsafe public bool Init(long blockSize, float* ir, long irLen)
            {
                this.blockSize = blockSize;
                return init(address, blockSize, ir, irLen);
            }
            unsafe public bool Init(long blockSize, float[] response)
            {
                this.blockSize = blockSize;
                fixed (float* ir_address = &response[0])
                {
                    return init(address, blockSize, ir_address, response.Length);
                }
            }
            unsafe public void Process(float* input, float* output, long len)
            {
                process(address, input, output, len);
            }
        }

    }
}

using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VvvfSimulator.Generation.Audio.TrainSound
{
    public class GenerateTrainAudioFilter
    {
        public class NAudioFilter
        {

            /// <summary>
            /// Basic example of a multi-band eq
            /// uses the same settings for both channels in stereo audio
            /// Call Update after you've updated the bands
            /// Potentially to be added to NAudio in a future version
            /// </summary>
            public class Equalizer : ISampleProvider
            {
                private readonly ISampleProvider sourceProvider;
                private BiQuadFilter[,] filters;
                private readonly int channels;
                private int filterCount;
                private bool updated;

                public Equalizer(ISampleProvider sourceProvider, BiQuadFilter[,] filters)
                {
                    this.sourceProvider = sourceProvider;
                    channels = sourceProvider.WaveFormat.Channels;
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
                        int ch = sample % channels;

                        for (int band = 0; band < filterCount; band++)
                        {
                            buffer[offset + sample] = filters[ch, band].Transform(buffer[offset + sample]);
                        }
                    }
                    return samplesRead;
                }
            }

        }
    }
}

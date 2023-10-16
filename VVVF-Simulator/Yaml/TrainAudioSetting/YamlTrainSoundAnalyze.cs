using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using static VvvfSimulator.Generation.Motor.GenerateMotorCore.MotorData;

namespace VvvfSimulator.Yaml.TrainAudio_Setting
{
    public class YamlTrainSoundAnalyze
    {
        public class YamlTrainSoundData
        {
            private int FinalSampleFreq { get; set; } = 192000;
            public List<HarmonicData> Gear_Harmonics { get; set; } = new List<HarmonicData>()
            {
                new HarmonicData{harmonic = 14, amplitude = new HarmonicData.HarmonicDataAmplitude{start=0,start_val=0x10,end=60,end_val=0x30,min_val = 0,max_val=0x60},disappear = 10000},
                new HarmonicData{harmonic = 99, amplitude = new HarmonicData.HarmonicDataAmplitude{start=0,start_val=0x10,end=60,end_val=0x30,min_val = 0,max_val=0x60},disappear = 10000},
            };
            public List<HarmonicData> Sine_Harmonics { get; set; } = new List<HarmonicData>()
            {
                new HarmonicData{harmonic = 1, amplitude = new HarmonicData.HarmonicDataAmplitude{start=0,start_val=0x10,end=60,end_val=0x30,min_val = 0,max_val=0x60},disappear = 10000},
                new HarmonicData{harmonic = 5, amplitude = new HarmonicData.HarmonicDataAmplitude{start=0,start_val=0x10,end=60,end_val=0x30,min_val = 0,max_val=0x60},disappear = 10000},
                new HarmonicData{harmonic = 7, amplitude = new HarmonicData.HarmonicDataAmplitude{start=0,start_val=0x10,end=60,end_val=0x30,min_val = 0,max_val=0x60},disappear = 10000},
            };
            public List<SoundFilter> Filteres { get; set; } = new List<SoundFilter>()
            {
                new(SoundFilter.FilterType.HighPassFilter,-1,50,2f),
                new(SoundFilter.FilterType.LowPassFilter,-1,900,2f),
            };

            public MotorSpecification Motor_Specification { get; set; } = new MotorSpecification();

            

            public class SoundFilter
            {
                public FilterType filterType { get; set; }
                public float Gain { get; set; }
                public float Frequency { get; set; }
                public float Q { get; set; }
                public enum FilterType
                {
                    PeakingEQ, HighPassFilter, LowPassFilter, NotchFilter
                }

                public SoundFilter(FilterType filterType, float gain, float frequency, float q)
                {
                    this.filterType = filterType;
                    this.Gain = gain;
                    this.Frequency = frequency;
                    this.Q = q;
                }

                public SoundFilter() { }

                public SoundFilter Clone()
                {
                    return (SoundFilter)MemberwiseClone();
                }
            }
            public class HarmonicData
            {
                public double harmonic { get; set; } = 0;
                public HarmonicDataAmplitude amplitude { get; set; } = new();
                public HarmonicDataRange range { get; set; } = new();
                public double disappear { get; set; } = 0;

                public class HarmonicDataAmplitude
                {
                    public double start { get; set; } = 0;
                    public double start_val { get; set; } = 0;
                    public double end { get; set; } = 0;
                    public double end_val { get; set; } = 0;

                    public double min_val { get; set; } = 0;
                    public double max_val { get; set; } = 0x60;

                    public HarmonicDataAmplitude Clone()
                    {
                        return (HarmonicDataAmplitude)MemberwiseClone();
                    }
                }

                public class HarmonicDataRange
                {
                    public double start { get; set; } = 0;
                    public double end { get; set; } = -1;

                    public HarmonicDataRange Clone()
                    {
                        return (HarmonicDataRange)MemberwiseClone();
                    }
                }

                public HarmonicData Clone()
                {
                    var cloned = (HarmonicData)MemberwiseClone();

                    cloned.amplitude = amplitude.Clone();
                    cloned.range = range.Clone();

                    return cloned;
                }
            }


            private BiQuadFilter[,] NFilteres = new BiQuadFilter[0, 0];
            public void Set_NFilteres(int SampleFreq)
            {
                FinalSampleFreq = SampleFreq;
                BiQuadFilter[,] nFilteres = new BiQuadFilter[1, Filteres.Count];
                for (int i = 0; i < Filteres.Count; i++)
                {
                    SoundFilter sf = Filteres[i];
                    BiQuadFilter bqf;
                    switch (sf.filterType)
                    {
                        case SoundFilter.FilterType.PeakingEQ:
                            {
                                bqf = BiQuadFilter.PeakingEQ(SampleFreq, sf.Frequency, sf.Q, sf.Gain);
                                break;
                            }
                        case SoundFilter.FilterType.HighPassFilter:
                            {
                                bqf = BiQuadFilter.HighPassFilter(SampleFreq, sf.Frequency, sf.Q);
                                break;
                            }
                        case SoundFilter.FilterType.LowPassFilter:
                            {
                                bqf = BiQuadFilter.LowPassFilter(SampleFreq, sf.Frequency, sf.Q);
                                break;
                            }
                        default: //case SoundFilter.FilterType.NotchFilter:
                            {
                                bqf = BiQuadFilter.NotchFilter(SampleFreq, sf.Frequency, sf.Q);
                                break;
                            }
                    }
                    nFilteres[0, i] = bqf;
                }

                NFilteres = nFilteres;

            }
            public BiQuadFilter[,] Get_NFilters() { return NFilteres; }


            public void Set_Calculated_Gear_Harmonics(int Gear1, int Gear2)
            {
                List<HarmonicData> Gear_Harmonics_List = new List<HarmonicData>();

                HarmonicData.HarmonicDataAmplitude amp_Strong = new HarmonicData.HarmonicDataAmplitude { start = 0, start_val = 0x0, end = 40, end_val = 0x60, min_val = 0, max_val = 0x60 };
                HarmonicData.HarmonicDataAmplitude amp_Weak = new HarmonicData.HarmonicDataAmplitude { start = 0, start_val = 0x0, end = 40, end_val = 0x20, min_val = 0, max_val = 0x20 };

                double gear_rate = Gear2 / (double)Gear1;
                double motor_r = 120 / 4 / 60.0;

                // Sound From Gear 1
                // It has `Gear1` amount of teeth.
                Gear_Harmonics_List.Add(new HarmonicData { harmonic = motor_r * Gear1 * 3, amplitude = amp_Strong, disappear = -1 });
                Gear_Harmonics_List.Add(new HarmonicData { harmonic = motor_r * Gear1 * 2.5, amplitude = amp_Strong, disappear = -1 });
                Gear_Harmonics_List.Add(new HarmonicData { harmonic = motor_r * Gear1 * 1, amplitude = amp_Strong, disappear = -1 });

                Gear_Harmonics_List.Add(new HarmonicData { harmonic = motor_r * 86 * 2, amplitude = new HarmonicData.HarmonicDataAmplitude { start = 0, start_val = 0x0, end = 20, end_val = 0x60, min_val = 0, max_val = 0x30 }, disappear = -1 });

                Gear_Harmonics = new List<HarmonicData>(Gear_Harmonics_List);
            }
            public YamlTrainSoundData Clone()
            {
                var cloned = (YamlTrainSoundData)MemberwiseClone();

                cloned.Gear_Harmonics = new List<HarmonicData>(Gear_Harmonics);
                cloned.Sine_Harmonics = new List<HarmonicData>(Sine_Harmonics);
                cloned.Filteres = new List<SoundFilter>(Filteres);
                cloned.Set_NFilteres(cloned.FinalSampleFreq);

                return cloned;
            }
            public YamlTrainSoundData(int SampleFreq, int Gear1, int Gear2)
            {
                Set_NFilteres(SampleFreq);
                Set_Calculated_Gear_Harmonics(Gear1, Gear2);
            }

            public YamlTrainSoundData() { }
        }

        public class YamlTrainSoundDataManage
        {
            public static YamlTrainSoundData current_data { get; set; } = new YamlTrainSoundData(192000, 16, 101);

            public static bool SaveYaml(String path)
            {
                try
                {
                    using TextWriter writer = File.CreateText(path);
                    var serializer = new Serializer();
                    serializer.Serialize(writer, current_data);
                    writer.Close();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public static YamlTrainSoundData LoadYaml(String path)
            {
                try
                {
                    var input = new StreamReader(path, Encoding.UTF8);
                    var deserializer = new Deserializer();
                    YamlTrainSoundData deserializeObject = deserializer.Deserialize<YamlTrainSoundData>(input);
                    input.Close();
                    return deserializeObject;
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}

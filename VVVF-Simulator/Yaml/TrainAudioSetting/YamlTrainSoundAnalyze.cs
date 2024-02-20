using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VvvfSimulator.Generation.Audio.TrainSound;
using YamlDotNet.Serialization;
using static VvvfSimulator.Generation.Motor.GenerateMotorCore.MotorData;

namespace VvvfSimulator.Yaml.TrainAudio_Setting
{
    public class YamlTrainSoundAnalyze
    {
        public class YamlTrainSoundData
        {
            public List<HarmonicData> GearSound { get; set; } = new List<HarmonicData>()
            {
                new() {Harmonic = 14, Amplitude = new HarmonicData.HarmonicDataAmplitude{Start=0,StartValue=0.1,End=60,EndValue=0.2,MinimumValue = 0,MaximumValue=0.2},Disappear = 10000},
                new() {Harmonic = 99, Amplitude = new HarmonicData.HarmonicDataAmplitude{Start=0,StartValue=0.1,End=60,EndValue=0.2,MinimumValue = 0,MaximumValue=0.2},Disappear = 10000},
            };
            public List<HarmonicData> HarmonicSound { get; set; } = new List<HarmonicData>()
            {
                new() {Harmonic = 1, Amplitude = new HarmonicData.HarmonicDataAmplitude{Start=0,StartValue=0.1,End=60,EndValue=0.2,MinimumValue = 0,MaximumValue=0.2},Disappear = 10000},
                new() {Harmonic = 5, Amplitude = new HarmonicData.HarmonicDataAmplitude{Start=0,StartValue=0.1,End=60,EndValue=0.2,MinimumValue = 0,MaximumValue=0.2},Disappear = 10000},
                new() {Harmonic = 7, Amplitude = new HarmonicData.HarmonicDataAmplitude{Start=0,StartValue=0.1,End=60,EndValue=0.2,MinimumValue = 0,MaximumValue=0.2},Disappear = 10000},
            };
            public bool UseFilteres { get; set; } = true;
            public List<SoundFilter> Filteres { get; set; } = new List<SoundFilter>()
            {
                new(SoundFilter.FilterType.HighPassFilter,-3,50,2f),
                new(SoundFilter.FilterType.LowPassFilter,-3,900,2f),
            };
            public bool UseImpulseResponse { get; set; } = true;
            public float[] ImpulseResponse { get; set; } = ImpulseResponseSample.data;
            public MotorSpecification MotorSpec { get; set; } = new MotorSpecification();
            public double MotorVolumeDb { get; set; } = 0.0;
            public double TotalVolumeDb { get; set; } = -0.5;

            public class SoundFilter
            {
                public FilterType Type { get; set; }
                public float Gain { get; set; }
                public float Frequency { get; set; }
                public float Q { get; set; }
                public enum FilterType
                {
                    PeakingEQ, HighPassFilter, LowPassFilter, NotchFilter
                }

                public SoundFilter(FilterType filterType, float gain, float frequency, float q)
                {
                    this.Type = filterType;
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
                public double Harmonic { get; set; } = 0;
                public HarmonicDataAmplitude Amplitude { get; set; } = new();
                public HarmonicDataRange Range { get; set; } = new();
                public double Disappear { get; set; } = 0;

                public class HarmonicDataAmplitude
                {
                    public double Start { get; set; } = 0;
                    public double StartValue { get; set; } = 0;
                    public double End { get; set; } = 0;
                    public double EndValue { get; set; } = 0;
                    public double MinimumValue { get; set; } = 0;
                    public double MaximumValue { get; set; } = 0x60;

                    public HarmonicDataAmplitude Clone()
                    {
                        return (HarmonicDataAmplitude)MemberwiseClone();
                    }
                }

                public class HarmonicDataRange
                {
                    public double Start { get; set; } = 0;
                    public double End { get; set; } = -1;

                    public HarmonicDataRange Clone()
                    {
                        return (HarmonicDataRange)MemberwiseClone();
                    }
                }

                public HarmonicData Clone()
                {
                    var cloned = (HarmonicData)MemberwiseClone();

                    cloned.Amplitude = Amplitude.Clone();
                    cloned.Range = Range.Clone();

                    return cloned;
                }
            }
            public BiQuadFilter[,] GetFilteres(int SampleFreq)
            {
                BiQuadFilter[,] nFilteres = new BiQuadFilter[1, Filteres.Count];
                for (int i = 0; i < Filteres.Count; i++)
                {
                    SoundFilter sf = Filteres[i];
                    BiQuadFilter bqf;
                    switch (sf.Type)
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
                return nFilteres;
            }
            public void SetCalculatedGearHarmonics(int Gear1, int Gear2)
            {
                List<HarmonicData> GearHarmonicsList = new();

                HarmonicData.HarmonicDataAmplitude amp_Strong = new() { Start = 0, StartValue = 0x0, End = 40, EndValue = 0.1, MinimumValue = 0, MaximumValue = 0.1 };
                HarmonicData.HarmonicDataAmplitude amp_Weak = new() { Start = 0, StartValue = 0x0, End = 40, EndValue = 0.05, MinimumValue = 0, MaximumValue = 0.05 };

                double motor_r = 120 / 6 / 60.0;

                // Sound From Gear 1
                int[] harmonics = { 1, 3, 5, 9 };
                for(int i = 0; i < harmonics.Length; i++)
                {
                    int k = harmonics[i];
                    GearHarmonicsList.Add(new HarmonicData { Harmonic = motor_r * Gear1 * k, Amplitude = amp_Strong, Disappear = -1 });
                }

                // IDK
                GearHarmonicsList.Add(new HarmonicData { Harmonic = motor_r * Gear1 * Gear2 / 9, Amplitude = amp_Weak, Disappear = -1 });

                GearSound = new List<HarmonicData>(GearHarmonicsList);
            }
            public YamlTrainSoundData Clone()
            {
                var cloned = (YamlTrainSoundData)MemberwiseClone();

                cloned.GearSound = new List<HarmonicData>(GearSound);
                cloned.HarmonicSound = new List<HarmonicData>(HarmonicSound);
                cloned.Filteres = new List<SoundFilter>(Filteres);

                return cloned;
            }
            public YamlTrainSoundData(int Gear1, int Gear2)
            {
                SetCalculatedGearHarmonics(Gear1, Gear2);
            }

            public YamlTrainSoundData() { }
        }

        public class YamlTrainSoundDataManage
        {
            public static YamlTrainSoundData CurrentData { get; set; } = new YamlTrainSoundData(16, 101);

            public static bool SaveYaml(string path)
            {
                try
                {
                    using TextWriter writer = System.IO.File.CreateText(path);
                    var serializer = new Serializer();
                    serializer.Serialize(writer, CurrentData);
                    writer.Close();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public static YamlTrainSoundData LoadYaml(string path)
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

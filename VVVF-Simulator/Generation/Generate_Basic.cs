using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.VvvfStructs;

namespace VVVF_Simulator.Generation
{
    public class Generate_Basic
    {
        /// <summary>
        ///  Calculates one cycle of UVW.
        /// </summary>
        /// <param name="Control"></param>
        /// <param name="Sound"></param>
        /// <param name="Division"> Recommend : 120000 , Brief : 2000 </param>
        /// <param name="Precise"> True for more precise calculation when Freq < 1 </param>
        /// <returns> One cycle of UVW </returns>
        public static WaveValues[] Get_UVW_Cycle(VvvfValues Control, Yaml_VVVF_Sound_Data Sound, double InitialPhase, int Division, bool Precise)
        {
            double _F = Control.get_Sine_Freq();
            double _K = (_F > 0.01 && _F < 1) ? 1 / _F : 1;
            int Count = Precise ? (int)Math.Round(Division * _K) : Division;
            double InvDeltaT = Count * _F;

            Control.set_Sine_Time(0);
            Control.set_Saw_Time(0);

            return Get_UVW(Control, Sound, InitialPhase, InvDeltaT, Count);
        }

        /// <summary>
        /// Calculates WaveForm of UVW in 1 sec.
        /// </summary>
        /// <param name="Control"></param>
        /// <param name="Sound"></param>
        /// <param name="InitialPhase"></param>
        /// <param name="Division"> Recommend : 120000 , Brief : 2000 </param>
        /// <param name="Precise"> True for more precise calculation when Freq < 1</param>
        /// <returns> WaveForm of UVW in 1 sec.</returns>
        public static WaveValues[] Get_UVW_Sec(VvvfValues Control, Yaml_VVVF_Sound_Data Sound, double InitialPhase, int Division, bool Precise)
        {
            double _F = Control.get_Sine_Freq();
            double _K = (_F > 0.01 && _F < 1) ? 1 / _F : 1;
            int Count = Precise ? (int)Math.Round(Division * _K) : Division;
            double InvDeltaT = Count;

            Control.set_Sine_Time(0);
            Control.set_Saw_Time(0);

            return Get_UVW(Control, Sound, InitialPhase, InvDeltaT, Count);
        }

        public static WaveValues[] Get_UVW(VvvfValues Control, Yaml_VVVF_Sound_Data Sound, double InitialPhase, double InvDeltaT, int Count)
        {
            ControlStatus cv = new()
            {
                brake = Control.is_Braking(),
                mascon_on = !Control.is_Mascon_Off(),
                free_run = Control.is_Free_Running(),
                wave_stat = Control.get_Control_Frequency()
            };
            PwmCalculateValues calculated_Values = Yaml_VVVF_Wave.calculate_Yaml(Control, cv, Sound);
            WaveValues[] PWM_Array = new WaveValues[Count + 1];
            for (int i = 0; i <= Count; i++)
            {
                Control.set_Sine_Time(i / InvDeltaT);
                Control.set_Saw_Time(i / InvDeltaT);
                WaveValues value = VVVF_Calculate.calculate_values(Control, calculated_Values, InitialPhase);
                PWM_Array[i] = value;
            }
            return PWM_Array;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.VVVF_Structs;

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
        public static Wave_Values[] Get_UVW_Cycle(VVVF_Values Control, Yaml_VVVF_Sound_Data Sound, double InitialPhase, int Division, bool Precise)
        {
            double _F = Control.get_Sine_Freq();
            double _K = (_F > 0.01 && _F < 1) ? 1 / _F : 1;
            int Count = Precise ? (int)Math.Round(Division * _K) : Division;
            double DeltaT = 1.0 / (Count * _F);

            Control.set_Sine_Time(0);
            Control.set_Saw_Time(0);

            return Get_UVW(Control, Sound, InitialPhase, DeltaT, Count);
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
        public static Wave_Values[] Get_UVW_Sec(VVVF_Values Control, Yaml_VVVF_Sound_Data Sound, double InitialPhase, int Division, bool Precise)
        {
            double _F = Control.get_Sine_Freq();
            double _K = (_F > 0.01 && _F < 1) ? 1 / _F : 1;
            int Count = Precise ? (int)Math.Round(Division * _K) : Division;
            double DeltaT = 1.0 / Count;

            Control.set_Sine_Time(0);
            Control.set_Saw_Time(0);

            return Get_UVW(Control, Sound, InitialPhase, DeltaT, Count);
        }

        private static Wave_Values[] Get_UVW(VVVF_Values Control, Yaml_VVVF_Sound_Data Sound, double InitialPhase, double DeltaT, int Count)
        {
            Control_Values cv = new()
            {
                brake = Control.is_Braking(),
                mascon_on = !Control.is_Mascon_Off(),
                free_run = Control.is_Free_Running(),
                wave_stat = Control.get_Control_Frequency()
            };
            PWM_Calculate_Values calculated_Values = Yaml_VVVF_Wave.calculate_Yaml(Control, cv, Sound);
            Wave_Values[] PWM_Array = new Wave_Values[Count + 1];
            for (int i = 0; i <= Count; i++)
            {
                Control.set_Sine_Time(i * DeltaT);
                Control.set_Saw_Time(i * DeltaT);
                Wave_Values value = VVVF_Calculate.calculate_values(Control, calculated_Values, InitialPhase);
                PWM_Array[i] = value;
            }
            return PWM_Array;
        }
    }
}

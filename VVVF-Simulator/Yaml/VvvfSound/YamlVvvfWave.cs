using static VvvfSimulator.VvvfCalculate;
using static VvvfSimulator.MyMath;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData;
using System;
using System.Collections.Generic;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData.YamlControlData;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData.YamlControlData.YamlFreeRunCondition;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData.YamlMasconData;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData.YamlControlData.YamlControlDataAmplitudeControl;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncParameterCarrierFreqTable;
using static VvvfSimulator.VvvfStructs;
using static VvvfSimulator.VvvfStructs.PulseMode;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterRandom.YamlAsyncParameterRandomValue;

namespace VvvfSimulator.Yaml.VVVFSound
{
    public class YamlVVVFWave
    {
		private static double YamlAmplitudeCalculate(YamlControlDataAmplitude amp_data, double x)
		{
			var amp_param = amp_data.parameter;
			AmplitudeArgument aa = new(amp_param,x);
			double amp = GetAmplitude(amp_data.mode, aa);
			if (amp_param.cut_off_amp > amp) amp = 0;
			if (amp_param.max_amp != -1 && amp_param.max_amp < amp) amp = amp_param.max_amp;
			return amp;
		}

		private static double GetMovingValue(YamlMovingValue moving_val, double current)
		{
			double val = 1000;
			if (moving_val.type == YamlMovingValue.MovingValueType.Proportional)
				val = get_Changing_Value(
					moving_val.start,
					moving_val.start_value,
					moving_val.end,
					moving_val.end_value,
					current
				);
			else if (moving_val.type == YamlMovingValue.MovingValueType.Pow2_Exponential)
				val = (Math.Pow(2, Math.Pow((current - moving_val.start) / (moving_val.end - moving_val.start), moving_val.degree)) - 1) * (moving_val.end_value - moving_val.start_value) + moving_val.start_value;
			else if(moving_val.type == YamlMovingValue.MovingValueType.Inv_Proportional)
            {
				double x = get_Changing_Value(
					moving_val.start,
					1 / moving_val.start_value,
					moving_val.end,
					1 / moving_val.end_value,
					current
				);

				double c = -moving_val.curve_rate;
				double k = moving_val.end_value;
				double l = moving_val.start_value;
				double a = 1 / ((1 / l) - (1 / k)) * (1 / (l - c) - 1 / (k - c));
				double b = 1 / (1 - (1 / l) * k) * (1 / (l - c) - (1 / l) * k / (k - c));

				val = 1 / (a * x + b) + c;
			}
            else if (moving_val.type == YamlMovingValue.MovingValueType.Sine)
			{
				double x = (M_PI_2 - Math.Asin(moving_val.start_value / moving_val.end_value)) / (moving_val.end - moving_val.start) * (current - moving_val.start) + Math.Asin(moving_val.start_value / moving_val.end_value);
				val = Math.Sin(x) * moving_val.end_value;
            }

            return val;

		}

		public static bool IsMatching(VvvfValues control, ControlStatus cv,YamlControlData ysd, bool compare_with_sine)
        {
			Yaml_Free_Run_Condition_Single free_run_data;
			if (cv.mascon_on) free_run_data = ysd.when_freerun.on;
			else free_run_data = ysd.when_freerun.off;

			bool enable_free_run_condition = cv.free_run && ((cv.mascon_on && ysd.enable_on_free_run) || (!cv.mascon_on && ysd.enable_off_free_run));
			bool enable_normal_condition = ysd.enable_normal && !cv.free_run;
			if (!(enable_free_run_condition || enable_normal_condition)) return false;

			bool over_from = ysd.from <= (compare_with_sine ? control.get_Sine_Freq() : cv.wave_stat);
			bool is_sine_from = ysd.rotate_sine_from == -1 || ysd.rotate_sine_from <= control.get_Sine_Freq();
			bool is_sine_below = ysd.rotate_sine_below == -1 || ysd.rotate_sine_below > control.get_Sine_Freq();

			if (!is_sine_from) return false;
			if (!is_sine_below) return false;

			if (!cv.free_run && over_from) return true;
			if (!cv.free_run && !over_from) return false;

			if (free_run_data.skip) return false;

			if (over_from) return true;

			if (free_run_data.stuck_at_here)
			{
				if (control.get_Sine_Freq() > ysd.from) return true;
				return false;
			}

			return false;

		}
		public static PwmCalculateValues CalculateYaml(VvvfValues control , ControlStatus cv, YamlVvvfSoundData yvs)
		{
			PulseMode pulse_mode;
			CarrierFreq carrier_freq = new(0, 0, 0.0005);
			double amplitude = 0;
			double dipolar = -1;

			//
			// mascon off solve
			//
			double mascon_off_check;
			YamlMasconDataOnOff mascon_on_off_check_data;
			if (cv.brake) mascon_on_off_check_data = yvs.mascon_data.braking;
			else mascon_on_off_check_data = yvs.mascon_data.accelerating;
			if (cv.mascon_on)
			{
				mascon_off_check = check_for_mascon_off(cv, control, mascon_on_off_check_data.on.control_freq_go_to);
				control.set_Free_Freq_Change(mascon_on_off_check_data.on.freq_per_sec);
			}
			else
			{
				mascon_off_check = check_for_mascon_off(cv, control, mascon_on_off_check_data.off.control_freq_go_to);
				control.set_Free_Freq_Change(mascon_on_off_check_data.off.freq_per_sec);
			}
			if (mascon_off_check != -1) cv.wave_stat = mascon_off_check;

			//
			// control stat solve
			//
			List<YamlControlData> control_list = new(cv.brake ? yvs.braking_pattern : yvs.accelerate_pattern);
			control_list.Sort((a, b) => b.from.CompareTo(a.from));

			//determine what control data to solve
			int solve = -1;
			for (int x = 0; x < control_list.Count; x++)
			{
				YamlControlData ysd = control_list[x];
				bool match = IsMatching(control, cv, ysd , false);
                if (match)
                {
					solve = x;
					break;
                }

			}

			if (solve == -1)
			{
                if (cv.free_run)
                {
                    if (!cv.mascon_on)
                    {
						control.set_Control_Frequency(0);
						return new PwmCalculateValues() { none = true };
					}
					else
					{
						control.set_Control_Frequency(control.get_Sine_Freq());
						cv.wave_stat = control.get_Sine_Freq();
						return new PwmCalculateValues() { none = true };
					}
				}
				else
					return new PwmCalculateValues() { none = true };
			}

			//
			// min sine freq solve
			//
			double minimum_sine_freq, original_wave_stat = cv.wave_stat;
			if (cv.brake) minimum_sine_freq = yvs.min_freq.braking;
			else minimum_sine_freq = yvs.min_freq.accelerate;
			if (0 < cv.wave_stat && cv.wave_stat < minimum_sine_freq && !cv.free_run) cv.wave_stat = minimum_sine_freq;

			YamlControlData solve_data = control_list[solve];
			pulse_mode = solve_data.pulse_Mode;

			if (pulse_mode.pulse_name == PulseModeNames.Async)
			{
				var async_data = solve_data.async_data;

				//
				//carrier freq solve
				//
				var carrier_data = async_data.carrier_wave_data;
				var carrier_freq_mode = carrier_data.carrier_mode;
				double carrier_freq_val = 100;
				if (carrier_freq_mode == YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncCarrierMode.Const)
					carrier_freq_val = carrier_data.const_value;
				else if (carrier_freq_mode == YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncCarrierMode.Moving)
					carrier_freq_val = GetMovingValue(carrier_data.moving_value, original_wave_stat);
				else if (carrier_freq_mode == YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncCarrierMode.Table)
				{
					var table_data = carrier_data.carrier_table_value;

					//Solve from high.
					List<YamlAsyncParameterCarrierFreqTableValue> async_carrier_freq_table = new(table_data.carrier_freq_table);
					async_carrier_freq_table.Sort((a, b) => Math.Sign(b.from - a.from));
	
					for(int i = 0; i < async_carrier_freq_table.Count; i++)
                    {
						var carrier = async_carrier_freq_table[i];
						bool condition_1 = carrier.free_run_stuck_here && (control.get_Sine_Freq() < carrier.from) && cv.free_run;
						bool condition_2 = original_wave_stat > carrier.from;
						if (!condition_1 && !condition_2) continue;

						carrier_freq_val = carrier.carrier_freq;
						break;

					}
					
				}
				else if(carrier_freq_mode == YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncCarrierMode.Vibrato)
				{
					var vibrato_data = carrier_data.vibrato_value;

					double highest, lowest;
					if (vibrato_data.highest.mode == YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncParameterCarrierFreqVibrato.YamlAsyncParameterVibratoValue.YamlAsyncParameterVibratoMode.Const)
						highest = vibrato_data.highest.const_value;
					else
					{
						var moving_val = vibrato_data.highest.moving_value;
						highest = GetMovingValue(moving_val, original_wave_stat);
					}

					if (vibrato_data.lowest.mode == YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncParameterCarrierFreqVibrato.YamlAsyncParameterVibratoValue.YamlAsyncParameterVibratoMode.Const)
						lowest = vibrato_data.lowest.const_value;
					else
					{
						var moving_val = vibrato_data.lowest.moving_value;
						lowest = GetMovingValue(moving_val, original_wave_stat);
					}

					double interval;
					if (vibrato_data.interval.mode == YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncParameterCarrierFreqVibrato.YamlAsyncParameterVibratoValue.YamlAsyncParameterVibratoMode.Const)
						interval = vibrato_data.interval.const_value;
					else
						interval = GetMovingValue(vibrato_data.interval.moving_value, original_wave_stat);

					carrier_freq_val = GetVibratoFrequency(lowest, highest, interval, vibrato_data.continuous, control);
				}

				//
				//random range solve
				//
				double random_range = 0, random_interval = 0;
				if (async_data.random_data.random_range.value_mode == Yaml_Async_Parameter_Random_Value_Mode.Const) random_range = async_data.random_data.random_range.const_value;
				else random_range = GetMovingValue(async_data.random_data.random_range.moving_value, original_wave_stat);

				if (async_data.random_data.random_interval.value_mode == Yaml_Async_Parameter_Random_Value_Mode.Const) random_interval = async_data.random_data.random_interval.const_value;
				else random_interval = GetMovingValue(async_data.random_data.random_interval.moving_value, original_wave_stat);

				carrier_freq = new CarrierFreq(carrier_freq_val, random_range, random_interval);

				//
				// dipolar solve
				//
				var dipolar_data = async_data.dipoar_data;
				if (dipolar_data.value_mode == YamlAsyncParameter.YamlAsyncParameterDipolar.YamlAsyncParameterDipolarMode.Const)
					dipolar = dipolar_data.const_value;
				else
				{
					var moving_val = dipolar_data.moving_value;
					dipolar = GetMovingValue(moving_val, original_wave_stat);
				}



			}

			amplitude = YamlAmplitudeCalculate(solve_data.amplitude_control.default_data, cv.wave_stat);

			if (cv.free_run && solve_data.amplitude_control.free_run_data != null)
			{
				var free_run_data = solve_data.amplitude_control.free_run_data;
				var free_run_amp_data = (cv.mascon_on) ? free_run_data.mascon_on : free_run_data.mascon_off;
				var free_run_amp_param = free_run_amp_data.parameter;

				double max_control_freq = cv.mascon_on ? mascon_on_off_check_data.on.control_freq_go_to : mascon_on_off_check_data.off.control_freq_go_to;

				double target_freq = free_run_amp_param.end_freq;
				if (free_run_amp_param.end_freq == -1)
				{
					if (solve_data.amplitude_control.default_data.parameter.disable_range_limit) target_freq = control.get_Sine_Freq();
					else
					{
                        target_freq = (control.get_Sine_Freq() > max_control_freq) ? max_control_freq : control.get_Sine_Freq();
						target_freq = (target_freq > solve_data.amplitude_control.default_data.parameter.end_freq) ? solve_data.amplitude_control.default_data.parameter.end_freq : target_freq;
                    }
                    
                }
					

				double target_amp = free_run_amp_param.end_amp;
				if (free_run_amp_param.end_amp == -1)
					target_amp = YamlAmplitudeCalculate(solve_data.amplitude_control.default_data, control.get_Sine_Freq());

				double start_amp = free_run_amp_param.start_amp;
				if(start_amp == -1) 
					start_amp = YamlAmplitudeCalculate(solve_data.amplitude_control.default_data, control.get_Sine_Freq());


				AmplitudeArgument aa = new()
				{
					min_freq = free_run_amp_param.start_freq,
					min_amp = start_amp,
					max_freq = target_freq,
					max_amp = target_amp,

					current = cv.wave_stat,
					disable_range_limit = free_run_amp_param.disable_range_limit,
					polynomial = free_run_amp_param.polynomial,
					change_const = free_run_amp_param.curve_change_rate
				};
				
				amplitude = GetAmplitude(free_run_amp_data.mode, aa);

				if (free_run_amp_param.cut_off_amp > amplitude) amplitude = 0;
				if (free_run_amp_param.max_amp != -1 && amplitude > free_run_amp_param.max_amp) amplitude = free_run_amp_param.max_amp;
				if (!cv.mascon_on && amplitude == 0) control.set_Control_Frequency(0);
			}

			if (cv.wave_stat == 0) return new PwmCalculateValues() { none = true };
			if (amplitude == 0) return new PwmCalculateValues() { none = true };

			PwmCalculateValues values = new()
			{
				none = false,
				carrier_freq = carrier_freq,
				pulse_mode = pulse_mode,
				level = yvs.level,
				dipolar = dipolar,

				min_sine_freq = minimum_sine_freq,
				amplitude = amplitude,
			};
			return values;
			
		}
	}
}

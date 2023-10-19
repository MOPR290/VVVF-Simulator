using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VvvfSimulator.VVVF_Window.Control_Settings.Async;
using VvvfSimulator.Yaml.VVVFSound;
using static VvvfSimulator.VvvfCalculate;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncParameterCarrierFreqTable;

namespace VvvfSimulator.Generation.Pi3Generator
{
    public class Pi3Generate
    {

		public class Pi3Compiler
		{
			public int indent { get; set; } = 0;
			public string code { get; set; } = "";

			public void AddIndent() { indent++; }
			public void DecrementIndent() {  indent--; }

			public void WriteCode(string code) { this.code += code; }
            public void WriteLineCode(string code) {
                WriteIndent();
                this.code += code + "\r\n"; 
            }
			public void WriteIndent()
			{
				for(int i = 0; i < indent; i++) { this.code += "	"; }
			}

            public string GetCode() { return this.code;}
		}
       
        private static void _WriteWaveStatChange(
            Pi3Compiler compiler,
            YamlVvvfSoundData.YamlMasconData.YamlMasconDataOnOff yaml,
            double min_freq
        )
        {
            compiler.WriteLineCode("pwm->min_freq = " + min_freq + ";");
            compiler.WriteLineCode("double _wave_stat = status->wave_stat;");
            compiler.WriteLineCode("_wave_stat = _wave_stat < pwm->min_freq ? pwm->min_freq : _wave_stat;");

            compiler.WriteLineCode("if (status->mascon_off)");
            compiler.WriteLineCode("{");
            compiler.AddIndent();

            compiler.WriteLineCode("status->free_freq_change = " + yaml.off.freq_per_sec + ";");
            compiler.WriteLineCode("if (status->wave_stat > " + yaml.off.control_freq_go_to + ")");
            compiler.AddIndent();
            compiler.WriteLineCode("status->wave_stat = " + yaml.off.control_freq_go_to + ";");
            compiler.DecrementIndent();

            compiler.DecrementIndent();
            compiler.WriteLineCode("}");
            compiler.WriteLineCode("if (status->free_run && !status->mascon_off)");
            compiler.WriteLineCode("{");
            compiler.AddIndent();

            compiler.WriteLineCode("status->free_freq_change = " + yaml.on.freq_per_sec + ";");
            compiler.WriteLineCode("if (status->wave_stat >= " + yaml.on.control_freq_go_to + ")");
            compiler.AddIndent();
            compiler.WriteLineCode("status->wave_stat = status->sin_angle_freq * M_1_2PI;");
            compiler.DecrementIndent();

            compiler.DecrementIndent();
            compiler.WriteLineCode("}");
        }

        private static void _WriteWavePatterns(
            Pi3Compiler compiler,
            List<YamlVvvfSoundData.YamlControlData> list
        )
        {
            List<YamlControlData> control_list = new(list);
            control_list.Sort((a, b) => b.from.CompareTo(a.from));

            for (int i = 0; control_list.Count > i;i++)
            {
                YamlControlData data = control_list[i];
                List<string> _if = new();

                if(!data.enable_normal)
                    _if.Add("status->free_run");
                if (!data.enable_off_free_run)
                    _if.Add("!(status->free_run && status->mascon_off)");
                if (!data.enable_on_free_run)
                    _if.Add("!(status->free_run && !status->mascon_off)");

                {
                    string _condition = data.from + " <= _wave_stat";

                    if (data.when_freerun.on.stuck_at_here && data.when_freerun.off.stuck_at_here)
                        _condition += " || " + "(status->free_run && status->sin_angle_freq > " + data.from + " * M_2PI)";
                    else
                    {
                        if (data.when_freerun.on.stuck_at_here) _condition += "(!status->mascon_off && status->free_run && status->sin_angle_freq > " + data.from + " * M_2PI)";
                        if (data.when_freerun.off.stuck_at_here) _condition += "(status->mascon_off && status->free_run && status->sin_angle_freq > " + data.from + " * M_2PI)";
                    }

                    _if.Add(_condition);
                }
                

                if (data.when_freerun.on.skip && data.when_freerun.off.skip)
                    _if.Add("!status->free_run");
                else {
                    if (data.when_freerun.on.skip) _if.Add("!(status->free_run && !status->mascon_off)");
                    if (data.when_freerun.off.skip) _if.Add("!(status->free_run && status->mascon_off)");
                }
                if (data.rotate_sine_below != -1) _if.Add("status->sin_angle_freq <" + data.rotate_sine_below + " * M_2PI");
                if (data.rotate_sine_from != -1) _if.Add("status->sin_angle_freq > " + data.rotate_sine_from + " * M_2PI");

                string _s = (i == 0 ? "if" : "else if") + "(";
                for(int x = 0; x < _if.Count; x++)
                {
                    _s += (x == 0 ? "" : " && ") + "(" + _if[x] + ")";
                }
                _s += ")";
                compiler.WriteLineCode(_s);
                compiler.WriteLineCode("{");
                compiler.AddIndent();

                compiler.WriteLineCode("pwm->pulse_mode = " + data.pulse_Mode.pulse_name.ToString() + ";");

                {
                    YamlControlData.YamlControlDataAmplitudeControl amplitude = data.amplitude_control;
                    static void _WriteAmplitudeControl(
                        Pi3Compiler compiler,
                        YamlControlData.YamlControlDataAmplitudeControl.YamlControlDataAmplitude? _default,
                        YamlControlData.YamlControlDataAmplitudeControl.YamlControlDataAmplitude _target,
                        bool refer_freq_sin
                    )
                    {
                        
                        if (_default == null) // This section should have _target as _default
                        {
                            compiler.WriteLineCode("double _amp = 0;");
                            compiler.WriteLineCode("double _c = " + (!refer_freq_sin ? "_wave_stat" : "status->sin_angle_freq * M_1_2PI") + ";");

                            compiler.WriteLineCode("{"); compiler.AddIndent();

                            YamlControlData.YamlControlDataAmplitudeControl.YamlControlDataAmplitude.YamlControlDataAmplitudeParameter _t = _target.parameter;
                            if (!_t.disable_range_limit)
                            {
                                compiler.WriteLineCode("if (_c < " + _t.start_freq + ") _c = " + _t.start_freq + ";");
                                compiler.WriteLineCode("if (_c > " + _t.end_freq + ") _c = " + _t.end_freq + ";");
                            }

                            if (_target.mode == AmplitudeMode.Linear)
                            {
                                double _a = (_t.end_amp - _t.start_amp) / (_t.end_freq - _t.start_freq);
                                compiler.WriteLineCode("_amp = " + _a + " * _c + " + (-_a * _t.start_freq + _t.start_amp) + ";");
                            }
                            else if (_target.mode == AmplitudeMode.Wide_3_Pulse)
                            {
                                double _a = (_t.end_amp - _t.start_amp) / (_t.end_freq - _t.start_freq);
                                double _b = -_a * _t.start_freq + _t.start_amp;
                                compiler.WriteLineCode("_amp = " + (0.2 * _a) + " * _c + " + (0.2 * _b + 0.8) + ";");
                            }
                            else if (_target.mode == AmplitudeMode.Inv_Proportional)
                            {
                                double _a = (1.0 / _t.end_amp - 1.0 / _t.start_amp) / (_t.end_freq - _t.start_freq);
                                double _b = -_a * _t.start_freq + (1.0 / _t.start_amp);
                                compiler.WriteLineCode("double _x = " + _a + " * _c + " + _b + ";");

                                double c = -_t.curve_change_rate;
                                double k = _t.end_amp;
                                double l = _t.start_amp;
                                double a = 1 / ((1 / l) - (1 / k)) * (1 / (l - c) - 1 / (k - c));
                                double b = 1 / (1 - 1 / l * k) * (1 / (l - c) - 1 / l * k / (k - c));
                                compiler.WriteLineCode("_amp = 1 / (" + a + " * _x + " + b + ") +" + c + " ;");
                            }
                            else
                            {
                                compiler.WriteLineCode(" // @ 20231019180255");
                            }

                            if (_t.cut_off_amp >= 0)
                                compiler.WriteLineCode("if (" + _t.cut_off_amp + " > _amp) _amp = 0;");
                            if (_t.max_amp != -1)
                                compiler.WriteLineCode("if (" + _t.max_amp + " < _amp) _amp = " + _t.max_amp + ";");

                            compiler.DecrementIndent(); compiler.WriteLineCode("}");
                        }
                        else
                        {

                            _WriteAmplitudeControl(compiler, null, _default, true);

                            YamlControlData.YamlControlDataAmplitudeControl.YamlControlDataAmplitude.YamlControlDataAmplitudeParameter _t = _target.parameter;
                            YamlControlData.YamlControlDataAmplitudeControl.YamlControlDataAmplitude.YamlControlDataAmplitudeParameter _d = _default.parameter;


                            compiler.WriteLineCode("{"); compiler.AddIndent();

                            compiler.WriteLineCode("_c = _wave_stat;");
                            if (!_t.disable_range_limit)
                            {
                                compiler.WriteLineCode("if (_c < " + (_t.start_freq == -1 ? _d.start_freq : _t.start_freq) + ") _c = " + (_t.start_freq == -1 ? _d.start_freq : _t.start_freq) + ";");
                                compiler.WriteLineCode("if (_c > " + (_t.end_freq == -1 ? _d.end_freq : _t.end_freq) + ") _c = " + (_t.end_freq == -1 ? _d.end_freq : _t.end_freq) + ";");
                            }

                            if (_target.mode == AmplitudeMode.Linear)
                            {
                                string _a = "double _a = (" + (_t.end_amp == -1 ? "_amp" : _t.end_amp.ToString()) + " - " + (_t.start_amp == -1 ? "0" : _t.start_amp.ToString()) + ") / (" + (_t.end_freq == -1 ? "status->sin_angle_freq * M_1_2PI" : _t.end_freq.ToString()) + " - " + (_t.start_freq == -1 ? "0" : _t.start_freq.ToString()) + ");";
                                string _b = "double _b = -_a * " + (_t.start_freq == -1 ? "0" : _t.start_freq.ToString()) + " + " + (_t.start_amp == -1 ? "0" : _t.start_amp.ToString()) + ";";
                                compiler.WriteLineCode(_a);
                                compiler.WriteLineCode(_b);
                                compiler.WriteLineCode("_amp = _a * _c + _b;");
                            }
                            else if (_target.mode == AmplitudeMode.Wide_3_Pulse)
                            {
                                string _a = "double _a = (" + (_t.end_amp == -1 ? "_amp" : _t.end_amp.ToString()) + " - " + (_t.start_amp == -1 ? "0" : _t.start_amp.ToString()) + ") / (" + (_t.end_freq == -1 ? "status->sin_angle_freq * M_1_2PI" : _t.end_freq.ToString()) + " - " + (_t.start_freq == -1 ? "0" : _t.start_freq.ToString()) + ");";
                                string _b = "double _b = -_a * " + (_t.start_freq == -1 ? "0" : _t.start_freq.ToString()) + " + " + (_t.start_amp == -1 ? "0" : _t.start_amp.ToString()) + ";";
                                compiler.WriteLineCode(_a);
                                compiler.WriteLineCode(_b);
                                compiler.WriteLineCode("_amp = 0.2 * (_a * _c + _b) + 0.8;");
                            }
                            else if (_target.mode == AmplitudeMode.Inv_Proportional)
                            {
                                //double _a = (1.0 / (_t.end_amp == -1 ? _d.end_amp : _t.end_amp) - 1.0 / (_t.start_amp == -1 ? _d.start_amp : _t.start_amp)) / ((_t.end_freq == -1 ? _d.end_freq : _t.end_freq) - (_t.start_freq == -1 ? _d.start_freq : _t.start_freq));
                                //double _b = -_a * (_t.start_freq == -1 ? _d.start_freq : _t.start_freq) + (1.0 / (_t.start_amp == -1 ? _d.start_amp : _t.start_amp));
                                //compiler.WriteLineCode("double _x = " + _a + " * _c + " + _b + ";");

                                string _a = "double _a = (1.0 / " + (_t.end_amp == -1 ? "_amp" : _t.end_amp.ToString()) + " - 1.0 / " + (_t.start_amp == -1 ? "1" : _t.start_amp.ToString()) + ") / (" + (_t.end_freq == -1 ? "status->sin_angle_freq * M_1_2PI" : _t.end_freq.ToString()) + " - " + (_t.start_freq == -1 ? "0" : _t.start_freq.ToString()) + ");";                                
                                string _b = "double _b = -_a * " + (_t.start_freq == -1 ? "0" : _t.start_freq.ToString()) + " + 1.0 / " + (_t.start_amp == -1 ? "1" : _t.start_amp.ToString()) + ";";
                                compiler.WriteLineCode(_a);
                                compiler.WriteLineCode(_b);
                                compiler.WriteLineCode("double _x = _a * _c + _b;");

                                compiler.WriteLineCode("double c = -" + (_t.curve_change_rate == -1 ? _d.curve_change_rate : _t.curve_change_rate) + ";");
                                compiler.WriteLineCode("double k = " + (_t.end_amp == -1 ? "_amp" : _t.end_amp) + ";");
                                compiler.WriteLineCode("double l = " + (_t.start_amp == -1 ? "1" : _t.start_amp) + ";");
                                compiler.WriteLineCode("double a = 1 / ((1 / l) - (1 / k)) * (1 / (l - c) - 1 / (k - c));");
                                compiler.WriteLineCode("double b = 1 / (1 - (1 / l) * k) * (1 / (l - c) - (1 / l) * k / (k - c));");
                                compiler.WriteLineCode("_amp = 1.0 / (a * _x + b) + c;");
                            }
                            else
                            {
                                compiler.WriteLineCode(" // @ 20231018213430");
                            }

                            if ((_t.cut_off_amp == -1 ? _d.cut_off_amp : _t.cut_off_amp) >= 0)
                                compiler.WriteLineCode("if (" + (_t.cut_off_amp == -1 ? _d.cut_off_amp : _t.cut_off_amp) + " > _amp) _amp = 0;");
                            if ((_t.max_amp == -1 ? _d.max_amp : _t.max_amp) != -1)
                                compiler.WriteLineCode("if (" + (_t.max_amp == -1 ? _d.max_amp : _t.max_amp) + " < _amp) _amp = " + (_t.max_amp == -1 ? _d.max_amp : _t.max_amp) + ";");

                            compiler.DecrementIndent(); compiler.WriteLineCode("}");

                        }
                    }

                    compiler.WriteLineCode("if (!status->free_run) {"); compiler.AddIndent();
                    _WriteAmplitudeControl(compiler, null, amplitude.default_data, false);
                    compiler.WriteLineCode("pwm->amplitude = _amp;");
                    compiler.DecrementIndent(); compiler.WriteLineCode("}");

                    compiler.WriteLineCode("if (status->free_run && !status->mascon_off) {"); compiler.AddIndent();
                    _WriteAmplitudeControl(compiler, amplitude.default_data, amplitude.free_run_data.mascon_on, false);
                    compiler.WriteLineCode("pwm->amplitude = _amp;");
                    compiler.DecrementIndent(); compiler.WriteLineCode("}");

                    compiler.WriteLineCode("if (status->free_run && status->mascon_off) {"); compiler.AddIndent();
                    _WriteAmplitudeControl(compiler, amplitude.default_data, amplitude.free_run_data.mascon_off, false);
                    compiler.WriteLineCode("pwm->amplitude = _amp;");
                    compiler.DecrementIndent(); compiler.WriteLineCode("}");


                }

                if (data.pulse_Mode.pulse_name == VvvfStructs.PulseMode.PulseModeNames.Async)
                {
                    {
                        YamlControlData.YamlAsyncParameter.YamlAsyncParameterCarrierFreq async = data.async_data.carrier_wave_data;
                        if (async.carrier_mode == YamlControlData.YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncCarrierMode.Const)
                        {
                            compiler.WriteLineCode("pwm->carrier_freq.base_freq = " + async.const_value + ";");
                        }
                        else if (async.carrier_mode == YamlControlData.YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncCarrierMode.Moving)
                        {
                            YamlControlData.YamlMovingValue moving = async.moving_value;
                            if (moving.type == YamlControlData.YamlMovingValue.MovingValueType.Proportional)
                            {
                                compiler.WriteLineCode("{"); compiler.AddIndent();
                                double _a = (moving.end_value - moving.start_value) / (moving.end - moving.start);
                                double _b = -_a * moving.start + moving.start_value;
                                compiler.WriteLineCode("pwm->carrier_freq.base_freq = " + _a + " * _wave_stat + " + _b + ";");
                                compiler.DecrementIndent(); compiler.WriteLineCode("}");
                            }
                            else
                            {
                                compiler.WriteLineCode(" // @ 20231018205819");
                            }

                        }else if (async.carrier_mode == YamlControlData.YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncCarrierMode.Table)
                        {
                            List<YamlAsyncParameterCarrierFreqTableValue> _list = new(async.carrier_table_value.carrier_freq_table);
                            _list.Sort((a, b) => b.from.CompareTo(a.from));
                            for (int x = 0; x < _list.Count; x++)
                            {
                                YamlAsyncParameterCarrierFreqTableValue val = _list[x];
                                string _con = (x == 0 ? "if" : "else if") + "(";
                                _con += "_wave_stat >= " + val.from;
                                if(val.free_run_stuck_here) _con += " || " + "(status->free_run && status->sin_angle_freq > " + val.from + " * M_2PI)";
                                _con += ")";
                                compiler.WriteLineCode(_con);
                                compiler.AddIndent(); compiler.WriteLineCode("pwm->carrier_freq.base_freq = " + val.carrier_freq + ";");
                                compiler.DecrementIndent();

                            }

                        }
                        else
                        {
                            compiler.WriteLineCode(" // @ 20231018205827");
                        }
                    }

                    {
                        YamlControlData.YamlAsyncParameter.YamlAsyncParameterRandom.YamlAsyncParameterRandomValue random_interval = data.async_data.random_data.random_interval;
                        if (random_interval.value_mode == YamlControlData.YamlAsyncParameter.YamlAsyncParameterRandom.YamlAsyncParameterRandomValue.Yaml_Async_Parameter_Random_Value_Mode.Const)
                        {
                            compiler.WriteLineCode("pwm->carrier_freq.interval = " + random_interval.const_value + ";");
                        }
                        else if (random_interval.value_mode == YamlControlData.YamlAsyncParameter.YamlAsyncParameterRandom.YamlAsyncParameterRandomValue.Yaml_Async_Parameter_Random_Value_Mode.Moving)
                        {
                            YamlControlData.YamlMovingValue moving = random_interval.moving_value;
                            if (moving.type == YamlControlData.YamlMovingValue.MovingValueType.Proportional)
                            {
                                compiler.WriteLineCode("{"); compiler.AddIndent();
                                double _a = (moving.end_value - moving.start_value) / (moving.end - moving.start);
                                double _b = -_a * moving.start + moving.start_value;
                                compiler.WriteLineCode("pwm->carrier_freq.interval = " + _a + " * _wave_stat + " + _b + ";");
                                compiler.DecrementIndent(); compiler.WriteLineCode("}");
                            }
                            else
                            {
                                compiler.WriteLineCode(" // @ 20231018210032 ");
                            }

                        }
                        else
                        {
                            compiler.WriteLineCode(" // @ 20231018210021 ");
                        }
                    }

                    {
                        YamlControlData.YamlAsyncParameter.YamlAsyncParameterRandom.YamlAsyncParameterRandomValue random_range = data.async_data.random_data.random_range;
                        if (random_range.value_mode == YamlControlData.YamlAsyncParameter.YamlAsyncParameterRandom.YamlAsyncParameterRandomValue.Yaml_Async_Parameter_Random_Value_Mode.Const)
                        {
                            compiler.WriteLineCode("pwm->carrier_freq.range = " + random_range.const_value + ";");
                        }
                        else if (random_range.value_mode == YamlControlData.YamlAsyncParameter.YamlAsyncParameterRandom.YamlAsyncParameterRandomValue.Yaml_Async_Parameter_Random_Value_Mode.Moving)
                        {
                            YamlControlData.YamlMovingValue moving = random_range.moving_value;
                            if (moving.type == YamlControlData.YamlMovingValue.MovingValueType.Proportional)
                            {
                                compiler.WriteLineCode("{"); compiler.AddIndent();
                                double _a = (moving.end_value - moving.start_value) / (moving.end - moving.start);
                                double _b = -_a * moving.start + moving.start_value;
                                compiler.WriteLineCode("pwm->carrier_freq.range = " + _a + " * _wave_stat + " + _b + ";");
                                compiler.DecrementIndent(); compiler.WriteLineCode("}");
                            }
                            else
                            {
                                compiler.WriteLineCode(" // @ 20231018210113 ");
                            }

                        }
                        else
                        {
                            compiler.WriteLineCode(" // @ 20231018210117 ");
                        }
                    }

                    {
                        YamlControlData.YamlAsyncParameter.YamlAsyncParameterDipolar dipolar = data.async_data.dipoar_data;
                        if (dipolar.value_mode == YamlControlData.YamlAsyncParameter.YamlAsyncParameterDipolar.YamlAsyncParameterDipolarMode.Const)
                        {
                            compiler.WriteLineCode("pwm->dipolar = " + dipolar.const_value + ";");
                        }
                        else if (dipolar.value_mode == YamlControlData.YamlAsyncParameter.YamlAsyncParameterDipolar.YamlAsyncParameterDipolarMode.Moving)
                        {
                            YamlControlData.YamlMovingValue moving = dipolar.moving_value;
                            if (moving.type == YamlControlData.YamlMovingValue.MovingValueType.Proportional)
                            {
                                compiler.WriteLineCode("{"); compiler.AddIndent();
                                double _a = (moving.end_value - moving.start_value) / (moving.end - moving.start);
                                double _b = -_a * moving.start + moving.start_value;
                                compiler.WriteLineCode("pwm->dipolar = " + _a + " * _wave_stat + " + _b + ";");
                                compiler.DecrementIndent(); compiler.WriteLineCode("}");
                            }
                            else
                            {
                                compiler.WriteLineCode(" // @ 20231018210510 ");
                            }

                        }
                        else
                        {
                            compiler.WriteLineCode(" // @ 20231018210517 ");
                        }
                    }
                }

                compiler.DecrementIndent();
                compiler.WriteLineCode("}");
                


            }

            compiler.WriteLineCode("else");
            compiler.WriteLineCode("{");
            compiler.AddIndent();
            compiler.WriteLineCode("pwm->none = true;");
            compiler.DecrementIndent();
            compiler.WriteLineCode("}");
        }

        public static string GenerateC(YamlVvvfSoundData vfsoundData, string functionName)
        {
            Pi3Compiler compiler = new Pi3Compiler();
            compiler.WriteLineCode("void calculate" + functionName + "(VvvfValues *status, PwmCalculateValues *pwm)");
            compiler.WriteLineCode("{");
            compiler.AddIndent();
            List<String> lines = new List<String>()
            {
                "CarrierFreq carrier_freq = {0, 0, 0};",
                "pwm->level = " + vfsoundData.level.ToString() + ";",
                "pwm->dipolar = -1;",
                "pwm->min_freq = 0;",
                "pwm->amplitude = 0;",
                "pwm->none = false;",
                "pwm->pulse_mode = P_1;",
                "pwm->carrier_freq = carrier_freq;"
            };
            for(int i = 0; i < lines.Count; i++)
            {
                compiler.WriteLineCode(lines[i]);
            }

            compiler.WriteLineCode("if (status->brake)");
            compiler.WriteLineCode("{");
            compiler.AddIndent();

            _WriteWaveStatChange(compiler, vfsoundData.mascon_data.braking, vfsoundData.min_freq.braking);
            _WriteWavePatterns(compiler, vfsoundData.braking_pattern);

            compiler.DecrementIndent();
            compiler.WriteLineCode("}");
            compiler.WriteLineCode("else");
            compiler.WriteLineCode("{");
            compiler.AddIndent();

            _WriteWaveStatChange(compiler, vfsoundData.mascon_data.accelerating, vfsoundData.min_freq.accelerate);
            _WriteWavePatterns(compiler, vfsoundData.accelerate_pattern);

            compiler.DecrementIndent();
            compiler.WriteLineCode("}");

            compiler.WriteLineCode("if (status->wave_stat == 0) pwm->none = true;");

            compiler.DecrementIndent();
            compiler.WriteLineCode("}");

            return compiler.GetCode();
        }

    }
}

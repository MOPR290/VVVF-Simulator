using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Media;
using System.Windows.Media;
using System.Threading.Tasks;
using YamlDotNet.Core;
using VvvfSimulator.Yaml.VVVFSound;
using VvvfSimulator.Generation.Pi3Generator;
using VvvfSimulator.GUI.Create.Waveform;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.GUI.Mascon;
using VvvfSimulator.GUI.TrainAudio;
using VvvfSimulator.GUI.TaskViewer;
using VvvfSimulator.GUI.Simulator.RealTime;
using VvvfSimulator.GUI.Simulator.RealTime.Display;
using VvvfSimulator.GUI.Simulator.RealTime.Setting_Window;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Yaml.VVVFSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;
using static VvvfSimulator.Generation.Audio.GenerateRealTimeCommon;
using static VvvfSimulator.Yaml.TrainAudio_Setting.YamlTrainSoundAnalyze;
using static VvvfSimulator.Generation.GenerateCommon.GenerationBasicParameter;

namespace VvvfSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ViewData BindingData = new();
        public class ViewData : ViewModelBase
        {
            private bool _Blocked = false;
            public bool Blocked
            {
                get
                {
                    return _Blocked;
                }
                set
                {
                    _Blocked = value;
                    RaisePropertyChanged(nameof(Blocked));
                }
            }
        };
        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MainWindow()
        {
            DataContext = BindingData;
            InitializeComponent();

        }

        

        private void SettingButtonClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string name = button.Name;
            if (name.Equals("settings_level"))
                setting_window.Navigate(new Uri("GUI/Create/Settings/level_setting.xaml", UriKind.Relative));
            else if(name.Equals("settings_minimum"))
                setting_window.Navigate(new Uri("GUI/Create/Settings/minimum_freq_setting.xaml", UriKind.Relative));
            else if(name.Equals("settings_mascon"))
                setting_window.Navigate(new Uri("GUI/Create/Settings/jerk_setting.xaml", UriKind.Relative));
        }

        private void SettingEditClick(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            object? tag = btn.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;
            String[] command = tag_str.Split("_");

            var list_view = command[0].Equals("accelerate") ? accelerate_settings : brake_settings;
            var settings = command[0].Equals("accelerate") ? YamlVvvfManage.CurrentData.accelerate_pattern : YamlVvvfManage.CurrentData.braking_pattern;

            if (command[1].Equals("remove"))
            {
                if(list_view.SelectedIndex >= 0)
                    settings.RemoveAt(list_view.SelectedIndex);
            }
                
            else if (command[1].Equals("add"))
                settings.Add(new YamlControlData());
            else if (command[1].Equals("reset"))
                settings.Clear();

            list_view.Items.Refresh();
        }
        private void SettingsLoad(object sender, RoutedEventArgs e)
        {
            ListView btn = (ListView)sender;
            object? tag = btn.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;

            if (tag.Equals("accelerate"))
            {
                UpdateControlList();
                AccelerateSelectedShow();
            }
            else
            {
                UpdateControlList();
                BrakeSelectedShow();
            }
        }
        private void SettingsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView btn = (ListView)sender;
            object? tag = btn.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;


            if(tag.Equals("accelerate"))
                AccelerateSelectedShow();
            else
                BrakeSelectedShow();
        }
       
        private void AccelerateSelectedShow()
        {
            int selected = accelerate_settings.SelectedIndex;
            if (selected < 0) return;

            YamlVvvfSoundData ysd = YamlVvvfManage.CurrentData;
            var selected_data = ysd.accelerate_pattern[selected];
            setting_window.Navigate(new Control_Setting_Page_Common(selected_data, this, ysd.level));

        }
        private void BrakeSelectedShow()
        {
            int selected = brake_settings.SelectedIndex;
            if (selected < 0) return;

            YamlVvvfSoundData ysd = YamlVvvfManage.CurrentData;
            var selected_data = ysd.braking_pattern[selected];
            setting_window.Navigate(new Control_Setting_Page_Common(selected_data, this, ysd.level));
        }

        public void UpdateControlList()
        {
            accelerate_settings.ItemsSource = YamlVvvfManage.CurrentData.accelerate_pattern;
            brake_settings.ItemsSource = YamlVvvfManage.CurrentData.braking_pattern;
            accelerate_settings.Items.Refresh();
            brake_settings.Items.Refresh();
        }
        public void UpdateContentSelected()
        {
            if (setting_tabs.SelectedIndex == 1)
            {
                AccelerateSelectedShow();
            }
            else if (setting_tabs.SelectedIndex == 2)
            {
                BrakeSelectedShow();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            Object? tag = mi.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;
            String[] command = tag_str.Split(".");
            if (command[0].Equals("brake"))
            {
                if (command[1].Equals("sort"))
                {
                    YamlVvvfManage.CurrentData.braking_pattern.Sort((a, b) => Math.Sign(b.from - a.from));
                    UpdateControlList();
                    BrakeSelectedShow();
                }
                else if (command[1].Equals("copy"))
                {
                    int selected = brake_settings.SelectedIndex;
                    if (selected < 0) return;

                    YamlVvvfSoundData ysd = YamlVvvfManage.CurrentData;
                    var selected_data = ysd.braking_pattern[selected];
                    YamlVvvfManage.CurrentData.braking_pattern.Add(selected_data.Clone());
                    UpdateControlList();
                    BrakeSelectedShow();
                }
            }
            else if (command[0].Equals("accelerate"))
            {
                if (command[1].Equals("sort"))
                {
                    YamlVvvfManage.CurrentData.accelerate_pattern.Sort((a, b) => Math.Sign(b.from - a.from));
                    UpdateControlList();
                    AccelerateSelectedShow();
                }
                else if (command[1].Equals("copy"))
                {
                    int selected = accelerate_settings.SelectedIndex;
                    if (selected < 0) return;

                    YamlVvvfSoundData ysd = YamlVvvfManage.CurrentData;
                    YamlControlData selected_data = ysd.accelerate_pattern[selected];
                    YamlVvvfManage.CurrentData.accelerate_pattern.Add(selected_data.Clone());
                    UpdateControlList();
                    BrakeSelectedShow();
                }
            }
        }



        private String load_path = "";
        public String GetLoadedYamlName()
        {
            return Path.GetFileNameWithoutExtension(load_path);
        }
        private void File_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            Object? tag = button.Tag;
            if (tag == null) return;
            if (tag.Equals("Load"))
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml|All (*.*)|*.*"
                };
                if (dialog.ShowDialog() == false) return;

                try
                {
                    YamlVvvfManage.load_Yaml(dialog.FileName);
                    MessageBox.Show("Load OK.", "Great", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch(YamlException ex)
                {
                    String error_message = "";
                    error_message += "Invalid yaml\r\n";
                    error_message += "\r\n" + ex.End.ToString() + "\r\n";
                    MessageBox.Show(error_message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }


                load_path = dialog.FileName;
                UpdateControlList();
                //update_Control_Showing();
                setting_window.Navigate(null);

            }
            else if (tag.Equals("Save_As"))
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml",
                    FileName = GetLoadedYamlName()
                };

                // ダイアログを表示する
                if (dialog.ShowDialog() == false) return;
                load_path = dialog.FileName;
                if (YamlVvvfManage.save_Yaml(dialog.FileName))
                    MessageBox.Show("Save OK.", "Great", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Error occurred on saving.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (tag.Equals("Save"))
            {
                String save_path = load_path;
                if(save_path.Length == 0)
                {
                    var dialog = new SaveFileDialog
                    {
                        Filter = "Yaml (*.yaml)|*.yaml",
                        FileName = "VVVF"
                    };

                    // ダイアログを表示する
                    if (dialog.ShowDialog() == false) return;
                    load_path = dialog.FileName;
                    save_path = load_path;
                }
                if (YamlVvvfManage.save_Yaml(save_path))
                    MessageBox.Show("Save OK.", "Great", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Error occurred on saving.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (tag.Equals("Export_As_C"))
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "C (*.c)|*.c",
                    FileName = GetLoadedYamlName()
                };

                // ダイアログを表示する
                if (dialog.ShowDialog() == false) return;

                try
                {
                    using (StreamWriter outputFile = new(dialog.FileName))
                    {
                        outputFile.Write(Pi3Generate.GenerateC(YamlVvvfManage.CurrentData, Path.GetFileNameWithoutExtension(dialog.FileName)));
                    }
                    MessageBox.Show("Export as C complete.", "Great", MessageBoxButton.OK, MessageBoxImage.Information);
                }catch
                {
                    MessageBox.Show("Error occurred.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }

        public static class Generation_Params
        {
            public static List<double> Double_Values = new();
        }
        private void Generation_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            Object? tag = button.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;
            String[] command = tag_str.Split("_");


            BindingData.Blocked = true;

            bool unblock = SolveCommand(command);

            if (!unblock) return;
            BindingData.Blocked = false;
            SystemSounds.Beep.Play();

        }

        public class TaskProgressData
        {
            public ProgressData Data { get; set; }
            public Task Task { get; set; }
            public string Description { get; set; }
            public bool Cancelable
            {
                get
                {
                    return (!Data.Cancel && Data.RelativeProgress < 99.9);
                }
            }

            public String Status
            {
                get
                {
                    if (Data.Cancel) return "Canceled";
                    if (Data.RelativeProgress > 99.9) return "Complete";
                    return "Running";
                }
            }

            public SolidColorBrush StatusColor
            {
                get
                {
                    if (Data.Cancel) return new SolidColorBrush(Color.FromRgb(0xFF,0xCB,0x47));
                    if (Data.RelativeProgress > 99.9) return new SolidColorBrush(Color.FromRgb(0x95, 0xE0, 0x6C));
                    return new SolidColorBrush(Color.FromRgb(0x4F, 0x86, 0xC6));
                }
            }

            public TaskProgressData(Task Task, ProgressData progressData, string Description)
            {
                this.Task = Task;
                this.Data = progressData;
                this.Description = Description;
            }
        }

        public static List<TaskProgressData> taskProgresses = new();

        private static GenerationBasicParameter GetGenerationBasicParameter()
        {
            GenerationBasicParameter generationBasicParameter = new(
                Yaml_Mascon_Manage.CurrentData.GetCompiled(),
                YamlVvvfManage.DeepClone(YamlVvvfManage.CurrentData),
                new ProgressData()
            );

            return generationBasicParameter;
        }
        private Boolean SolveCommand(String[] command)
        {

            if (command[0].Equals("VVVF"))
            {
                if (command[1].Equals("WAV"))
                {
                    var dialog = new SaveFileDialog
                    {
                        Filter = "Ultra High Resolution|*.wav|High Resolution|*.wav|Low Resolution|*.wav",
                        FilterIndex = 2
                    };
                    if (dialog.ShowDialog() == false) return true;

                    int sample_freq = new int[] { 1000000 * 5, 192000, 192000 }[dialog.FilterIndex - 1];
                    bool resize = new bool[] {false,false,true}[dialog.FilterIndex - 1];

                    GenerationBasicParameter generationBasicParameter = GetGenerationBasicParameter();

                    Task task = Task.Run(() => {
                        try
                        {
                            Generation.Audio.VVVF_Sound.GenerateVVVFAudio.Export_VVVF_Sound(generationBasicParameter, dialog.FileName, resize, sample_freq);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, generationBasicParameter.progressData, "VVVF sound generation of " + GetLoadedYamlName());
                    taskProgresses.Add(taskProgressData);
                }
               
                else if(command[1].Equals("RealTime"))
                {
                    RealTimeParameter parameter = new()
                    {
                        quit = false
                    };

                    BindingData.Blocked = true;

                    RealTime_Mascon_Window mascon = new(parameter);
                    mascon.Show();
                    mascon.Start_Task();

                    if (Properties.Settings.Default.RealTime_VVVF_WaveForm_Show)
                    {
                        RealTime_WaveForm_Window window = new(parameter);
                        window.Show();
                        window.RunTask();
                    }

                    if (Properties.Settings.Default.RealTime_VVVF_Control_Show)
                    {
                        RealTime_ControlStat_Window window = new(
                            parameter,
                            (RealTime_ControlStat_Style)Properties.Settings.Default.RealTime_VVVF_Control_Style,
                            Properties.Settings.Default.RealTime_VVVF_Control_Precise
                        );
                        window.Show();
                        window.RunTask();
                    }

                    if (Properties.Settings.Default.RealTime_VVVF_Hexagon_Show)
                    {
                        RealTime_Hexagon_Window window = new(
                            parameter,
                            (RealTime_Hexagon_Style)Properties.Settings.Default.RealTime_VVVF_Hexagon_Style,
                            Properties.Settings.Default.RealTime_VVVF_Hexagon_ZeroVector
                        );
                        window.Show();
                        window.RunTask();
                    }

                    if (Properties.Settings.Default.RealTime_VVVF_FFT_Show)
                    {
                        RealTime_FFT_Window window = new(parameter);
                        window.Show();
                        window.RunTask();
                    }

                    if (Properties.Settings.Default.RealTime_VVVF_FS_Show)
                    {
                        RealTime_FS_Window window = new(parameter);
                        window.Show();
                        window.RunTask();
                    }

                    Task task = Task.Run(() => {
                        try
                        {
                            bool do_clone = !Properties.Settings.Default.RealTime_VVVF_EditAllow;
                            YamlVvvfSoundData data;
                            if (do_clone)
                                data = YamlVvvfManage.DeepClone(YamlVvvfManage.CurrentData);
                            else
                                data = YamlVvvfManage.CurrentData;
                            Generation.Audio.VVVF_Sound.RealTimeVVVFAudio.RealTime_VVVF_Generation(data, parameter);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        BindingData.Blocked = false;
                        SystemSounds.Beep.Play();
                    });
                    return Properties.Settings.Default.RealTime_VVVF_EditAllow;
                }
                else if (command[1].Equals("Setting"))
                {
                    RealTime_Basic_Settings setting = new( RealTime_Basic_Settings.RealTime_Basic_Setting_Type.VVVF );
                    setting.ShowDialog();
                }
            }
            else if (command[0].Equals("Train"))
            {
                if (command[1].Equals("WAV"))
                {

                    var dialog = new SaveFileDialog { Filter = "High Resolution|*.wav|Down Sampled|*.wav" };
                    if (dialog.ShowDialog() == false) return true;

                    GenerationBasicParameter generationBasicParameter = GetGenerationBasicParameter();

                    Task task = Task.Run(() => {
                        try
                        {
                            bool resize = dialog.FilterIndex == 2;

                            YamlTrainSoundData trainSound_Data_clone = YamlTrainSoundDataManage.current_data.Clone();
                            Generation.Audio.TrainSound.GenerateTrainAudio.Export_Train_Sound(generationBasicParameter, dialog.FileName, resize, trainSound_Data_clone);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, generationBasicParameter.progressData, "Train sound generation of " + GetLoadedYamlName());
                    taskProgresses.Add(taskProgressData);
                }
                else if (command[1].Equals("RealTime"))
                {
                    RealTimeParameter parameter = new()
                    {
                        quit = false
                    };

                    RealTime_Mascon_Window mascon = new(parameter);
                    mascon.Show();
                    mascon.Start_Task();

                    if (Properties.Settings.Default.RealTime_Train_WaveForm_Show)
                    {
                        RealTime_WaveForm_Window window = new(parameter);
                        window.Show();
                        window.RunTask();
                    }

                    if (Properties.Settings.Default.RealTime_Train_Control_Show)
                    {
                        RealTime_ControlStat_Window window = new(
                            parameter,
                            (RealTime_ControlStat_Style)Properties.Settings.Default.RealTime_Train_Control_Style,
                            Properties.Settings.Default.RealTime_Train_Control_Precise
                        );
                        window.Show();
                        window.RunTask();
                    }

                    if (Properties.Settings.Default.RealTime_Train_Hexagon_Show)
                    {
                        RealTime_Hexagon_Window window = new(
                            parameter,
                            (RealTime_Hexagon_Style)Properties.Settings.Default.RealTime_Train_Hexagon_Style,
                            Properties.Settings.Default.RealTime_Train_Hexagon_ZeroVector
                        );
                        window.Show();
                        window.RunTask();
                    }

                    if (Properties.Settings.Default.RealTime_Train_FFT_Show)
                    {
                        RealTime_FFT_Window window = new(parameter);
                        window.Show();
                        window.RunTask();
                    }

                    if (Properties.Settings.Default.RealTime_Train_FS_Show)
                    {
                        RealTime_FS_Window window = new(parameter);
                        window.Show();
                        window.RunTask();
                    }


                    BindingData.Blocked = true;
                    Task task = Task.Run(() => {
                        try
                        {
                            bool do_clone = !Properties.Settings.Default.RealTime_Train_EditAllow;
                            YamlVvvfSoundData data;
                            if (do_clone)
                                data = YamlVvvfManage.DeepClone(YamlVvvfManage.CurrentData);
                            else
                                data = YamlVvvfManage.CurrentData;
                            Generation.Audio.TrainSound.RealTimeTrainAudio.RealTime_Train_Generation(data , parameter);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        BindingData.Blocked = false;
                        SystemSounds.Beep.Play();
                    });
                    return Properties.Settings.Default.RealTime_Train_EditAllow;
                }
                else if (command[1].Equals("Setting"))
                {
                    RealTime_Basic_Settings setting = new( RealTime_Basic_Settings.RealTime_Basic_Setting_Type.Train );
                    setting.ShowDialog();
                }
            }
            else if (command[0].Equals("Control"))
            {
                var dialog = new SaveFileDialog { Filter = "mp4 (*.mp4)|*.mp4" };
                if (dialog.ShowDialog() == false) return true;
                if (command[1].Equals("Original"))
                {
                    GenerationBasicParameter generationBasicParameter = GetGenerationBasicParameter();

                    Task task = Task.Run(() => {
                        try
                        {
                            Generation.Video.ControlInfo.GenerateControlOriginal.Generate_Control_Original_Video(generationBasicParameter, dialog.FileName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, generationBasicParameter.progressData, "Control video(O1) generation of " + GetLoadedYamlName());
                    taskProgresses.Add(taskProgressData);
                }

                else if (command[1].Equals("Original2"))
                {
                    GenerationBasicParameter generationBasicParameter = GetGenerationBasicParameter();

                    Task task = Task.Run(() => {
                        try
                        {
                            Generation.Video.ControlInfo.GenerateControlOriginal2.Generate_Control_Original2_Video(generationBasicParameter, dialog.FileName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, generationBasicParameter.progressData, "Control video(O2) generation of " + GetLoadedYamlName());
                    taskProgresses.Add(taskProgressData);
                }

                    
            }
            else if (command[0].Equals("WaveForm"))
            {
                var dialog = new SaveFileDialog { Filter = "mp4 (*.mp4)|*.mp4" };
                if (dialog.ShowDialog() == false) return true;

                GenerationBasicParameter generationBasicParameter = GetGenerationBasicParameter();
                Task task = Task.Run(() => {
                    try
                    {
                        if (command[1].Equals("Original"))
                            Generation.Video.WaveForm.GenerateWaveFormUV.Generate_UV_2(generationBasicParameter, dialog.FileName);
                        else if (command[1].Equals("Spaced"))
                            Generation.Video.WaveForm.GenerateWaveFormUV.Generate_UV_1(generationBasicParameter, dialog.FileName);
                        else if (command[1].Equals("UVW"))
                            Generation.Video.WaveForm.GenerateWaveFormUVW.generate_wave_UVW(generationBasicParameter, dialog.FileName);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    SystemSounds.Beep.Play();
                });

                TaskProgressData taskProgressData = new(task, generationBasicParameter.progressData, "Waveform video(" + command[1] + ") generation of " + GetLoadedYamlName());
                taskProgresses.Add(taskProgressData);
            }
            else if (command[0].Equals("Hexagon"))
            {
                MessageBoxResult result = MessageBox.Show("Enable zero vector circle?", "Info", MessageBoxButton.YesNo, MessageBoxImage.Question);
                bool circle = result == MessageBoxResult.Yes;

                if (command[1].Equals("Original"))
                {
                    var dialog = new SaveFileDialog { Filter = "mp4 (*.mp4)|*.mp4" };
                    if (dialog.ShowDialog() == false) return true;

                    GenerationBasicParameter generationBasicParameter = GetGenerationBasicParameter();
                    Task task = Task.Run(() => {
                        try
                        {
                            Generation.Video.Hexagon.GenerateHexagonOriginal.Generate_Hexagon_Original_Video(generationBasicParameter, dialog.FileName, circle);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, generationBasicParameter.progressData, "Hexagon video(Original) generation of " + GetLoadedYamlName());
                    taskProgresses.Add(taskProgressData);
                }
                else if (command[1].Equals("Explain"))
                {
                    var dialog = new SaveFileDialog { Filter = "mp4 (*.mp4)|*.mp4" };
                    if (dialog.ShowDialog() == false) return true;

                    Double_Ask_Form double_Ask_Dialog = new("Enter the frequency.");
                    double_Ask_Dialog.ShowDialog();

                    GenerationBasicParameter generationBasicParameter = GetGenerationBasicParameter();
                    Task task = Task.Run(() => {
                        try
                        {
                            Generation.Video.Hexagon.GenerateHexagonExplain.generate_wave_hexagon_explain(generationBasicParameter, dialog.FileName, circle, Generation_Params.Double_Values[0]);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, generationBasicParameter.progressData, "Hexagon video(Explain) generation of " + GetLoadedYamlName());
                    taskProgresses.Add(taskProgressData);
                }
                else if (command[1].Equals("OriginalImage"))
                {
                    var dialog = new SaveFileDialog { Filter = "png (*.png)|*.png" };
                    if (dialog.ShowDialog() == false) return true;

                    Double_Ask_Form double_Ask_Dialog = new ("Enter the frequency.");
                    double_Ask_Dialog.ShowDialog();

                    Task task = Task.Run(() => {
                        try
                        {
                            YamlVvvfSoundData clone = YamlVvvfManage.DeepClone(YamlVvvfManage.CurrentData);
                            Generation.Video.Hexagon.GenerateHexagonOriginal.Generate_Hexagon_Original_Image(dialog.FileName, clone, circle, Generation_Params.Double_Values[0]);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });
                }
                
            }
            else if (command[0].Equals("FFT"))
            {
                if (command[1].Equals("Video"))
                {
                    var dialog = new SaveFileDialog { Filter = "mp4 (*.mp4)|*.mp4" };
                    if (dialog.ShowDialog() == false) return true;

                    GenerationBasicParameter generationBasicParameter = GetGenerationBasicParameter();
                    Task task = Task.Run(() => {
                        try
                        {
                            Generation.Video.FFT.GenerateFFT.Generate_FFT_Video(generationBasicParameter, dialog.FileName);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });

                    TaskProgressData taskProgressData = new(task, generationBasicParameter.progressData, "FFT video generation of " + GetLoadedYamlName());
                    taskProgresses.Add(taskProgressData);
                }
                else if (command[1].Equals("Image"))
                {
                    var dialog = new SaveFileDialog { Filter = "png (*.png)|*.png" };
                    if (dialog.ShowDialog() == false) return true;

                    Double_Ask_Form double_Ask_Dialog = new("Enter the frequency.");
                    double_Ask_Dialog.ShowDialog();

                    Task task = Task.Run(() => {
                        try
                        {
                            YamlVvvfSoundData clone = YamlVvvfManage.DeepClone(YamlVvvfManage.CurrentData);
                            Generation.Video.FFT.GenerateFFT.Generate_FFT_Image(dialog.FileName, clone, Generation_Params.Double_Values[0]);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });
                }

            }
            return true;
        }

        private void Window_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            Object? tag = button.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;

            if (tag_str.Equals("LCalc"))
            {
                Linear_Calculator lc = new();
                lc.Show();
            }
            else if (tag_str.Equals("TaskProgressView"))
            {
                TaskViewer_Main tvm = new();
                tvm.Show();
            }
        }

        private void Setting_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            Object? tag = button.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;

            if (tag_str.Equals("AccelPattern"))
            {
                BindingData.Blocked = true;
                Mascon_Control_Main gmcw = new();
                gmcw.ShowDialog();
                BindingData.Blocked = false;
            }
            else if (tag_str.Equals("TrainSoundSetting"))
            {
                BindingData.Blocked = true;
                YamlTrainSoundData _TrainSound_Data = YamlTrainSoundDataManage.current_data;
                TrainAudio_Setting_Main tahw = new(_TrainSound_Data);
                tahw.ShowDialog();
                BindingData.Blocked = false;
            }
            
        }

        private void Process_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            Object? tag = button.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;

            if (tag_str.Equals("AutoVoltage"))
            {
                BindingData.Blocked = true;
                Task.Run(() =>
                {
                    MessageBox.Show("The settings which is not using `Linear` will be skipped.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    bool result = YamlVvvfUtil.Auto_Voltage(YamlVvvfManage.CurrentData);
                    if(!result)
                        MessageBox.Show("Please check next things.\r\nAll of the amplitude mode are linear.\r\nAccel and Braking has more than 2 settings.\r\nFrom is grater or equal to 0", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    BindingData.Blocked = false;
                    SystemSounds.Beep.Play();
                });
                
            }else if (tag_str.Equals("FreeRunAmpZero"))
            {
                BindingData.Blocked = true;
                Task.Run(() =>
                {
                    bool result = YamlVvvfUtil.Set_All_FreeRunAmp_Zero(YamlVvvfManage.CurrentData);
                    if (!result)
                        MessageBox.Show("Something went wrong.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    BindingData.Blocked = false;
                    SystemSounds.Beep.Play();
                });
            }

            
        }

        private void Util_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            Object? tag = button.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;

            if(tag_str.Equals("MIDI"))
            {
                GUI.MIDIConvert.MIDIConvert_Main mIDIConvert_Main = new();
                mIDIConvert_Main.Show();
            }

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}

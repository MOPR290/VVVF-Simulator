using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static VvvfSimulator.Generation.GenerateCommon.GenerationBasicParameter;

namespace VvvfSimulator.GUI.TaskViewer
{
    /// <summary>
    /// TaskViewer_Main.xaml の相互作用ロジック
    /// </summary>
    public partial class TaskViewer_Main : Window
    {
        public TaskViewer_Main()
        {
            InitializeComponent();

            DataContext = MainWindow.taskProgresses;
            TaskView.Items.Refresh();

            RunUpdateTask();
        }

        public bool updateGridTask = true;
        public void RunUpdateTask()
        {
            Task task = Task.Run(() =>
            {
                while (updateGridTask)
                {
                    try
                    {
                        Dispatcher.Invoke(() =>
                        {
                            TaskView.Items.Refresh();
                        });
                    }
                    catch
                    {
                        break;
                    }

                    Thread.Sleep(500);
                }
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            updateGridTask = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Object tag = btn.Tag;
            if (tag == null) return;

            List<MainWindow.TaskProgressData> taskProgresses = MainWindow.taskProgresses;
            for (int i = 0; i < taskProgresses.Count; i++)
            {
                MainWindow.TaskProgressData data = taskProgresses[i];
                if (data.Task.Id.ToString().Equals(tag.ToString()))
                {
                    data.Data.Cancel = true;
                    break;
                }
            }
        }

        private void OnWindowControlButtonClick(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            if (btn == null) return;
            string? tag = btn.Tag.ToString();
            if (tag == null) return;

            if (tag.Equals("Close"))
                Close();
            else if (tag.Equals("Maximize"))
            {
                if (WindowState.Equals(WindowState.Maximized))
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;
            }
            else if (tag.Equals("Minimize"))
                WindowState = WindowState.Minimized;
        }
    }
}

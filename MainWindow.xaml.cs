using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ToDoApp
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<TaskItem> TaskItems { get; set; }
        public ObservableCollection<TaskItem> CompletedTaskItems { get; set; }
        private System.Timers.Timer _timer;

        public MainWindow()
        {
            InitializeComponent();

            TaskItems = new ObservableCollection<TaskItem>();
            CompletedTaskItems = new ObservableCollection<TaskItem>();

            TaskListBox.ItemsSource = TaskItems;
            CompletedTaskListBox.ItemsSource = CompletedTaskItems;

            LoadTasks();

            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        private void LoadTasks()
        {
            if (File.Exists("tasks.txt"))
            {
                var lines = File.ReadAllLines("tasks.txt");
                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length == 3 && DateTime.TryParse(parts[1], out var deadline))
                    {
                        var taskItem = new TaskItem
                        {
                            Description = parts[0],
                            Deadline = deadline,
                            IsCompleted = bool.Parse(parts[2]),
                            ForegroundColor = System.Windows.Media.Brushes.Black
                        };

                        taskItem.TaskCompleted += TaskItem_TaskCompleted;
                        taskItem.TaskDeleted += TaskItem_TaskDeleted;

                        if (taskItem.IsCompleted)
                        {
                            CompletedTaskItems.Add(taskItem);
                        }
                        else
                        {
                            TaskItems.Add(taskItem);
                        }
                    }
                }
            }
        }

        private void SaveTasks()
        {
            var lines = TaskItems.Select(t => $"{t.Description}|{t.Deadline}|{t.IsCompleted}")
                        .Concat(CompletedTaskItems.Select(t => $"{t.Description}|{t.Deadline}|{t.IsCompleted}"))
                        .ToArray();
            File.WriteAllLines("tasks.txt", lines); // Uložení úkolů do souboru
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TaskItem taskItem)
            {
                taskItem.DeleteTask();
            }
        }

        private void TaskItem_TaskDeleted(object sender, EventArgs e)
        {
            if (sender is TaskItem taskItem)
            {
                TaskItems.Remove(taskItem);
                CompletedTaskItems.Remove(taskItem);
                SaveTasks();
            }
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnMax_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
            else WindowState = WindowState.Maximized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
            //Application.Current.Shutdown();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (var taskItem in TaskItems.ToList())
                {
                    if (taskItem.Deadline <= DateTime.Now && !taskItem.IsCompleted)
                    {
                        taskItem.IsCompleted = true;
                    }
                }
            });
        }

        private void TaskInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(TaskInput.Text))
            {
                PlaceholderTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                PlaceholderTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TaskInput.Text) && TaskDatePicker.SelectedDate.HasValue)
            {
                var time = ((ComboBoxItem)TaskTimePicker.SelectedItem)?.Content.ToString();
                if (time == null)
                {
                    MessageBox.Show("Please select a time for the task.");
                    return;
                }

                var deadline = DateTime.Parse(TaskDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd") + " " + time);

                var taskItem = new TaskItem
                {
                    Description = TaskInput.Text,
                    Deadline = deadline,
                    ForegroundColor = System.Windows.Media.Brushes.Black
                };

                taskItem.TaskCompleted += TaskItem_TaskCompleted;
                taskItem.TaskDeleted += TaskItem_TaskDeleted;
                TaskItems.Add(taskItem);

                SaveTasks();

                TaskInput.Text = string.Empty;
                TaskDatePicker.SelectedDate = null;
                TaskTimePicker.SelectedIndex = -1;
            }
            else
            {
                MessageBox.Show("Please enter a task description and select a date.");
            }
        }

        private void TaskItem_TaskCompleted(object sender, EventArgs e)
        {
            if (sender is TaskItem taskItem)
            {
                TaskItems.Remove(taskItem);
                CompletedTaskItems.Add(taskItem);
                SaveTasks();
            }
        }

        private void ShowCompletedTasksButton_Click(object sender, RoutedEventArgs e)
        {
            if (CompletedTaskListBox.Visibility == Visibility.Collapsed)
            {
                CompletedTaskListBox.Visibility = Visibility.Visible;
                ShowCompletedTasksButton.Content = "Hide Completed";
            }
            else
            {
                CompletedTaskListBox.Visibility = Visibility.Collapsed;
                ShowCompletedTasksButton.Content = "Show Completed";
            }
        }
    }
}

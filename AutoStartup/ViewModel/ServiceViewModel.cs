using AutoStartup.Model;
using AutoStartup.Services;
using NLog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;

namespace AutoStartup.ViewModel
{
    public class ServiceViewModel : INotifyPropertyChanged
    {
        public Service Service { get; }

        public string Name => Service.Name;

        public ServiceStatus Status => Service.Status;

        public bool Running => Status == ServiceStatus.Running;

        public bool Stopped => Status != ServiceStatus.Running && Status != ServiceStatus.Restarting && Status != ServiceStatus.Error;

        public bool Restarting => Status == ServiceStatus.Restarting;

        public bool Error => Status == ServiceStatus.Error;

        public ObservableCollection<(LogLevel, string)> OperationLogs { get; } = new();

        public ObservableCollection<string> ConsoleOutputs { get; } = new();

        // 操作命令
        public ICommand StartCommand { get; }

        public ICommand StopCommand { get; }

        public ICommand RestartCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand OpenLogFolderCommand { get; }

        public ICommand OpenWorkingDirectoryCommand { get; }

        public ICommand OpenFileFolderCommand { get; }

        public ServiceViewModel(Service svc, Action<ServiceViewModel> onDelete)
        {
            Service = svc;
            StartCommand = new RelayCommand(async _ => await Service.StartAsync(false));
            StopCommand = new RelayCommand(async _ => await Service.StopAsync());
            RestartCommand = new RelayCommand(async _ => await Service.RestartAsync());
            DeleteCommand = new RelayCommand(_ => onDelete?.Invoke(this));
            OpenLogFolderCommand = new RelayCommand(_ => OpenLogFolder());
            OpenWorkingDirectoryCommand = new RelayCommand(_ => OpenWorkingDirectory());
            OpenFileFolderCommand = new RelayCommand(_ => OpenFileFolder());
            // 状态变化通知
            Service.StatusChanged += (_, s) =>
            {
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(Running));
                OnPropertyChanged(nameof(Stopped));
                OnPropertyChanged(nameof(Restarting));
                OnPropertyChanged(nameof(Error));
            };
            foreach (var item in Service.InMemoryOperationLogs)
            {
                OperationLogs.Add((GetLogLevel(item), item));
            }
            foreach (var item in Service.InMemoryLogs)
            {
                ConsoleOutputs.Add(item);
            }

            Service.OperationReceived += (n, line) =>
            {
                MainWindow.UIDispatcher.Invoke(() =>
                {
                    OperationLogs.Add((GetLogLevel(line), line));
                    while (OperationLogs.Count > Service.InMemoryLogLimit)
                    {
                        OperationLogs.RemoveAt(0);
                    }
                });
            };

            Service.OutputReceived += (n, line) =>
            {
                MainWindow.UIDispatcher.Invoke(() =>
                {
                    ConsoleOutputs.Add(line);
                    while (ConsoleOutputs.Count > Service.InMemoryLogLimit)
                    {
                        ConsoleOutputs.RemoveAt(0);
                    }
                });
            };
        }

        private void OpenFileFolder()
        {
            string? filePath = Service?.FileName;
            if (!File.Exists(filePath))
            {
                string fileName = Path.GetFileName(filePath ?? "");
                if (!Path.HasExtension(fileName))
                {
                    fileName += ".exe";
                }
                string[] paths = (Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User)?.Split(';') ?? [])
                    .Concat((Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine)?.Split(';') ?? [])).ToArray();
                foreach (var item in paths)
                {
                    string path = Path.Combine(item, fileName);
                    if (File.Exists(path))
                    {
                        filePath = path;
                        break;
                    }
                }
            }

            if (File.Exists(filePath))
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
            }
        }

        private void OpenWorkingDirectory()
        {
            string? dir = Service.WorkingDirectory;
            if (string.IsNullOrEmpty(dir))
            {
                dir = AppDomain.CurrentDomain.BaseDirectory;
            }
            if (Directory.Exists(dir))
            {
                System.Diagnostics.Process.Start("explorer.exe", dir);
            }
        }

        private void OpenLogFolder()
        {
            try
            {
                string logDir = Path.Combine("Logs", $"Service_{Name}");
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                System.Diagnostics.Process.Start("explorer.exe", logDir);
            }
            catch { }
        }

        private static LogLevel GetLogLevel(string item)
        {
            if (item.Contains("[DEBUG]", StringComparison.OrdinalIgnoreCase))
            {
                return LogLevel.Debug;
            }
            else if (item.Contains("[INFO]", StringComparison.OrdinalIgnoreCase))
            {
                return LogLevel.Info;
            }
            else if (item.Contains("[WARN]", StringComparison.OrdinalIgnoreCase))
            {
                return LogLevel.Warn;
            }
            else if (item.Contains("[ERROR]", StringComparison.OrdinalIgnoreCase))
            {
                return LogLevel.Error;
            }
            else if (item.Contains("[FATAL]", StringComparison.OrdinalIgnoreCase))
            {
                return LogLevel.Fatal;
            }
            return LogLevel.Info;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
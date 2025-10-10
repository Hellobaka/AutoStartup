using AutoStartup.Model;
using AutoStartup.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

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

        public ServiceViewModel(Service svc, Action<ServiceViewModel> onDelete)
        {
            Service = svc;
            StartCommand = new RelayCommand(async _ => await Service.StartAsync());
            StopCommand = new RelayCommand(async _ => await Service.StopAsync());
            RestartCommand = new RelayCommand(async _ => await Service.RestartAsync());
            DeleteCommand = new RelayCommand(_ => onDelete?.Invoke(this));
            // 状态变化通知
            Service.StatusChanged += s =>
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
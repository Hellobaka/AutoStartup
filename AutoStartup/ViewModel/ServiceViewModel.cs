using AutoStartup.Model;
using AutoStartup.Services;
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

        public bool Stopped => Status != ServiceStatus.Running;

        public ObservableCollection<string> OperationLogs { get; } = new();

        public ObservableCollection<string> ConsoleOutputs { get; } = new();

        // 操作命令
        public ICommand StartCommand { get; }

        public ICommand StopCommand { get; }

        public ICommand RestartCommand { get; }

        public ICommand DeleteCommand { get; }

        public ServiceViewModel(Service svc, Action<ServiceViewModel> onDelete)
        {
            Service = svc;
            StartCommand = new RelayCommand(_ => Service.StartAsync());
            StopCommand = new RelayCommand(_ => Service.StopAsync());
            RestartCommand = new RelayCommand(_ => Service.RestartAsync());
            DeleteCommand = new RelayCommand(_ => onDelete?.Invoke(this));
            // 状态变化通知
            Service.StatusChanged += s =>
            {
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(Running));
                OnPropertyChanged(nameof(Stopped));
            };
            Service.OutputReceived += (n, line) => Dispatcher.CurrentDispatcher.Invoke(() => ConsoleOutputs.Add(line));
            // 你可以在Service内部加事件，或者这里订阅NLog日志
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(propertyName, new PropertyChangedEventArgs(propertyName));
        }
    }
}
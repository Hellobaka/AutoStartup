using AutoStartup.Model;
using AutoStartup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace AutoStartup.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ServiceViewModel> Services { get; } = new();
        public ServiceViewModel SelectedService { get; set; }
        public string ManualFilePath { get; set; }
        public string ManualArguments { get; set; }
        public string ManualName { get; set; }
        public ICommand AddManualServiceCommand { get; }

        public MainWindowViewModel()
        {
            AddManualServiceCommand = new RelayCommand(_ => AddManualService());
        }
        // 拖拽添加服务
        public void AddServiceFromFile(string filePath)
        {
            var name = Path.GetFileNameWithoutExtension(filePath);
            var workingDir = Path.GetDirectoryName(filePath);
            var svc = new Service(
                name, filePath, "", workingDir,
                hideWindow: true, autoRestart: false, logConsoleOutput: true, inMemoryLogLimit: 200
            );
            Services.Add(new ServiceViewModel(svc, vm => Services.Remove(vm)));
        }

        // 手动添加服务
        public void AddManualService()
        {
            if (string.IsNullOrWhiteSpace(ManualFilePath)) return;
            var name = string.IsNullOrWhiteSpace(ManualName) ? Path.GetFileNameWithoutExtension(ManualFilePath) : ManualName;
            var workingDir = Path.GetDirectoryName(ManualFilePath);
            var svc = new Service(
                name, ManualFilePath, ManualArguments ?? "", workingDir,
                hideWindow: true, autoRestart: false, logConsoleOutput: true, inMemoryLogLimit: 200
            );
            Services.Add(new ServiceViewModel(svc, vm => Services.Remove(vm)));
            // 清空输入
            ManualFilePath = ManualArguments = ManualName = "";
            OnPropertyChanged(nameof(ManualFilePath));
            OnPropertyChanged(nameof(ManualArguments));
            OnPropertyChanged(nameof(ManualName));
        }
        // 加载/保存JSON配置
        public void LoadFromJson(string file)
        {
            var json = File.ReadAllText(file);
            var configs = JsonSerializer.Deserialize<List<ServiceConfig>>(json);
            Services.Clear();
            foreach (var cfg in configs)
            {
                var svc = new Service(
                    cfg.Name, cfg.FileName, cfg.Arguments, cfg.WorkingDirectory,
                    cfg.HideWindow, 0, cfg.AutoRestart, cfg.RestartDelayMs,
                    cfg.InMemoryLogLimit, cfg.LogConsoleOutput
                );
                Services.Add(new ServiceViewModel(svc, vm => Services.Remove(vm)));
            }
        }

        public void SaveToJson(string file)
        {
            var configs = Services.Select(vm => new ServiceConfig
            {
                Name = vm.Service.Name,
                FileName = vm.Service.FileName,
                Arguments = vm.Service.Arguments,
                WorkingDirectory = vm.Service.WorkingDirectory,
                HideWindow = vm.Service.HideWindow,
                AutoRestart = vm.Service.AutoRestart,
                RestartDelayMs = vm.Service.RestartDelayMs,
                LogConsoleOutput = vm.Service.LogConsoleOutput,
                InMemoryLogLimit = vm.Service.InMemoryLogLimit
            }).ToList();
            var json = JsonSerializer.Serialize(configs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(file, json);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(propertyName, new PropertyChangedEventArgs(propertyName));
        }
    }
}
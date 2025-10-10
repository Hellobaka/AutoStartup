using AutoStartup.Model;
using AutoStartup.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;

namespace AutoStartup.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            ServiceEditCommand = new RelayCommand(_ => EditService());
            ServiceCreateCommand = new RelayCommand(_ => CreateService());
            FilePathBrowserCommand = new RelayCommand(_ => BrowserFilePath());
            WorkingDirectoryBrowserCommand = new RelayCommand(_ => BrowserWorkingDirectory());
            StartAllServiceCommand = new RelayCommand(async _ => await StartAllService());
            StopAllServiceCommand = new RelayCommand(async _ => await StopAllService());
            foreach (var item in ServiceManager.Instance.ListServices())
            {
                Services.Add(new ServiceViewModel(item, RemoveService));
            }

            ListenServiceCollectionChanged();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Service? PreviewService { get; set; }

        public ServiceViewModel? SelectedService { get; set; }

        public ObservableCollection<ServiceViewModel> Services { get; set; } = [];

        public ICommand FilePathBrowserCommand { get; }

        public ICommand StartAllServiceCommand { get; }

        public ICommand StopAllServiceCommand { get; }

        public ICommand ServiceEditCommand { get; }

        public ICommand ServiceCreateCommand { get; }

        public ICommand WorkingDirectoryBrowserCommand { get; }

        public void RemoveDragPreview()
        {
            SelectedService = null;
            PreviewService = new("", "");
        }

        private void CreateService()
        {
            if (string.IsNullOrEmpty(PreviewService?.Name))
            {
                MainWindow.ShowError("服务名称不可为空");
                return;
            }
            if (string.IsNullOrEmpty(PreviewService?.FileName))
            {
                MainWindow.ShowError("服务进程路径不可为空");
                return;
            }
            if (!CheckServiceNameValid(PreviewService.Name))
            {
                MainWindow.ShowError("服务名称包含无效字符");
                return;
            }
            Service service = PreviewService.Clone();
            if (Services.Any(s => s.Service.Name == service.Name))
            {
                MainWindow.ShowError("服务名称已存在，请更换");
                return;
            }
            SelectedService = new ServiceViewModel(service, RemoveService);
            Services.Add(SelectedService);
            ServiceManager.Instance.AddService(service, true);
            if (ServiceManager.Instance.SaveToFile())
            {
                MainWindow.ShowInfo("保存成功");
            }
            else
            {
                MainWindow.ShowError("保存失败，请检查日志获取错误原因");
            }
        }

        public void EditService()
        {
            if (SelectedService == null || PreviewService == null)
            {
                MainWindow.ShowError("未选中项目，无法进行编辑操作");
                return;
            }
            if (!CheckServiceNameValid(PreviewService.Name))
            {
                MainWindow.ShowError("服务名称包含无效字符");
                return;
            }
            if (SelectedService.Name != PreviewService.Name
                && !MainWindow.ShowConfirm("更改服务名称将导致无法查看旧日志，是否确认"))
            {
                return;
            }
            if (Services.Any(x => x.Name == PreviewService.Name && x != SelectedService))
            {
                MainWindow.ShowError("欲更改的服务名称重复，请修改");
                return;
            }
            SelectedService.Service.UpdateBy(PreviewService);
            Services = [.. Services];
            ListenServiceCollectionChanged();
            if (ServiceManager.Instance.SaveToFile())
            {
                MainWindow.ShowInfo("保存成功");
            }
            else
            {
                MainWindow.ShowError("保存失败，请检查日志获取错误原因");
            }
        }

        /// <summary>
        /// 显示拖拽预览
        /// </summary>
        /// <param name="filePath"></param>
        public void ShowDragPreview(string filePath)
        {
            var name = Path.GetFileNameWithoutExtension(filePath);
            var workingDir = Path.GetDirectoryName(filePath);
            var svc = new Service(
                name, filePath, true, "", workingDir,
                hideWindow: false, autoRestart: false, logConsoleOutput: true, inMemoryLogLimit: 200
            );
            SelectedService = new ServiceViewModel(svc, RemoveService);
            PreviewService = svc;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void BrowserFilePath()
        {
            if (PreviewService == null)
            {
                return;
            }
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                FileName = PreviewService.FileName,
                Multiselect = false,
                Filter = "应用程序|*.exe",
                Title = "请选择程序",
            };
            if (dialog.ShowDialog() ?? false)
            {
                PreviewService.FileName = dialog.FileName;
            }
        }

        private void BrowserWorkingDirectory()
        {
            if (PreviewService == null)
            {
                return;
            }
            var dialog = new OpenFolderDialog()
            {
                FolderName = PreviewService.WorkingDirectory,
                Multiselect = false,
                Title = "请选择当前程序的工作目录",
            };
            if (dialog.ShowDialog() ?? false)
            {
                PreviewService.WorkingDirectory = dialog.FolderName;
            }
        }

        private void Services_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            {
                ServiceManager.Instance.MoveService(ServiceManager.Instance.ListServices().ElementAt(e.OldStartingIndex).Name, e.NewStartingIndex);
                ServiceManager.Instance.SaveToFile();
            }
        }

        private void RemoveService(ServiceViewModel service)
        {
            if (MainWindow.ShowConfirm("确定要删除此服务吗？此操作不可逆"))
            {
                Services.Remove(service);
                ServiceManager.Instance.RemoveService(service.Name);
                if (!ServiceManager.Instance.SaveToFile())
                {
                    MainWindow.ShowError("保存失败，请检查日志获取错误原因");
                }
                if (SelectedService == service)
                {
                    SelectedService = null;
                    PreviewService = new("", "");
                }
            }
        }

        private async Task StopAllService()
        {
            await ServiceManager.Instance.StopAllAsync();
        }

        private async Task StartAllService()
        {
            await ServiceManager.Instance.StartAllAsync();
        }

        private void ListenServiceCollectionChanged()
        {
            Services.CollectionChanged -= Services_CollectionChanged;
            Services.CollectionChanged += Services_CollectionChanged;
        }

        private static bool CheckServiceNameValid(string name)
        {
            var chars = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars());
            return !name.Any(name => chars.Contains(name));
        }
    }
}
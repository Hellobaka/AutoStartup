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
            foreach (var item in ServiceManager.Instance.ListServices())
            {
                Services.Add(new ServiceViewModel(item, vm => Services.Remove(vm)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Service? PreviewService { get; set; }

        public ServiceViewModel? SelectedService { get; set; }

        public ObservableCollection<ServiceViewModel> Services { get; set; } = [];

        public ICommand FilePathBrowserCommand { get; }

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
            Service service = PreviewService.Clone();
            if (Services.Any(s => s.Service.Name == service.Name))
            {
                MainWindow.ShowError("服务名称已存在，请更换");
                return;
            }
            SelectedService = new ServiceViewModel(service, vm => Services.Remove(vm));
            Services.Add(SelectedService);
            ServiceManager.Instance.AddService(service);
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
            if (SelectedService == null)
            {
                MainWindow.ShowError("未选中项目，无法进行编辑操作");
                return;
            }
            SelectedService.Service.UpdateBy(PreviewService);
            Services = [.. Services];
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
            SelectedService = new ServiceViewModel(svc, vm => Services.Remove(vm));
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
    }
}
using AutoStartup.Services;
using AutoStartup.ViewModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;

namespace AutoStartup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // TODO: 实现控制台文本自动切割，ANSI颜色
        // TODO: 验证后台服务管理逻辑
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            UIDispatcher = Dispatcher.CurrentDispatcher;
            ExtraServiceButtonMenu.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(ExtraServiceButtonMenuItem_Click));
        }

        public static Dispatcher UIDispatcher { get; private set; }

        private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

        private bool BalloonShown { get; set; }

        public static void ShowError(string msg)
        {
            HandyControl.Controls.MessageBox.Show(msg, "Error", icon: MessageBoxImage.Error);
        }

        public static void ShowInfo(string msg)
        {
            HandyControl.Controls.MessageBox.Show(msg, "Info", icon: MessageBoxImage.Information);
        }

        public static bool ShowConfirm(string msg)
        {
            return HandyControl.Controls.MessageBox.Show(msg, "Info", MessageBoxButton.YesNo, icon: MessageBoxImage.Information) == MessageBoxResult.Yes;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Shared.Maintenance && !BalloonShown)
            {
                BalloonShown = true;
                TaskbarHelper.ShowTrayInfo("AutoStartup", "程序已隐藏到系统托盘，双击图标可重新打开窗口。", ToolTipIcon.Info);
            }

            e.Cancel = true;
            Hide();
        }

        private void ServiceEditor_DropEnter(object sender, DragEventArgs e)
        {
            ServiceEditor.IsExpanded = true;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length != 1 || Path.GetExtension(files.FirstOrDefault()) != ".exe")
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                    return;
                }
                e.Effects = DragDropEffects.Copy;
                ViewModel!.ShowDragPreview(files.First());
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void ServiceEditor_DragLeave(object sender, DragEventArgs e)
        {
            ViewModel!.RemoveDragPreview();
        }

        private void ServiceList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ViewModel!.PreviewService = ViewModel.SelectedService?.Service.Clone();
            foreach (var item in e.RemovedItems)
            {
                if (item is ServiceViewModel vm)
                {
                    vm.OperationLogs.CollectionChanged -= OperationLogs_CollectionChanged;
                    vm.ConsoleOutputs.CollectionChanged -= ConsoleOutputs_CollectionChanged;
                }
            }
            foreach (var item in e.AddedItems)
            {
                if (item is ServiceViewModel vm)
                {
                    vm.OperationLogs.CollectionChanged += OperationLogs_CollectionChanged;
                    vm.ConsoleOutputs.CollectionChanged += ConsoleOutputs_CollectionChanged;
                }
            }
        }

        private void OperationLogs_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }

        private void ConsoleOutputs_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }

        private void ExtraServiceButton_Click(object sender, RoutedEventArgs e)
        {
            ExtraServiceButtonMenu.PlacementTarget = ExtraServiceButton;
            ExtraServiceButtonMenu.Placement = PlacementMode.Bottom;
            ExtraServiceButtonMenu.IsOpen = true;
        }

        private void ExtraServiceButtonMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is not MenuItem menuItem)
            {
                return;
            }
            var tag = menuItem.Tag as string;
            if (string.IsNullOrEmpty(tag))
            {
                return;
            }

            string type = $"AutoStartup.Windows.{tag}";
            var windowType = Type.GetType(type);
            if(windowType == null)
            {
                ShowError($"无法找到窗口类型 {type}");
                return;
            }
            if (Activator.CreateInstance(windowType) is not Window window)
            {
                ShowError($"无法创建窗口 {type}");
                return;
            }
            window.Owner = this;
            window.ShowDialog();
            if ((window.DialogResult ?? false)
                && window.Tag is Service service)
            {
                ViewModel!.PreviewService = service;
            }
        }
    }
}
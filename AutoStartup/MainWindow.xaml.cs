using AnsiColorParser;
using AutoStartup.Services;
using AutoStartup.ViewModel;
using NLog;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
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
            OperationLogDisplay.Document.Blocks.Clear();
            ConsoleOutputDisplay.Document.Blocks.Clear();

            foreach (var item in ViewModel?.SelectedService?.OperationLogs ?? [])
            {
                AddOperationLogLine(item);
            }
            foreach (var item in ViewModel?.SelectedService?.ConsoleOutputs ?? [])
            {
                AddConsoleOutputLine(item);
            }
        }

        private void OperationLogs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                for (int i = 0; i < e.OldItems?.Count; i++)
                {
                    OperationLogDisplay.Document.Blocks.Remove(OperationLogDisplay.Document.Blocks.FirstBlock);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null && e.NewItems.Count > 0)
                {
                    foreach ((LogLevel, string) line in e.NewItems)
                    {
                        AddOperationLogLine(line);
                    }
                }
            }
        }

        private void AddOperationLogLine((LogLevel logLevel, string log) line)
        {
            Paragraph paragraph = new();
            paragraph.LineHeight = 12;
            Run run = new(line.log)
            {
                Foreground = line.logLevel.Name switch
                {
                    "Trace" => System.Windows.Media.Brushes.Gray,
                    "Debug" => System.Windows.Media.Brushes.LightGray,
                    "Info" => System.Windows.Media.Brushes.Green,
                    "Warn" => System.Windows.Media.Brushes.Orange,
                    "Error" => System.Windows.Media.Brushes.Red,
                    "Fatal" => System.Windows.Media.Brushes.DarkRed,
                    _ => System.Windows.Media.Brushes.Black,
                }
            };
            paragraph.Inlines.Add(run);
            OperationLogDisplay.Document.Blocks.Add(paragraph);
            OperationLogDisplay.ScrollToEnd();
        }

        private void AddConsoleOutputLine(string line)
        {
            var ansiParts = ColorParser.SplitAnsi(line);
            if (ansiParts.Length == 0)
            {
                return;
            }
            Paragraph paragraph = new();
            paragraph.LineHeight = 12;
            Run run = new();
            foreach (var item in ansiParts)
            {
                if (item.IsColorAnsi())
                {
                    if (!ColorParser.TryParse(item, out var color)
                        || !color.Valid)
                    {
                        continue;
                    }
                    var brush = new SolidColorBrush(new System.Windows.Media.Color()
                    {
                        A = 255,
                        R = color.Color.R,
                        G = color.Color.G,
                        B = color.Color.B
                    });
                    if (color.IsBackgroundColor)
                    {
                        run.Background = brush;
                    }
                    else if (color.Reset)
                    {
                        run.Foreground = System.Windows.Media.Brushes.Black;
                        run.Background = System.Windows.Media.Brushes.Transparent;
                    }
                    else
                    {
                        run.Foreground = brush;
                    }
                    continue;
                }
                else
                {
                    run.Text = item;
                    paragraph.Inlines.Add(run);
                    Run recreateRun = new();
                    recreateRun.Background = run.Background?.Clone();
                    recreateRun.Foreground = run.Foreground?.Clone();

                    run = recreateRun;
                }
            }
            ConsoleOutputDisplay.Document.Blocks.Add(paragraph);
            ConsoleOutputDisplay.ScrollToEnd();
        }

        private void ConsoleOutputs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                for (int i = 0; i < e.OldItems?.Count; i++)
                {
                    ConsoleOutputDisplay.Document.Blocks.Remove(ConsoleOutputDisplay.Document.Blocks.FirstBlock);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null && e.NewItems.Count > 0)
                {
                    foreach (string line in e.NewItems)
                    {
                        AddConsoleOutputLine(line);
                    }
                }
            }
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
            if (windowType == null)
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
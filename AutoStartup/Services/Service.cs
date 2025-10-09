using Newtonsoft.Json;
using NLog;
using System.ComponentModel;
using System.Diagnostics;

namespace AutoStartup.Services
{
    public enum ServiceStatus
    {
        Stopped,

        Running,

        Starting,

        Stopping,

        Restarting,

        Error
    }

    public class Service : INotifyPropertyChanged
    {
        private readonly LinkedList<string> _inMemoryLogs = new();

        private readonly LinkedList<string> _inMemoryOperationLogs = new();

        private readonly Lock _lock = new();

        private readonly Logger _logger;

        private readonly Lock _logLock = new();

        private CancellationTokenSource? _cts;

        private Process? _process;

        private ServiceStatus _serviceStatus = ServiceStatus.Stopped;

        public Service(string name,
                             string fileName,
                             bool enabled = false,
                             string args = "",
                             string workingDir = "",
                             bool hideWindow = false,
                             int startDelayMs = 0,
                             bool autoRestart = false,
                             int restartDelayMs = 2000,
                             int inMemoryLogLimit = 2000,
                             bool logConsoleOutput = true)
        {
            Name = name;
            FileName = fileName;
            Arguments = args;
            WorkingDirectory = workingDir;
            Enabled = enabled;
            HideWindow = hideWindow;
            StartDelayMs = startDelayMs;
            AutoRestart = autoRestart;
            RestartDelayMs = restartDelayMs;
            LogConsoleOutput = logConsoleOutput;
            InMemoryLogLimit = inMemoryLogLimit;

            string logName = $"Service_{name}";
            _logger = LogManager.GetLogger(logName);
            LogService.RegisterServiceLogger(logName);

            _inMemoryLogs = LogService.ReadLastNLineLog(logName, inMemoryLogLimit, "[Console]");
            _inMemoryOperationLogs = LogService.ReadLastNLineLog(logName, inMemoryLogLimit, "[Operation]");
        }

        public event Action<string, string>? OperationReceived;

        public event Action<string, string>? OutputReceived;

        public event PropertyChangedEventHandler? PropertyChanged;

        public event Action<ServiceStatus>? StatusChanged;

        public string Arguments { get; set; }

        public bool AutoRestart { get; set; }

        public bool Enabled { get; set; }

        public string FileName { get; set; }

        public bool HideWindow { get; set; }

        public int InMemoryLogLimit { get; set; }

        [JsonIgnore]
        public IReadOnlyCollection<string> InMemoryLogs
        {
            get
            {
                lock (_logLock)
                {
                    return _inMemoryLogs.ToList().AsReadOnly();
                }
            }
        }

        [JsonIgnore]
        public IReadOnlyCollection<string> InMemoryOperationLogs
        {
            get
            {
                lock (_logLock)
                {
                    return _inMemoryOperationLogs.ToList().AsReadOnly();
                }
            }
        }

        public bool LogConsoleOutput { get; set; }

        public string Name { get; set; }

        public int RestartDelayMs { get; set; }

        public int StartDelayMs { get; set; }

        [JsonIgnore]
        public ServiceStatus Status
        {
            get => _serviceStatus;
            set
            {
                _serviceStatus = value;
                StatusChanged?.Invoke(_serviceStatus);
            }
        }

        public string WorkingDirectory { get; set; }

        public async Task RestartAsync()
        {
            if (Status == ServiceStatus.Running)
            {
                await StopAsync();
                await Task.Delay(RestartDelayMs);
            }
            await StartAsync();
        }

        public async Task StartAsync()
        {
            lock (_lock)
            {
                if (Status is ServiceStatus.Running or ServiceStatus.Starting)
                {
                    return;
                }

                Status = ServiceStatus.Starting;
            }
            AddOperationLog($"启动服务...", LogLevel.Info);
            if (StartDelayMs > 0)
            {
                await Task.Delay(StartDelayMs);
            }

            _cts = new CancellationTokenSource();

            await Task.Run(() => RunProcessLoop(_cts.Token));
        }

        public async Task StopAsync()
        {
            lock (_lock)
            {
                if (Status is not ServiceStatus.Running and not ServiceStatus.Starting and not ServiceStatus.Error)
                {
                    return;
                }

                Status = ServiceStatus.Stopping;
            }
            _cts?.Cancel();
            try
            {
                if (_process != null && !_process.HasExited)
                {
                    AddOperationLog($"结束进程中...", LogLevel.Info);
                    _process.Kill(true);
                    await _process.WaitForExitAsync();
                }
            }
            catch (Exception ex)
            {
                AddOperationLog($"结束进程过程中发生异常.\n{ex}", LogLevel.Error);
            }
            finally
            {
                lock (_lock) { Status = ServiceStatus.Stopped; }
            }
        }

        public override string ToString()
        {
            return $"名称：{Name}; " +
                   $"文件名：{FileName}; " +
                   $"参数：{Arguments}; " +
                   $"工作目录：{WorkingDirectory}; " +
                   $"启用：{Enabled}; " +
                   $"隐藏窗口：{HideWindow}; " +
                   $"启动延迟(ms)：{StartDelayMs}; " +
                   $"自动重启：{AutoRestart}; " +
                   $"重启延迟(ms)：{RestartDelayMs}; " +
                   $"日志输出：{LogConsoleOutput}; " +
                   $"内存日志限制：{InMemoryLogLimit}; ";
        }

        public void UpdateBy(Service? service)
        {
            if (service == null) return;
            Name = service.Name;
            FileName = service.FileName;
            Arguments = service.Arguments;
            WorkingDirectory = service.WorkingDirectory;
            Enabled = service.Enabled;
            HideWindow = service.HideWindow;
            StartDelayMs = service.StartDelayMs;
            AutoRestart = service.AutoRestart;
            RestartDelayMs = service.RestartDelayMs;
            InMemoryLogLimit = service.InMemoryLogLimit;
        }

        public Service Clone()
        {
            return new Service(Name, FileName, Enabled, Arguments, WorkingDirectory, HideWindow, StartDelayMs, AutoRestart, RestartDelayMs, InMemoryLogLimit, LogConsoleOutput);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AddLogLine(LinkedList<string> list, string line)
        {
            lock (_logLock)
            {
                list.AddLast(line);
                while (list.Count > InMemoryLogLimit)
                {
                    list.RemoveFirst();
                }
            }
        }

        private void AddOperationLog(string log, LogLevel logLevel)
        {
            string fakeNLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff}][{logLevel}] [Operation]{log}";
            OperationReceived?.Invoke(Name, fakeNLog);
            AddLogLine(_inMemoryOperationLogs, fakeNLog);
            _logger.Log(logLevel, "[Operation]" + log);
        }

        private void OnErrorReceived(string output)
        {
            OutputReceived?.Invoke(Name, output);
            AddLogLine(_inMemoryLogs, output);
            if (LogConsoleOutput)
            {
                _logger.Error("[Console]" + output);
            }
        }

        private void OnOutputReceived(string output)
        {
            OutputReceived?.Invoke(Name, output);
            AddLogLine(_inMemoryLogs, output);
            if (LogConsoleOutput)
            {
                _logger.Info("[Console]" + output);
            }
        }

        private async Task RunProcessLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var psi = new ProcessStartInfo(FileName, Arguments)
                    {
                        WorkingDirectory = string.IsNullOrWhiteSpace(WorkingDirectory) ? Environment.CurrentDirectory : WorkingDirectory,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = HideWindow,
                        EnvironmentVariables = { }
                    };
                    _process = new Process { StartInfo = psi, EnableRaisingEvents = true };
                    _process.OutputDataReceived += (s, e) => { if (e.Data != null) { OnOutputReceived(e.Data); } };
                    _process.ErrorDataReceived += (s, e) => { if (e.Data != null) { OnErrorReceived(e.Data); } };
                    _process.Exited += (s, e) => AddOperationLog($"进程已退出。ExitCode={_process.ExitCode}", LogLevel.Warn);

                    _process.Start();
                    _process.BeginOutputReadLine();
                    _process.BeginErrorReadLine();

                    lock (_lock) { Status = ServiceStatus.Running; }
                    AddOperationLog($"进程已启动 (PID: {_process.Id})", LogLevel.Info);

                    await _process.WaitForExitAsync(token);

                    if (token.IsCancellationRequested)
                    {
                        AddOperationLog($"进程已结束，原因：用户手动结束.", LogLevel.Info);
                        lock (_lock) { Status = ServiceStatus.Stopped; }
                        break;
                    }
                    else
                    {
                        AddOperationLog($"进程已结束，原因：自行退出.", LogLevel.Warn);
                        lock (_lock) { Status = ServiceStatus.Error; }
                        if (AutoRestart)
                        {
                            AddOperationLog($"将在 {RestartDelayMs}ms 后尝试重启...", LogLevel.Info);
                            await Task.Delay(RestartDelayMs, token);
                            lock (_lock) { Status = ServiceStatus.Restarting; }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    AddOperationLog($"启动过程发生异常.\n{ex}", LogLevel.Error);
                    lock (_lock) { Status = ServiceStatus.Error; }
                    if (AutoRestart)
                    {
                        await Task.Delay(RestartDelayMs, token);
                        lock (_lock) { Status = ServiceStatus.Restarting; }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}
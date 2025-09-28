using NLog;
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

    public class Service(string name, string fileName, string args = "", string workingDir = "", bool hideWindow = false, int startDelayMs = 0, bool autoRestart = false, int restartDelayMs = 2000, int inMemoryLogLimit = 2000, bool logConsoleOutput = true)
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private ServiceStatus _serviceStatus = ServiceStatus.Stopped;

        public string Name { get; } = name;

        public string FileName { get; } = fileName;

        public string Arguments { get; } = args;

        public string WorkingDirectory { get; } = workingDir;

        public bool HideWindow { get; } = hideWindow;

        public int StartDelayMs { get; } = startDelayMs;

        public bool AutoRestart { get; set; } = autoRestart;

        public int RestartDelayMs { get; set; } = restartDelayMs;

        public bool LogConsoleOutput { get; set; } = logConsoleOutput;

        public ServiceStatus Status
        {
            get => _serviceStatus;
            set
            {
                _serviceStatus = value;
                StatusChanged?.Invoke(_serviceStatus);
            }
        }

        public int InMemoryLogLimit { get; set; } = inMemoryLogLimit;

        private readonly LinkedList<string> _inMemoryLogs = new();

        private readonly object _logLock = new();

        private Process _process;

        private CancellationTokenSource _cts;

        private readonly object _lock = new();

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

        public event Action<string, string> OutputReceived; // (ServiceName, Output)

        public event Action<string, string> ErrorReceived; // (ServiceName, Output)
        public event Action<ServiceStatus> StatusChanged;

        private void OnOutputReceived(string output)
        {
            OutputReceived?.Invoke(Name, output);
            AddLogLine($"[OUT] {output}");
            if (LogConsoleOutput)
            {
                _logger.Info($"{Name}: {output}");
            }
        }

        private void OnErrorReceived(string output)
        {
            ErrorReceived?.Invoke(Name, output);
            AddLogLine($"[ERR] {output}");
            if (LogConsoleOutput)
            {
                _logger.Error($"{Name}: {output}");
            }
        }

        private void AddLogLine(string line)
        {
            lock (_logLock)
            {
                _inMemoryLogs.AddLast(line);
                while (_inMemoryLogs.Count > InMemoryLogLimit)
                {
                    _inMemoryLogs.RemoveFirst();
                }
            }
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
            _logger.Info($"{Name}: Starting service...");
            if (StartDelayMs > 0)
            {
                await Task.Delay(StartDelayMs);
            }

            _cts = new CancellationTokenSource();

            await Task.Run(() => RunProcessLoop(_cts.Token));
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
                    };
                    _process = new Process { StartInfo = psi, EnableRaisingEvents = true };
                    _process.OutputDataReceived += (s, e) => { if (e.Data != null) { OnOutputReceived(e.Data); } };
                    _process.ErrorDataReceived += (s, e) => { if (e.Data != null) { OnErrorReceived(e.Data); } };
                    _process.Exited += (s, e) => _logger.Warn($"{Name}: Process exited with code {_process.ExitCode}");

                    _process.Start();
                    _process.BeginOutputReadLine();
                    _process.BeginErrorReadLine();

                    lock (_lock) { Status = ServiceStatus.Running; }
                    _logger.Info($"{Name}: Service started (PID: {_process.Id})");

                    await _process.WaitForExitAsync(token);

                    if (token.IsCancellationRequested)
                    {
                        _logger.Info($"{Name}: Service stopped by request.");
                        lock (_lock) { Status = ServiceStatus.Stopped; }
                        break;
                    }
                    else
                    {
                        _logger.Warn($"{Name}: Service exited unexpectedly.");
                        lock (_lock) { Status = ServiceStatus.Error; }
                        if (AutoRestart)
                        {
                            _logger.Info($"{Name}: Auto-restarting in {RestartDelayMs}ms...");
                            await Task.Delay(RestartDelayMs, token);
                            lock (_lock) { Status = ServiceStatus.Restarting; }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"{Name}: Exception in service process.");
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
            _logger.Info($"{Name}: Stopping service...");
            _cts?.Cancel();
            try
            {
                if (_process != null && !_process.HasExited)
                {
                    _process.Kill(true);
                    await _process.WaitForExitAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"{Name}: Error stopping process.");
            }
            finally
            {
                lock (_lock) { Status = ServiceStatus.Stopped; }
            }
        }

        public async Task RestartAsync()
        {
            await StopAsync();
            await Task.Delay(RestartDelayMs);
            await StartAsync();
        }
    }
}
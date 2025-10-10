using Newtonsoft.Json;
using NLog;
using System.IO;

namespace AutoStartup.Services
{
    public class ServiceManager
    {
        public const string ConfigFileName = "services.json";

        private readonly Logger _logger = LogManager.GetLogger("ServiceManager");

        private CancellationTokenSource? _batchStartCts;

        private readonly List<Service> _services = new();

        private readonly Lock serviceMaintenanceLock = new();

        public ServiceManager()
        {
            Instance = this;
            LogService.RegisterServiceLogger("ServiceManager");
        }

        public event Action<string, string>? ServiceOutputReceived;

        public static ServiceManager Instance { get; private set; }

        private bool Reloading { get; set; } = false;

        public int TotalCount => _services.Count(x => x.Enabled);

        public int RunningCount => _services.Count(s => s.Enabled && s.Status == ServiceStatus.Running);

        public bool AddService(Service service, bool add = false)
        {
            lock (serviceMaintenanceLock)
            {
                _services.Add(service);
                service.OutputReceived += (name, output) => ServiceOutputReceived?.Invoke(name, output);
                if (add)
                {
                    _logger.Info($"服务 {service.Name} 已被添加, 详情：{service}.");
                }
                return true;
            }
        }

        public IEnumerable<Service> ListServices()
        {
            return _services;
        }

        public bool LoadFromFile()
        {
            if (!File.Exists(ConfigFileName))
            {
                _services.Clear();

                File.WriteAllText(ConfigFileName, "[]");
                _logger.Info("创建默认服务文件");
                return true;
            }

            try
            {
                var services = JsonConvert.DeserializeObject<Service[]>(File.ReadAllText(ConfigFileName));
                Reloading = true;
                if (services != null)
                {
                    _services.Clear();
                    foreach (var svc in services)
                    {
                        AddService(svc);
                    }
                    _logger.Info($"加载了 {services.Length} 个服务");
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"无法从 {Path.GetFullPath(ConfigFileName)} 文件加载服务列表.");
                return false;
            }
            finally
            {
                Reloading = false;
            }
        }

        public bool RemoveService(string name)
        {
            if (Reloading)
            {
                _logger.Warn("Cannot add service while reloading.");
                return false;
            }
            lock (serviceMaintenanceLock)
            {
                var item = _services.FirstOrDefault(x => x.Name == name);
                if (item != null)
                {
                    _services.Remove(item);
                    item.OutputReceived -= (n, o) => ServiceOutputReceived?.Invoke(n, o);
                    _logger.Info($"服务 {name} 已被移除.");
                    return true;
                }
                return false;
            }
        }

        public async Task RestartAllAsync()
        {
            if (Reloading)
            {
                _logger.Warn("由于正在重载配置，无法进行服务重启.");
                return;
            }

            _batchStartCts?.Cancel();
            _batchStartCts = new CancellationTokenSource();
            var token = _batchStartCts.Token;
            _logger.Info("重启所有服务.");

            foreach (var svc in _services.Where(x => x.Enabled))
            {
                if (token.IsCancellationRequested)
                {
                    _logger.Info("批量重启被用户中断.");
                    break;
                }
                _logger.Info($"重启服务 {svc.Name}");
                await svc.RestartAsync();
            }
        }

        public bool SaveToFile()
        {
            try
            {
                File.WriteAllText(ConfigFileName, JsonConvert.SerializeObject(_services, Formatting.Indented));
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"无法将当前服务列表保存到 {Path.GetFullPath(ConfigFileName)} 中");

                return false;
            }
        }

        public async Task StartAllAsync()
        {
            if (Reloading)
            {
                _logger.Warn("由于正在重载配置，无法进行服务启动.");
                return;
            }

            _batchStartCts?.Cancel();
            _batchStartCts = new CancellationTokenSource();
            var token = _batchStartCts.Token;
            _logger.Info("启动所有服务...");

            foreach (var svc in _services.Where(x => x.Enabled))
            {
                if (token.IsCancellationRequested)
                {
                    _logger.Info("批量启动被用户中断.");
                    break;
                }
                _logger.Info($"启动服务 {svc.Name}");
                await svc.StartAsync();
            }
        }

        public async Task StopAllAsync()
        {
            if (Reloading)
            {
                _logger.Warn("由于正在重载配置，无法进行服务终止.");
                return;
            }
            _logger.Info("终止所有服务.");

            _batchStartCts?.Cancel();
            foreach (var svc in _services)
            {
                _logger.Info($"终止服务 {svc.Name}");
                await svc.StopAsync();
            }
        }

        public void MoveService(string name, int newIndex)
        {
            if (Reloading)
            {
                _logger.Warn("由于正在重载配置，无法进行服务重排序.");
                return;
            }
            lock (serviceMaintenanceLock)
            {
                var item = _services.FirstOrDefault(x => x.Name == name);
                if (item != null)
                {
                    _services.Remove(item);
                    _services.Insert(newIndex, item);
                    _logger.Info($"服务 {name} 已被移动到位置 {newIndex}.");
                }
            }
        }
    }
}
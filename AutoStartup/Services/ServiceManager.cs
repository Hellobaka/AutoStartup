using Newtonsoft.Json;
using NLog;
using System.Collections.Concurrent;
using System.IO;

namespace AutoStartup.Services
{
    public class ServiceManager
    {
        public const string ConfigFileName = "services.json";

        private readonly Logger _logger = LogManager.GetLogger("ServiceManager");

        private readonly ConcurrentDictionary<string, Service> _services = new();

        public ServiceManager()
        {
            Instance = this;
            LogService.RegisterServiceLogger("ServiceManager");
        }

        public event Action<string, string>? ServiceOutputReceived;

        public static ServiceManager Instance { get; private set; }

        public int RunningCount => _services.Values.Count(s => s.Status == ServiceStatus.Running);

        private bool Reloading { get; set; } = false;

        public bool AddService(Service service)
        {
            if (_services.TryAdd(service.Name, service))
            {
                service.OutputReceived += (name, output) => ServiceOutputReceived?.Invoke(name, output);
                _logger.Info($"服务 {service.Name} 已被添加, 详情：{service}.");
                return true;
            }
            return false;
        }

        public Service? GetService(string name)
        {
            return _services.TryGetValue(name, out var svc) ? svc : null;
        }

        public IEnumerable<Service> ListServices()
        {
            return _services.Values;
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

            if (_services.TryRemove(name, out var service))
            {
                service.OutputReceived -= (n, o) => ServiceOutputReceived?.Invoke(n, o);
                _logger.Info($"服务 {name} 已被移除.");
                return true;
            }
            return false;
        }

        public async Task RestartAllAsync()
        {
            if (Reloading)
            {
                _logger.Warn("由于正在重载配置，无法进行服务重启.");
                return;
            }

            foreach (var svc in _services.Values)
            {
                await svc.RestartAsync();
            }
        }

        public bool SaveToFile()
        {
            try
            {
                var services = _services.Values.ToArray();
                File.WriteAllText(ConfigFileName, JsonConvert.SerializeObject(services, Formatting.Indented));
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

            foreach (var svc in _services.Values)
            {
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

            foreach (var svc in _services.Values)
            {
                await svc.StopAsync();
            }
        }
    }
}
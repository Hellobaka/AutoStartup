using NLog;
using System.Collections.Concurrent;

namespace AutoStartup.Services
{
    public class ServiceManager
    {
        public int RunningCount => _services.Values.Count(s => s.Status == ServiceStatus.Running);

        private readonly ConcurrentDictionary<string, Service> _services = new();

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public event Action<string, string> ServiceOutputReceived;

        public event Action<string, string> ServiceErrorReceived;

        public bool AddService(Service service)
        {
            if (_services.TryAdd(service.Name, service))
            {
                service.OutputReceived += (name, output) => ServiceOutputReceived?.Invoke(name, output);
                service.ErrorReceived += (name, output) => ServiceErrorReceived?.Invoke(name, output);
                _logger.Info($"Service '{service.Name}' added.");
                return true;
            }
            return false;
        }

        public bool RemoveService(string name)
        {
            if (_services.TryRemove(name, out var service))
            {
                service.OutputReceived -= (n, o) => ServiceOutputReceived?.Invoke(n, o);
                service.ErrorReceived -= (n, o) => ServiceErrorReceived?.Invoke(n, o);
                _logger.Info($"Service '{name}' removed.");
                return true;
            }
            return false;
        }

        public Service GetService(string name)
        {
            return _services.TryGetValue(name, out var svc) ? svc : null;
        }

        public IEnumerable<Service> ListServices()
        {
            return _services.Values;
        }

        public async Task StartAllAsync()
        {
            foreach (var svc in _services.Values)
            {
                await svc.StartAsync();
            }
        }

        public async Task StopAllAsync()
        {
            foreach (var svc in _services.Values)
            {
                await svc.StopAsync();
            }
        }

        public async Task RestartAllAsync()
        {
            foreach (var svc in _services.Values)
            {
                await svc.RestartAsync();
            }
        }
    }
}
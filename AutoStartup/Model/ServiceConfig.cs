using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoStartup.Model
{
    public class ServiceConfig
    {
        public string Name { get; set; }

        public string FileName { get; set; }

        public string Arguments { get; set; }

        public string WorkingDirectory { get; set; }

        public bool HideWindow { get; set; }

        public bool AutoRestart { get; set; }

        public int RestartDelayMs { get; set; }

        public bool LogConsoleOutput { get; set; }

        public int InMemoryLogLimit { get; set; }
    }
}
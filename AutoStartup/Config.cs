using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutoStartup.MainForm;

namespace AutoStartup
{
    public class Config
    {
        public List<StartUpItem> StartUpItems { get; set; } = new();

        public int StartWaitTime { get; set; } = 3;
    }
}

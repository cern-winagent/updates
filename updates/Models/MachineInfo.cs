using System;
using System.Collections.Generic;

namespace updates.Models
{
    class MachineInfo
    {
        public string HostName { set; get; }
        public bool AutomaticUpdates { set; get; }
        public DateTime LastTimeUpdated { set; get; }
        public int NumUpdatesAvailable { set; get; }
        public List<Update> UpdatesAvailable { set; get; }
    }
}

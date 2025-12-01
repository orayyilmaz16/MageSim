using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MageSim.Domain.Events
{
    public readonly struct CombatEvent
    {
        public CombatEventType Type { get; }
        public string Payload { get; }
        public DateTime Timestamp { get; }

        public CombatEvent(CombatEventType type, string payload)
        {
            Type = type; Payload = payload; Timestamp = DateTime.UtcNow;
        }
    }

}

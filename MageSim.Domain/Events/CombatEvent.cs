using System;

namespace MageSim.Domain.Events
{
    public class CombatEvent
    {
        public CombatEventType Type { get; set; }
        public string Payload { get; set; }
        public DateTime Timestamp { get; set; }

        public CombatEvent(CombatEventType type, string payload)
        {
            Type = type;
            Payload = payload;
            Timestamp = DateTime.UtcNow;
        }
    }

}

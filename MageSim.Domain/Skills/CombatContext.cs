using MageSim.Domain.Events;
using MageSim.Domain.States;
using System;

namespace MageSim.Domain.Skills
{
    public sealed class CombatContext
    {
        public int Mana { get; set; }
        public bool TargetInRange { get; set; }
        public bool TargetAlive { get; set; }
        public MageState State { get; set; } = MageState.Idle;

        // Nullable event yerine klasik tanım
        public event Action<CombatEvent> OnEvent;

        public void Emit(CombatEvent ev)
        {
            var handler = OnEvent;
            if (handler != null)
            {
                handler(ev);
            }
        }
    }


}

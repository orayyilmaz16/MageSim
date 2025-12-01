using MageSim.Domain.Abstractions;
using MageSim.Domain.Events;
using MageSim.Domain.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MageSim.Domain.Skills
{
    public sealed class RotationEngine
    {
        private readonly IReadOnlyList<Skill> _priority;
        private readonly TimeSpan _tickInterval;
        private readonly IConditionEvaluator _evaluator;
        private readonly IClock _clock;
        private readonly Random _rng = new Random();

        public RotationEngine(
            IReadOnlyList<Skill> priority,
            TimeSpan tickInterval,
            IConditionEvaluator evaluator,
            IClock clock)
        {
            _priority = priority; _tickInterval = tickInterval; _evaluator = evaluator; _clock = clock;
        }

        public async Task RunAsync(CombatContext ctx, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                Step(ctx);
                var jitterMs = _rng.Next(-25, 35);
                await _clock.Delay(_tickInterval + TimeSpan.FromMilliseconds(jitterMs), ct);
            }
        }

        private void Step(CombatContext ctx)
        {
            // Basit state transitions
            if (ctx.Mana <= 100) ctx.State = MageState.OOM;
            else if (ctx.TargetAlive && ctx.TargetInRange) ctx.State = MageState.Burst;
            else ctx.State = MageState.Idle;

            ctx.Emit(new CombatEvent(CombatEventType.StateChange, ctx.State.ToString()));

            if (ctx.State == MageState.Burst)
            {
                foreach (var s in _priority)
                {
                    if (s.IsReady(ctx, _evaluator, _clock))
                    {
                        s.Use(ctx, _clock);
                        break; // GCD benzeri
                    }
                }
            }
            else if (ctx.State == MageState.OOM)
            {
                // Basit recover simülasyonu
                ctx.Mana += 20;
                ctx.Emit(new CombatEvent(CombatEventType.Warning, "Recovering mana"));
            }
        }
    }
}

    

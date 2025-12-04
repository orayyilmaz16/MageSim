using MageSim.Domain.Abstractions;
using MageSim.Domain.Events;
using MageSim.Domain.States;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MageSim.Domain.Skills
{
    public sealed class RotationEngine
    {
        private readonly List<Skill> _priority;
        private readonly TimeSpan _tickInterval;
        private readonly IConditionEvaluator _evaluator;
        private readonly IClock _clock;
        private readonly Random _rng = new Random();

        private EngineOptions _options = new EngineOptions();

        public RotationEngine(
            List<Skill> skills,
            TimeSpan tickInterval,
            IConditionEvaluator evaluator,
            IClock clock)
        {
            _priority = skills ?? throw new ArgumentNullException(nameof(skills));
            _tickInterval = tickInterval;
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>
        /// Opsiyonları ayarla.
        /// </summary>
        public void Configure(EngineOptions options)
        {
            _options = options ?? new EngineOptions();
        }

        public async Task RunAsync(CombatContext ctx, CancellationToken ct)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    Step(ctx);

                    var jitterMs = _rng.Next(-25, 35);
                    var delay = _tickInterval + TimeSpan.FromMilliseconds(jitterMs);

                    // SpeedMultiplier uygula
                    if (_options.SpeedMultiplier != 1.0)
                        delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds / _options.SpeedMultiplier);

                    if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

                    await _clock.Delay(delay, ct);

                    if (_options.VerboseLogging)
                        ctx.Emit(new CombatEvent(CombatEventType.Info, $"Tick @ {DateTime.Now:HH:mm:ss}"));
                }
            }
            catch (TaskCanceledException)
            {
                if (_options.ErrorTolerance)
                {
                    // iptal normal → swallow
                }
                else
                {
                    throw;
                }
            }
        }

        public void Step(CombatContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));

            // State transitions
            if (!ctx.TargetAlive || !ctx.TargetInRange)
            {
                ctx.State = MageState.Idle;
            }
            else if (ctx.Mana <= 100)
            {
                ctx.State = MageState.OOM;
            }
            else
            {
                ctx.State = MageState.Burst;
            }

            ctx.Emit(new CombatEvent(CombatEventType.StateChange, ctx.State.ToString()));

            switch (ctx.State)
            {
                case MageState.Burst:
                    foreach (var s in _priority)
                    {
                        if (s.IsReady(ctx, _evaluator, _clock))
                        {
                            s.Use(ctx, _clock);
                            ctx.Emit(new CombatEvent(CombatEventType.Action, $"Used {s.Name}"));
                            break; // GCD benzeri
                        }
                    }
                    break;

                case MageState.OOM:
                    ctx.Mana += 20;
                    ctx.Emit(new CombatEvent(CombatEventType.Warning, "Recovering mana"));
                    break;

                case MageState.Idle:
                    ctx.Emit(new CombatEvent(CombatEventType.Info, "Idle"));
                    break;
            }
        }
    }
}

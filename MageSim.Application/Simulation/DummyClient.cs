using MageSim.Domain.Abstractions;
using MageSim.Domain.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MageSim.Application.Simulation
{
    // MageSim.Application/Simulation/DummyClient.cs
    public sealed class DummyClient
    {
        public string Id { get; }
        public CombatContext Context { get; }
        public RotationEngine Engine { get; }
        public IReadOnlyList<Skill> Skills => _skills;

        private readonly List<Skill> _skills;

        public DummyClient(string id, List<Skill> skills, TimeSpan tick, IConditionEvaluator evaluator, IClock clock)
        {
            Id = id; _skills = skills;
            Context = new CombatContext { Mana = 600, TargetAlive = true, TargetInRange = true };
            Engine = new RotationEngine(_skills, tick, evaluator, clock);
        }

        public Task StartAsync(CancellationToken ct) => Engine.RunAsync(Context, ct);
    }

}

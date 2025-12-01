using MageSim.Application.Simulation;
using MageSim.Domain.Abstractions;
using MageSim.Domain.Skills;
using MageSim.Infrastructure.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MageSim.Application.Services
{
    // MageSim.Application/Services/RotationFactory.cs
    public static class RotationFactory
    {
        public static DummyClient Create(InstanceConfig cfg, IConditionEvaluator evaluator, IClock clock)
        {
            var skills = cfg.skills.Select(s =>
                new Skill(s.name, s.key, TimeSpan.FromMilliseconds(s.cdMs), s.mana, s.condition)).ToList();

            return new DummyClient(cfg.id, skills, TimeSpan.FromMilliseconds(cfg.tickMs), evaluator, clock);
        }
    }

}

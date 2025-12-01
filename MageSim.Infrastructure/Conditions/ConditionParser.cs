using MageSim.Domain.Skills;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MageSim.Infrastructure.Conditions
{
    public static class ConditionParser
    {
        // Örnek: "alive&range&mana>=250"
        public static Func<CombatContext, bool> Compile(string dsl)
        {
            // TrimEntries yerine manuel Trim
            var parts = dsl.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                           .Select(p => p.Trim());

            return ctx =>
            {
                foreach (var p in parts)
                {
                    if (p.Equals("alive", StringComparison.OrdinalIgnoreCase) && !ctx.TargetAlive)
                        return false;
                    else if (p.Equals("range", StringComparison.OrdinalIgnoreCase) && !ctx.TargetInRange)
                        return false;
                    else if (p.StartsWith("mana>=", StringComparison.OrdinalIgnoreCase))
                    {
                        int th;
                        if (int.TryParse(p.Substring("mana>=".Length), out th))
                        {
                            if (ctx.Mana < th) return false;
                        }
                    }
                    // İleride: hp<=, debuffMissing, buffActive, vb.
                }
                return true;
            };
        }
    }
}
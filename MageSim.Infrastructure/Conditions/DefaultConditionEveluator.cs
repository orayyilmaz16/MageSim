using MageSim.Domain.Abstractions;
using MageSim.Domain.Skills;
using System;
using System.Collections.Generic;

namespace MageSim.Infrastructure.Conditions
{
    // MageSim.Infrastructure/Conditions/DefaultConditionEvaluator.cs
    public sealed class DefaultConditionEvaluator : IConditionEvaluator
    {
        // target-typed new() yerine açık tip kullanımı
        private readonly Dictionary<string, Func<CombatContext, bool>> _cache
            = new Dictionary<string, Func<CombatContext, bool>>();

        public bool Evaluate(string conditionDsl, CombatContext ctx)
        {
            if (!_cache.TryGetValue(conditionDsl, out var fn))
            {
                fn = ConditionParser.Compile(conditionDsl);
                _cache[conditionDsl] = fn;
            }
            return fn(ctx);
        }
    }
}
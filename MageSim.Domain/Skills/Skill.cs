using MageSim.Domain.Abstractions;
using MageSim.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MageSim.Domain.Skills
{
    public sealed class Skill
    {
        public string Name { get; }
        public string Key { get; }                 // UI için sembolik tuş
        public TimeSpan Cooldown { get; }
        public int ManaCost { get; }
        public string ConditionDsl { get; }

        private DateTime _lastUse = DateTime.MinValue;

        public Skill(string name, string key, TimeSpan cd, int manaCost, string conditionDsl)
        {
            Name = name; Key = key; Cooldown = cd; ManaCost = manaCost; ConditionDsl = conditionDsl;
        }

        public bool IsReady(CombatContext ctx, IConditionEvaluator evaluator, IClock clock)
            => (clock.UtcNow - _lastUse) >= Cooldown
               && ((ManaCost <= 0) || ctx.Mana >= ManaCost)
               && evaluator.Evaluate(ConditionDsl, ctx);

        public void Use(CombatContext ctx, IClock clock)
        {
            _lastUse = clock.UtcNow;
            ctx.Mana += (ManaCost <= 0 ? Math.Abs(ManaCost) : -ManaCost); // negatif mana cost → recover benzeri
            ctx.Emit(new CombatEvent(CombatEventType.Cast, Name));
        }

        public double RelativeCooldownMs(IClock clock)
            => Math.Max(0, Cooldown.TotalMilliseconds - (clock.UtcNow - _lastUse).TotalMilliseconds);
    }

}

using MageSim.Domain.Skills;

namespace MageSim.Domain.Abstractions
{
    public interface IConditionEvaluator
    {
        bool Evaluate(string conditionDsl, CombatContext ctx);
    }

}

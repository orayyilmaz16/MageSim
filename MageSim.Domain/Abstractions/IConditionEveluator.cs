using MageSim.Domain.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MageSim.Domain.Abstractions
{
    public interface IConditionEvaluator
    {
        bool Evaluate(string conditionDsl, CombatContext ctx);
    }

}

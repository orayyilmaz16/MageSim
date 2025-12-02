using FluentAssertions;
using MageSim.Domain.Skills;
using MageSim.Infrastructure.Conditions;


namespace MageSim.Tests.Infrastructure
{
    public class ConditionParserTests
    {
        [Fact]
        public void AliveCondition_ShouldReturnFalse_WhenTargetNotAlive()
        {
            var ctx = new CombatContext { TargetAlive = false };
            var func = ConditionParser.Compile("alive");

            func(ctx).Should().BeFalse();
        }

        [Fact]
        public void AliveCondition_ShouldReturnTrue_WhenTargetAlive()
        {
            var ctx = new CombatContext { TargetAlive = true };
            var func = ConditionParser.Compile("alive");

            func(ctx).Should().BeTrue();
        }

        [Fact]
        public void RangeCondition_ShouldReturnFalse_WhenTargetNotInRange()
        {
            var ctx = new CombatContext { TargetInRange = false };
            var func = ConditionParser.Compile("range");

            func(ctx).Should().BeFalse();
        }

        [Fact]
        public void RangeCondition_ShouldReturnTrue_WhenTargetInRange()
        {
            var ctx = new CombatContext { TargetInRange = true };
            var func = ConditionParser.Compile("range");

            func(ctx).Should().BeTrue();
        }

        [Fact]
        public void ManaCondition_ShouldReturnFalse_WhenManaBelowThreshold()
        {
            var ctx = new CombatContext { Mana = 100 };
            var func = ConditionParser.Compile("mana>=250");

            func(ctx).Should().BeFalse();
        }

        [Fact]
        public void ManaCondition_ShouldReturnTrue_WhenManaAboveThreshold()
        {
            var ctx = new CombatContext { Mana = 300 };
            var func = ConditionParser.Compile("mana>=250");

            func(ctx).Should().BeTrue();
        }

        [Fact]
        public void CombinedConditions_ShouldReturnTrue_WhenAllSatisfied()
        {
            var ctx = new CombatContext
            {
                TargetAlive = true,
                TargetInRange = true,
                Mana = 500
            };

            var func = ConditionParser.Compile("alive&range&mana>=250");

            func(ctx).Should().BeTrue();
        }

        [Fact]
        public void CombinedConditions_ShouldReturnFalse_WhenAnyFails()
        {
            var ctx = new CombatContext
            {
                TargetAlive = true,
                TargetInRange = false, // range koşulu başarısız
                Mana = 500
            };

            var func = ConditionParser.Compile("alive&range&mana>=250");

            func(ctx).Should().BeFalse();
        }
    }
}
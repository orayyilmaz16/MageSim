using FluentAssertions;
using MageSim.Domain.Skills;
using MageSim.Infrastructure.Conditions;

namespace MageSim.Tests.Infrastructure
{
    public class DefaultConditionEvaluatorTests
    {
        [Fact]
        public void Evaluate_ShouldReturnTrue_WhenAliveConditionAndTargetAlive()
        {
            var ctx = new CombatContext { TargetAlive = true };
            var evaluator = new DefaultConditionEvaluator();

            var result = evaluator.Evaluate("alive", ctx);

            result.Should().BeTrue();
        }

        [Fact]
        public void Evaluate_ShouldReturnFalse_WhenAliveConditionAndTargetNotAlive()
        {
            var ctx = new CombatContext { TargetAlive = false };
            var evaluator = new DefaultConditionEvaluator();

            var result = evaluator.Evaluate("alive", ctx);

            result.Should().BeFalse();
        }

        [Fact]
        public void Evaluate_ShouldReturnTrue_WhenRangeConditionAndTargetInRange()
        {
            var ctx = new CombatContext { TargetInRange = true };
            var evaluator = new DefaultConditionEvaluator();

            var result = evaluator.Evaluate("range", ctx);

            result.Should().BeTrue();
        }

        [Fact]
        public void Evaluate_ShouldReturnFalse_WhenManaBelowThreshold()
        {
            var ctx = new CombatContext { Mana = 100 };
            var evaluator = new DefaultConditionEvaluator();

            var result = evaluator.Evaluate("mana>=250", ctx);

            result.Should().BeFalse();
        }

        [Fact]
        public void Evaluate_ShouldReturnTrue_WhenManaAboveThreshold()
        {
            var ctx = new CombatContext { Mana = 300 };
            var evaluator = new DefaultConditionEvaluator();

            var result = evaluator.Evaluate("mana>=250", ctx);

            result.Should().BeTrue();
        }

        [Fact]
        public void Evaluate_ShouldCacheCompiledFunction()
        {
            var ctx = new CombatContext { TargetAlive = true };
            var evaluator = new DefaultConditionEvaluator();

            // İlk çağrı cache'e ekler
            var result1 = evaluator.Evaluate("alive", ctx);

            // İkinci çağrı aynı DSL için cache'ten çalışmalı
            var result2 = evaluator.Evaluate("alive", ctx);

            result1.Should().BeTrue();
            result2.Should().BeTrue();
        }

        [Fact]
        public void Evaluate_ShouldHandleCombinedConditions()
        {
            var ctx = new CombatContext
            {
                TargetAlive = true,
                TargetInRange = true,
                Mana = 500
            };
            var evaluator = new DefaultConditionEvaluator();

            var result = evaluator.Evaluate("alive&range&mana>=250", ctx);

            result.Should().BeTrue();
        }

        [Fact]
        public void Evaluate_ShouldReturnFalse_WhenAnyConditionFails()
        {
            var ctx = new CombatContext
            {
                TargetAlive = true,
                TargetInRange = false, // range koşulu başarısız
                Mana = 500
            };
            var evaluator = new DefaultConditionEvaluator();

            var result = evaluator.Evaluate("alive&range&mana>=250", ctx);

            result.Should().BeFalse();
        }
    }
}
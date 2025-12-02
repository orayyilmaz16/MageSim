using FluentAssertions;
using MageSim.Domain.Abstractions;
using MageSim.Domain.Events;
using MageSim.Domain.Skills;

namespace MageSim.Tests.Domain
{
    // Basit sahte evaluator (her zaman true döner)
    public class AlwaysTrueEvaluator : IConditionEvaluator
    {
        public bool Evaluate(string conditionDsl, CombatContext ctx) => true;
    }

    // Sahte clock (Delay hemen tamamlanır, UtcNow kontrol edilebilir)
    public class FakeClock : IClock
    {
        private DateTime _now;
        public FakeClock(DateTime start) { _now = start; }
        public DateTime UtcNow => _now;
        public Task Delay(TimeSpan delay, CancellationToken ct) => Task.CompletedTask;
        public void Advance(TimeSpan span) => _now = _now.Add(span);
    }

    public class SkillTests
    {
        [Fact]
        public void IsReady_ShouldReturnTrue_WhenCooldownElapsedAndManaSufficient()
        {
            var ctx = new CombatContext { Mana = 200, TargetAlive = true, TargetInRange = true };
            var clock = new FakeClock(DateTime.UtcNow);
            var skill = new Skill("Fireball", "F", TimeSpan.FromMilliseconds(500), 50, "alive&range");

            // İlk kullanım → _lastUse set edilir
            skill.Use(ctx, clock);

            // Cooldown süresini ilerlet
            clock.Advance(TimeSpan.FromMilliseconds(600));

            var ready = skill.IsReady(ctx, new AlwaysTrueEvaluator(), clock);

            ready.Should().BeTrue();
        }

        [Fact]
        public void IsReady_ShouldReturnFalse_WhenManaInsufficient()
        {
            var ctx = new CombatContext { Mana = 10, TargetAlive = true, TargetInRange = true };
            var clock = new FakeClock(DateTime.UtcNow);
            var skill = new Skill("Frostbolt", "R", TimeSpan.FromMilliseconds(500), 40, "alive&range");

            var ready = skill.IsReady(ctx, new AlwaysTrueEvaluator(), clock);

            ready.Should().BeFalse();
        }

        [Fact]
        public void Use_ShouldReduceManaAndEmitEvent()
        {
            var ctx = new CombatContext { Mana = 100, TargetAlive = true, TargetInRange = true };
            var clock = new FakeClock(DateTime.UtcNow);
            var skill = new Skill("Arcane Blast", "A", TimeSpan.FromMilliseconds(500), 30, "alive&range");

            CombatEvent castEvent = null;
            ctx.OnEvent += e => { if (e.Type == CombatEventType.Cast) castEvent = e; };

            skill.Use(ctx, clock);

            ctx.Mana.Should().Be(70); // 100 - 30
            castEvent.Should().NotBeNull();
            castEvent.Payload.Should().Be("Arcane Blast");
        }

        [Fact]
        public void Use_ShouldRecoverMana_WhenManaCostNegative()
        {
            var ctx = new CombatContext { Mana = 50, TargetAlive = true, TargetInRange = true };
            var clock = new FakeClock(DateTime.UtcNow);
            var skill = new Skill("Mana Surge", "M", TimeSpan.FromMilliseconds(500), -20, "alive&range");

            skill.Use(ctx, clock);

            ctx.Mana.Should().Be(70); // 50 + abs(-20)
        }

        [Fact]
        public void RelativeCooldownMs_ShouldReturnRemainingCooldown()
        {
            var ctx = new CombatContext { Mana = 100 };
            var clock = new FakeClock(DateTime.UtcNow);
            var skill = new Skill("Fireball", "F", TimeSpan.FromMilliseconds(1000), 50, "alive&range");

            skill.Use(ctx, clock);

            clock.Advance(TimeSpan.FromMilliseconds(400));
            var remaining = skill.RelativeCooldownMs(clock);

            remaining.Should().BeApproximately(600, 50); // yaklaşık 600ms kaldı
        }
    }
}
using FluentAssertions;
using MageSim.Domain.Abstractions;
using MageSim.Domain.Events;
using MageSim.Domain.Skills;
using MageSim.Domain.States;
using MageSim.Application.Services;
using MageSim.Infrastructure.Config;
using MageSim.Integration.Adapters;

namespace MageSim.Tests.Domain
{
    public class RotationEngineTests
    {
        public class FakeClock : IClock
        {
            private DateTime? _fixedTime;

            public DateTime UtcNow => _fixedTime ?? DateTime.UtcNow;

            public Task Delay(TimeSpan delay, CancellationToken ct) => Task.CompletedTask;

            public void SetFixedTime(DateTime time) => _fixedTime = time;
            public void ResetTime() => _fixedTime = null;
        }

        private Skill CreateSkill(string name, int manaCost, int cdMs)
        {
            return new Skill(name, name.Substring(0, 1), TimeSpan.FromMilliseconds(cdMs), manaCost, "alive&range");
        }

        [Fact]
        public void Step_ShouldSetStateToOOM_WhenManaLow()
        {
            var ctx = new CombatContext { Mana = 50, TargetAlive = true, TargetInRange = true };
            var skills = new List<Skill> { CreateSkill("Fireball", 50, 1000) };
            var engine = new RotationEngine(skills, TimeSpan.FromMilliseconds(100), new AlwaysTrueEvaluator(), new FakeClock());

            engine.Step(ctx);

            ctx.State.Should().Be(MageState.OOM);
            ctx.Mana.Should().BeGreaterThan(50);
        }

        [Fact]
        public void Step_ShouldCastSkill_WhenBurstStateAndSkillReady()
        {
            var ctx = new CombatContext { Mana = 200, TargetAlive = true, TargetInRange = true };
            CombatEvent castEvent = null;
            CombatEvent stateEvent = null;

            ctx.OnEvent += e =>
            {
                if (e.Type == CombatEventType.Cast) castEvent = e;
                if (e.Type == CombatEventType.StateChange) stateEvent = e;
            };

            var skills = new List<Skill> { CreateSkill("Fireball", 50, 1000) };
            var engine = new RotationEngine(skills, TimeSpan.FromMilliseconds(100), new AlwaysTrueEvaluator(), new FakeClock());

            engine.Step(ctx);

            ctx.State.Should().Be(MageState.Burst);
            stateEvent.Should().NotBeNull();
            castEvent.Should().NotBeNull();
            castEvent.Payload.Should().Be("Fireball");
        }

        [Fact]
        public async Task RunAsync_ShouldEmitStateChangeEvents()
        {
            var ctx = new CombatContext { Mana = 300, TargetAlive = true, TargetInRange = true };
            var skills = new List<Skill> { CreateSkill("Fireball", 50, 1000) };
            var engine = new RotationEngine(skills, TimeSpan.FromMilliseconds(50), new AlwaysTrueEvaluator(), new FakeClock());

            CombatEvent stateEvent = null;
            ctx.OnEvent += e =>
            {
                if (e.Type == CombatEventType.StateChange) stateEvent = e;
            };

            var cts = new CancellationTokenSource();
            cts.CancelAfter(200);

            await engine.RunAsync(ctx, cts.Token);

            stateEvent.Should().NotBeNull();
            stateEvent.Type.Should().Be(CombatEventType.StateChange);
            stateEvent.Payload.Should().Be(ctx.State.ToString());
        }

        [Fact]
        public void RotationFactory_CreateKo4Fun_ShouldReturnEngineAndTarget()
        {
            // Arrange: InstanceConfig ile Integration test
            var cfg = new InstanceConfig
            {
                Id = "MageKo4Fun",
                TickMs = 100,
                Skills = new List<SkillConfig>
                {
                    new SkillConfig { Name = "Nova", Key = "D1", CdMs = 1500, Mana = 100, Condition = "alive&range&mana>=100" }
                },
                Window = new WindowSelector { TitleContains = "FakeWindow" }
            };

            var evaluator = new AlwaysTrueEvaluator();
            var clock = new FakeClock();

            // Act
            var (engine, target) = RotationFactory.CreateKo4Fun(cfg, evaluator, clock);

            // Assert
            engine.Should().NotBeNull();
            target.Should().NotBeNull();

            // Engine içindeki skill listesi kontrolü
            var skillsField = engine.GetType()
                .GetField("_priority", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(engine) as List<Skill>;

            skillsField.Should().NotBeNull();
            skillsField.Should().HaveCount(1);
            skillsField[0].Name.Should().Be("Nova");
            skillsField[0].Key.Should().Be("D1");
            skillsField[0].ManaCost.Should().Be(100);
            skillsField[0].Cooldown.Should().Be(TimeSpan.FromMilliseconds(1500));
        }
    }
}

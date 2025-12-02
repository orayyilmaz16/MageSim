using FluentAssertions;
using MageSim.Domain.Abstractions;
using MageSim.Domain.Events;
using MageSim.Domain.Skills;
using MageSim.Domain.States;


namespace MageSim.Tests.Domain
{
    
    public class RotationEngineTests
    {

        public class FakeClock : IClock
        {
            private DateTime? _fixedTime;

            public DateTime UtcNow => _fixedTime ?? DateTime.UtcNow;
            
            public Task Delay(TimeSpan delay, CancellationToken ct)
            {
                return Task.CompletedTask;
            }

           
            public void SetFixedTime(DateTime time)
            {
                _fixedTime = time;
            }

            
            public void ResetTime()
            {
                _fixedTime = null;
            }
        }

        private Skill CreateSkill(string name, int manaCost, int cdMs)
        {
            // Skill: name, icon, cooldown, manaCost, condition
            return new Skill(name, name.Substring(0, 1), TimeSpan.FromMilliseconds(cdMs), manaCost, "alive&range");
        }

        [Fact]
        public void Step_ShouldSetStateToOOM_WhenManaLow()
        {
            // Arrange
            var ctx = new CombatContext { Mana = 50, TargetAlive = true, TargetInRange = true };
            var skills = new List<Skill> { CreateSkill("Fireball", 50, 1000) };
            var engine = new RotationEngine(skills, TimeSpan.FromMilliseconds(100), new AlwaysTrueEvaluator(), new FakeClock());

            // Act
            engine.Step(ctx);

            // Assert
            ctx.State.Should().Be(MageState.OOM);
            ctx.Mana.Should().BeGreaterThan(50); // recovery check
        }

        [Fact]
        public void Step_ShouldCastSkill_WhenBurstStateAndSkillReady()
        {
            // Arrange
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

            // Act
            engine.Step(ctx);

            // Assert
            ctx.State.Should().Be(MageState.Burst);
            stateEvent.Should().NotBeNull();
            castEvent.Should().NotBeNull();
            castEvent.Payload.Should().Be("Fireball");
        }

        [Fact]
        public async Task RunAsync_ShouldEmitStateChangeEvents()
        {
            // Arrange
            var ctx = new CombatContext { Mana = 300, TargetAlive = true, TargetInRange = true };
            var skills = new List<Skill> { CreateSkill("Fireball", 50, 1000) };
            var engine = new RotationEngine(skills, TimeSpan.FromMilliseconds(50), new AlwaysTrueEvaluator(), new FakeClock());

            CombatEvent stateEvent = null;
            ctx.OnEvent += e =>
            {
                if (e.Type == CombatEventType.StateChange) stateEvent = e;
            };

            var cts = new CancellationTokenSource();
            cts.CancelAfter(500); // run a bit longer

            // Act
            await engine.RunAsync(ctx, cts.Token);

            // Assert
            stateEvent.Should().NotBeNull();
            stateEvent.Type.Should().Be(CombatEventType.StateChange);
            stateEvent.Payload.Should().Be(ctx.State.ToString());
        }
    }
}
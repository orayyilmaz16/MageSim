using FluentAssertions;
using MageSim.Application.Services;
using MageSim.Domain.Abstractions;
using MageSim.Domain.Skills;
using MageSim.Infrastructure.Config;

namespace MageSim.Tests.Application
{
    // Basit sahte evaluator ve clock
    public class AlwaysTrueEvaluator : IConditionEvaluator
    {
        public bool Evaluate(string conditionDsl, CombatContext ctx) => true;
    }

    public class FakeClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public Task Delay(TimeSpan delay, System.Threading.CancellationToken ct)
            => Task.CompletedTask;
    }

    public class RotationFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnDummyClientWithCorrectProperties()
        {
            // Arrange: InstanceConfig tanımı
            var cfg = new InstanceConfig
            {
                id = "Mage1",
                tickMs = 150,
                skills = new List<SkillConfig>
                {
                    new SkillConfig { name = "Fireball", key = "F", cdMs = 1000, mana = 50, condition = "alive&range&mana>=50" },
                    new SkillConfig { name = "Frostbolt", key = "R", cdMs = 1200, mana = 40, condition = "alive&range&mana>=40" }
                }
            };

            var evaluator = new AlwaysTrueEvaluator();
            var clock = new FakeClock();

            // Act: RotationFactory kullanımı
            var client = RotationFactory.Create(cfg, evaluator, clock);

            // Assert: DummyClient özellikleri doğru mu?
            client.Should().NotBeNull();
            client.Id.Should().Be("Mage1");
            client.Skills.Should().HaveCount(2);

            var fireball = client.Skills[0];
            fireball.Name.Should().Be("Fireball");
            fireball.Key.Should().Be("F");
            fireball.ManaCost.Should().Be(50);
            fireball.Cooldown.Should().Be(TimeSpan.FromMilliseconds(1000));
            fireball.ConditionDsl.Should().Be("alive&range&mana>=50");

            var frostbolt = client.Skills[1];
            frostbolt.Name.Should().Be("Frostbolt");
            frostbolt.Key.Should().Be("R");
            frostbolt.ManaCost.Should().Be(40);
            frostbolt.Cooldown.Should().Be(TimeSpan.FromMilliseconds(1200));
            frostbolt.ConditionDsl.Should().Be("alive&range&mana>=40");
        }
    }
}
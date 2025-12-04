using FluentAssertions;
using MageSim.Application.Services;
using MageSim.Domain.Abstractions;
using MageSim.Domain.Skills;
using MageSim.Infrastructure.Config;
using MageSim.Integration.Adapters;

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
        public void CreateDummy_ShouldReturnDummyClientWithCorrectProperties()
        {
            // Arrange: InstanceConfig tanımı
            var cfg = new InstanceConfig
            {
                Id = "Mage1",
                TickMs = 150,
                Skills = new List<SkillConfig>
                {
                    new SkillConfig { Name = "Fireball", Key = "F", CdMs = 1000, Mana = 50, Condition = "alive&range&mana>=50" },
                    new SkillConfig { Name = "Frostbolt", Key = "R", CdMs = 1200, Mana = 40, Condition = "alive&range&mana>=40" }
                }
            };

            var evaluator = new AlwaysTrueEvaluator();
            var clock = new FakeClock();

            // Act: RotationFactory kullanımı (DummyClient)
            var client = RotationFactory.CreateDummy(cfg, evaluator, clock);

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

        [Fact]
        public void CreateKo4Fun_ShouldReturnEngineAndTarget()
        {
            // Arrange: InstanceConfig tanımı
            var cfg = new InstanceConfig
            {
                Id = "Mage2",
                TickMs = 200,
                Skills = new List<SkillConfig>
                {
                    new SkillConfig { Name = "Nova", Key = "D1", CdMs = 1500, Mana = 100, Condition = "alive&range&mana>=100" }
                },
                Window = new WindowSelector
                {
                    TitleContains = "FakeWindow" // test için sahte değer
                }
            };

            var evaluator = new AlwaysTrueEvaluator();
            var clock = new FakeClock();

            // Act: RotationFactory kullanımı (Ko4Fun entegrasyonu)
            var (engine, target) = RotationFactory.CreateKo4Fun(cfg, evaluator, clock);

            // Assert: Engine ve Target doğru mu?
            engine.Should().NotBeNull();
            target.Should().NotBeNull();

            // Engine skill listesi kontrolü
            var skill = engine.GetType()
                              .GetField("_priority", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                              ?.GetValue(engine) as List<Skill>;

            skill.Should().NotBeNull();
            skill.Should().HaveCount(1);
            skill[0].Name.Should().Be("Nova");
            skill[0].Key.Should().Be("D1");
            skill[0].ManaCost.Should().Be(100);
            skill[0].Cooldown.Should().Be(TimeSpan.FromMilliseconds(1500));
        }
    }
}

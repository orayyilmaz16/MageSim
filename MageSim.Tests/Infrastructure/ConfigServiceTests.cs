
using FluentAssertions;
using MageSim.Infrastructure.Config;

namespace MageSim.Tests.Infrastructure
{
    public class ConfigServiceTests
    {
        private string GetTempFilePath()
        {
            var fileName = Path.GetTempFileName();
            File.Delete(fileName); // sadece path kalsın
            return fileName;
        }

        [Fact]
        public async Task LoadAsync_ShouldReturnEmptyRootConfig_WhenFileDoesNotExist()
        {
            // Arrange
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "config.json");
            var service = new ConfigService(path);

            // Act
            var config = await service.LoadAsync();

            // Assert
            config.Should().NotBeNull();
            config.Instances.Should().BeEmpty();
        }

        [Fact]
        public async Task SaveAsync_AndLoadAsync_ShouldPersistInstances()
        {
            // Arrange
            var path = GetTempFilePath();
            var service = new ConfigService(path);

            var root = new RootConfig
            {
                Instances = new List<InstanceConfig>
                {
                    new InstanceConfig
                    {
                        Id = "Mage1",
                        TickMs = 100,
                        Skills = new List<SkillConfig>
                        {
                            new SkillConfig { Name = "Fireball", Key = "F", CdMs = 1500, Mana = 50, Condition = "TargetAlive" },
                            new SkillConfig { Name = "Frostbolt", Key = "R", CdMs = 2000, Mana = 40, Condition = "TargetInRange" }
                        }
                    },
                    new InstanceConfig
                    {
                        Id = "Mage2",
                        TickMs = 120,
                        Skills = new List<SkillConfig>
                        {
                            new SkillConfig { Name = "Arcane Blast", Key = "A", CdMs = 1000, Mana = 30, Condition = "Mana>30" }
                        }
                    }
                }
            };

            // Act
            await service.SaveAsync(root);
            var loaded = await service.LoadAsync();

            // Assert
            loaded.Should().NotBeNull();
            loaded.Instances.Should().HaveCount(2);
            loaded.Instances[0].Id.Should().Be("Mage1");
            loaded.Instances[0].Skills.Should().ContainSingle(s => s.Name == "Fireball");
            loaded.Instances[1].Id.Should().Be("Mage2");
            loaded.Instances[1].Skills.Should().ContainSingle(s => s.Name == "Arcane Blast");
        }

        [Fact]
        public async Task SaveAsync_ShouldCreateDirectory_WhenNotExists()
        {
            // Arrange
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var path = Path.Combine(dir, "config.json");
            var service = new ConfigService(path);

            var root = new RootConfig();

            // Act
            await service.SaveAsync(root);

            // Assert
            File.Exists(path).Should().BeTrue();
        }
    }
}
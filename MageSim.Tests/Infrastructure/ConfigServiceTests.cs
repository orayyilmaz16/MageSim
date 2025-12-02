
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
            config.instances.Should().BeEmpty();
        }

        [Fact]
        public async Task SaveAsync_AndLoadAsync_ShouldPersistInstances()
        {
            // Arrange
            var path = GetTempFilePath();
            var service = new ConfigService(path);

            var root = new RootConfig
            {
                instances = new List<InstanceConfig>
                {
                    new InstanceConfig
                    {
                        id = "Mage1",
                        tickMs = 100,
                        skills = new List<SkillConfig>
                        {
                            new SkillConfig { name = "Fireball", key = "F", cdMs = 1500, mana = 50, condition = "TargetAlive" },
                            new SkillConfig { name = "Frostbolt", key = "R", cdMs = 2000, mana = 40, condition = "TargetInRange" }
                        }
                    },
                    new InstanceConfig
                    {
                        id = "Mage2",
                        tickMs = 120,
                        skills = new List<SkillConfig>
                        {
                            new SkillConfig { name = "Arcane Blast", key = "A", cdMs = 1000, mana = 30, condition = "Mana>30" }
                        }
                    }
                }
            };

            // Act
            await service.SaveAsync(root);
            var loaded = await service.LoadAsync();

            // Assert
            loaded.Should().NotBeNull();
            loaded.instances.Should().HaveCount(2);
            loaded.instances[0].id.Should().Be("Mage1");
            loaded.instances[0].skills.Should().ContainSingle(s => s.name == "Fireball");
            loaded.instances[1].id.Should().Be("Mage2");
            loaded.instances[1].skills.Should().ContainSingle(s => s.name == "Arcane Blast");
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
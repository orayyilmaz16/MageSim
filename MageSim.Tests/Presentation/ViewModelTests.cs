using FluentAssertions;
using MageSim.Application.Simulation;
using MageSim.Domain.Abstractions;
using MageSim.Domain.Events;
using MageSim.Domain.Skills;
using MageSim.Infrastructure.Config;
using MageSim.Presentation.ViewModels;

namespace MageSim.Tests.Presentation
{
    // Always-true condition evaluator
    public class AlwaysTrueEvaluator : IConditionEvaluator
    {
        public bool Evaluate(string conditionDsl, CombatContext ctx) => true;
    }

    // Fake clock (no real delays)
    public class FakeClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public Task Delay(TimeSpan delay, CancellationToken ct) => Task.CompletedTask;
    }

    public class ViewModelTests
    {
        [Fact]
        public void SkillViewModel_ShouldRaisePropertyChanged()
        {
            var vm = new SkillViewModel();
            string changed = null;

            vm.PropertyChanged += (s, e) => changed = e.PropertyName;

            vm.Name = "Fireball";
            changed.Should().Be(nameof(SkillViewModel.Name));

            vm.Key = "F";
            changed.Should().Be(nameof(SkillViewModel.Key));

            vm.CooldownMs = 1500;
            changed.Should().Be(nameof(SkillViewModel.CooldownMs));

            vm.Mana = 40;
            changed.Should().Be(nameof(SkillViewModel.Mana));

            vm.Condition = "alive&range";
            changed.Should().Be(nameof(SkillViewModel.Condition));
        }

        [Fact]
        public void ClientViewModel_Bind_ShouldReflectStateAndManaChanges()
        {
            var skills = new List<Skill>
            {
                new Skill("Fireball","F",TimeSpan.FromMilliseconds(1000),50,"alive&range"),
                new Skill("Frostbolt","R",TimeSpan.FromMilliseconds(1200),40,"alive&range")
            };

            var client = new DummyClient("Mage1", skills,
                TimeSpan.FromMilliseconds(100),
                new AlwaysTrueEvaluator(),
                new FakeClock());

            var vm = new ClientViewModel(client.Id);
            vm.Bind(client);

            vm.Skills.Should().HaveCount(2);
            vm.State.Should().Be("Idle");
            vm.Mana.Should().Be(client.Context.Mana);

            // State change event
            client.Context.Emit(new CombatEvent(CombatEventType.StateChange, "Burst"));
            vm.State.Should().Be("Burst");

            // Cast event adjusts mana
            client.Context.Emit(new CombatEvent(CombatEventType.Cast, "Fireball"));
            vm.Mana.Should().Be(client.Context.Mana);
        }

        [Fact]
        public async Task MainViewModel_Load_ShouldPopulateRootAndClients()
        {
            // Gerçek mage-config.json dosyasını kullan
            var configService = new ConfigService("mage-config.json");
            var coord = new Coordinator();
            var vm = new MainViewModel(configService, coord, new AlwaysTrueEvaluator(), new FakeClock());

            string changedProp = null;
            vm.PropertyChanged += (s, e) => changedProp = e.PropertyName;

            // ICommand → async void → Load() çağrılır
            vm.LoadConfigCommand.Execute(null);

            // Load() bitmesini beklemek için
            await Task.Delay(100);

            changedProp.Should().Be(nameof(MainViewModel.Root));
            vm.Root.Should().NotBeNull();
            vm.Clients.Should().NotBeEmpty();
            vm.Clients[0].Id.Should().NotBeNullOrEmpty();
        }
    }
}
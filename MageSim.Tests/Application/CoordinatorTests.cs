using FluentAssertions;
using MageSim.Application.Simulation;
using MageSim.Domain.Events;
using MageSim.Domain.Skills;
using MageSim.Domain.States;

namespace MageSim.Tests.Application
{
 
    public class CoordinatorTests
    {
        private DummyClient CreateDummyClient(string id)
        {
            var skills = new List<Skill>
            {
                new Skill("Fireball", "F", TimeSpan.FromMilliseconds(1000), 50, "alive&range&mana>=50"),
                new Skill("Frostbolt", "R", TimeSpan.FromMilliseconds(1200), 40, "alive&range&mana>=40")
            };

            return new DummyClient(
                id,
                skills,
                TimeSpan.FromMilliseconds(100),
                new AlwaysTrueEvaluator(),
                new FakeClock()
            );
        }

        [Fact]
        public void Add_ShouldRegisterClientAndRaiseEvents()
        {
            var coordinator = new Coordinator();
            var client = CreateDummyClient("Mage1");

            string receivedId = null;
            CombatEvent receivedEvent = null;

            coordinator.OnClientEvent += (id, ev) =>
            {
                receivedId = id;
                receivedEvent = ev;
            };

            coordinator.Add(client);

            // Emit event from client context
            client.Context.Emit(new CombatEvent(CombatEventType.Cast, "Fireball"));

            receivedId.Should().Be("Mage1");
            receivedEvent.Should().NotBeNull();
            receivedEvent.Type.Should().Be(CombatEventType.Cast);
            receivedEvent.Payload.Should().Be("Fireball");
        }

        [Fact]
        public void Broadcast_ShouldEmitWarningEventToAllClients()
        {
            var coordinator = new Coordinator();
            var client1 = CreateDummyClient("Mage1");
            var client2 = CreateDummyClient("Mage2");

            CombatEvent ev1 = null;
            CombatEvent ev2 = null;

            coordinator.Add(client1);
            coordinator.Add(client2);

            client1.Context.OnEvent += e => ev1 = e;
            client2.Context.OnEvent += e => ev2 = e;

            coordinator.Broadcast("Low Mana!");

            ev1.Should().NotBeNull();
            ev1.Type.Should().Be(CombatEventType.Warning);
            ev1.Payload.Should().Be("Low Mana!");

            ev2.Should().NotBeNull();
            ev2.Type.Should().Be(CombatEventType.Warning);
            ev2.Payload.Should().Be("Low Mana!");
        }

        [Fact]
        public async Task StartAllAsync_ShouldRunClients()
        {
            var coordinator = new Coordinator();
            var client = CreateDummyClient("Mage1");
            coordinator.Add(client);

            await coordinator.StartAllAsync();

            // DummyClient çalıştıktan sonra context Idle durumda kalmalı
            client.Context.State.Should().Be(MageState.Idle);

            coordinator.StopAll(); // Cancel token
        }

        [Fact]
        public void ClientsProperty_ShouldReturnAddedClients()
        {
            var coordinator = new Coordinator();
            var client1 = CreateDummyClient("Mage1");
            var client2 = CreateDummyClient("Mage2");

            coordinator.Add(client1);
            coordinator.Add(client2);

            coordinator.Clients.Should().Contain(client1);
            coordinator.Clients.Should().Contain(client2);
        }
    }
}
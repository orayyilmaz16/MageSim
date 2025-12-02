using MageSim.Domain.Events;
using MageSim.Domain.States;
using MageSim.Domain.Skills;
using FluentAssertions;

namespace MageSim.Tests.Domain
{
    public class CombatContextTests
    {
        [Fact]
        public void Emit_ShouldTriggerOnEvent()
        {
            // Arrange
            var context = new CombatContext();
            CombatEvent received = null;

            context.OnEvent += e => received = e;

            var ev = new CombatEvent(
                CombatEventType.StateChange,
                "Casting"
            );

            // Act
            context.Emit(ev);

            // Assert
            received.Should().NotBeNull();
            received.Type.Should().Be(CombatEventType.StateChange);
            received.Payload.Should().Be("Casting");
        }

        [Fact]
        public void DefaultState_ShouldBeIdle()
        {
            var context = new CombatContext();
            context.State.Should().Be(MageState.Idle);
        }

        [Fact]
        public void ManaProperty_ShouldBeSettable()
        {
            var context = new CombatContext { Mana = 100 };
            context.Mana.Should().Be(100);
        }

        [Fact]
        public void TargetFlags_ShouldBeSettable()
        {
            var context = new CombatContext
            {
                TargetAlive = true,
                TargetInRange = false
            };

            context.TargetAlive.Should().BeTrue();
            context.TargetInRange.Should().BeFalse();
        }
    }
}
using MageSim.Domain.Skills;

namespace MageSim.Integration.Adapters
{
    public interface IRotationTarget
    {
        void Cast(Skill skill);
    }

    public sealed class Ko4FunRotationTarget : IRotationTarget
    {
        private readonly Ko4FunClient _client;
        public Ko4FunRotationTarget(Ko4FunClient client) => _client = client;
        public void Cast(Skill skill) => _client.CastSkill(skill);
    }
}

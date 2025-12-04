using System;
using MageSim.Domain.Skills;
using MageSim.Integration.Input;

namespace MageSim.Integration.Adapters
{
    public sealed class Ko4FunClient
    {
        private readonly IntPtr _hWnd;
        private readonly KeyDispatcher _keys = new KeyDispatcher();

        public Ko4FunClient(IntPtr hWnd) => _hWnd = hWnd;

        public void CastSkill(Skill skill)
        {
            var vk = KeyMap.Map(skill.Key);
            _keys.SendKey(_hWnd, vk);
        }
    }
}

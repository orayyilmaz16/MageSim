namespace MageSim.Api.DTOs
{
    public class SkillDto
    {
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int CooldownMs { get; set; }
        public int Mana { get; set; }
        public string Condition { get; set; } = string.Empty;
    }
}

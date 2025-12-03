namespace MageSim.Api.DTOs
{
    public class InstanceDto
    {
        public string Id { get; set; } = string.Empty;
        public int TickMs { get; set; }
        public List<SkillDto> Skills { get; set; } = new();
    }
}

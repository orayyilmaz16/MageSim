
using System.Collections.Generic;

namespace MageSim.Infrastructure.Config
{
    // MageSim.Infrastructure/Config/ConfigModels.cs
    public sealed class SkillConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int CdMs { get; set; }
        public int Mana { get; set; }
        public string Condition { get; set; } = string.Empty;
    }

    public sealed class InstanceConfig
    {
        public string Id { get; set; } = string.Empty;
        public int TickMs { get; set; }
        public WindowSelector Window { get; set; }
        public List<SkillConfig> Skills { get; set; } = new List<SkillConfig>();
    }

    public sealed class WindowSelector
    {
        public string TitleContains { get; set; }
        public string ClassName { get; set; }
        public string ProcessName { get; set; }
        public int? ProcessIndex { get; set; }
    }

    public sealed class RootConfig
    {
        // yine açık tip kullanımı
        public List<InstanceConfig> Instances { get; set; } = new List<InstanceConfig>();
    }
}
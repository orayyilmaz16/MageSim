
using System.Collections.Generic;

namespace MageSim.Infrastructure.Config
{
    // MageSim.Infrastructure/Config/ConfigModels.cs
    public sealed class SkillConfig
    {
        public string name { get; set; } = string.Empty;
        public string key { get; set; } = string.Empty;
        public int cdMs { get; set; }
        public int mana { get; set; }
        public string condition { get; set; } = string.Empty;
    }

    public sealed class InstanceConfig
    {
        public string id { get; set; } = string.Empty;
        public int tickMs { get; set; }
        // target-typed new() yerine açık tip kullanımı
        public List<SkillConfig> skills { get; set; } = new List<SkillConfig>();
    }

    public sealed class RootConfig
    {
        // yine açık tip kullanımı
        public List<InstanceConfig> instances { get; set; } = new List<InstanceConfig>();
    }
}
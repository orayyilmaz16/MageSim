using System;
using System.Linq;
using MageSim.Domain.Abstractions;
using MageSim.Domain.Skills;
using MageSim.Infrastructure.Config;
using MageSim.Application.Simulation;
using MageSim.Integration.Window;
using MageSim.Integration.Adapters;

namespace MageSim.Application.Services
{
    // MageSim.Application/Services/RotationFactory.cs
    public static class RotationFactory
    {
        /// <summary>
        /// DummyClient oluşturur (test/simülasyon için).
        /// </summary>
        public static DummyClient CreateDummy(InstanceConfig cfg, IConditionEvaluator evaluator, IClock clock)
        {
            if (cfg == null) throw new ArgumentNullException(nameof(cfg));

            var skills = cfg.Skills.Select(s =>
                new Skill(s.Name, s.Key, TimeSpan.FromMilliseconds(s.CdMs), s.Mana, s.Condition)).ToList();

            return new DummyClient(cfg.Id, skills, TimeSpan.FromMilliseconds(cfg.TickMs), evaluator, clock);
        }

        /// <summary>
        /// Ko4Fun entegrasyonu için RotationEngine + IRotationTarget döndürür.
        /// Opsiyonel EngineOptions parametresi ile macro senaryoları desteklenir.
        /// </summary>
        public static (RotationEngine engine, IRotationTarget target) CreateKo4Fun(
            InstanceConfig cfg, IConditionEvaluator eval, IClock clock, EngineOptions options = null)
        {
            if (cfg == null) throw new ArgumentNullException(nameof(cfg));

            var skills = cfg.Skills.Select(s => new Skill(
                s.Name, s.Key, TimeSpan.FromMilliseconds(s.CdMs), s.Mana, s.Condition)).ToList();

            var engine = new RotationEngine(skills, TimeSpan.FromMilliseconds(cfg.TickMs), eval, clock);

            // Macro senaryoları için opsiyon uygula
            if (options != null)
                engine.Configure(options);

            IntPtr hWnd = IntPtr.Zero;
            if (cfg.Window != null)
            {
                if (!string.IsNullOrWhiteSpace(cfg.Window.TitleContains))
                    hWnd = WindowFinder.FindByTitleContains(cfg.Window.TitleContains);

                if (hWnd == IntPtr.Zero && !string.IsNullOrWhiteSpace(cfg.Window.ProcessName))
                    hWnd = WindowFinder.FindByProcess(cfg.Window.ProcessName, cfg.Window.ProcessIndex ?? 0);
            }

            if (hWnd == IntPtr.Zero)
                Console.WriteLine($"[WARN] Pencere bulunamadı: {cfg.Window?.TitleContains ?? cfg.Window?.ProcessName}");

            var client = new Ko4FunClient(hWnd);
            var target = new Ko4FunRotationTarget(client);

            return (engine, target);
        }
    }
}

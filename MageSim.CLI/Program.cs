using MageSim.Application.Services;
using MageSim.Application.Simulation;
using MageSim.Infrastructure.Conditions;
using MageSim.Infrastructure.Config;
using MageSim.Infrastructure.Time;
using MageSim.Domain.Skills;

using System;
using System.Threading.Tasks;

namespace MageSim.CLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== MageSim CLI (.NET Framework 4.8.1) ===");
            Console.WriteLine("Komutlar: start | stop | clients | load | exit | macro <fast-rotation|safe-mode|debug>");

            var coord = new Coordinator();
            var configService = new ConfigService("config/mage-config.json");
            var evaluator = new DefaultConditionEvaluator();
            var clock = new SystemClock();

            RootConfig root = null;

            // 🔑 Eventleri CLI’ye yazdır
            coord.OnClientEvent += (id, ev) =>
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {id}: {ev.Type} → {ev.Payload}");
            };

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine()?.Trim().ToLower();

                if (string.IsNullOrEmpty(input)) continue;

                var parts = input.Split(' ');
                var command = parts[0];
                var arg = parts.Length > 1 ? parts[1] : null;

                switch (command)
                {
                    case "start":
                        if (root == null || root.Instances == null || root.Instances.Count == 0)
                        {
                            Console.WriteLine("Önce config yükleyin (load komutu).");
                            break;
                        }

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await coord.StartAllAsync();
                            }
                            catch (TaskCanceledException)
                            {
                                // iptal normal bir durum
                            }
                        });

                        Console.WriteLine("Simülasyon başlatıldı. Durdurmak için bir tuşa basın...");

                        // Kullanıcıdan tuş bekle
                        Console.ReadKey(true);

                        // Tuşa basınca durdur
                        coord.StopAll();
                        Console.WriteLine("Simülasyon durduruldu.");
                        break;

                    case "stop":
                        coord.StopAll();
                        Console.WriteLine("Simülasyon durduruldu.");
                        break;

                    case "clients":
                        foreach (var client in coord.DummyClients)
                            Console.WriteLine($"- DummyClient: {client.Id}");

                        foreach (var engine in coord.RotationEngines)
                            Console.WriteLine($"- RotationEngine ({engine.GetHashCode()})");
                        break;

                    case "load":
                        root = await configService.LoadAsync();
                        coord.StopAll();

                        if (root?.Instances != null && root.Instances.Count > 0)
                        {
                            foreach (var inst in root.Instances)
                            {
                                var dummy = RotationFactory.CreateDummy(inst, evaluator, clock);
                                coord.Add(dummy);

                                // Eğer Ko4Fun entegrasyonu kullanmak istiyorsanız:
                                // var (engine, target) = RotationFactory.CreateKo4Fun(inst, evaluator, clock);
                                // coord.Add(engine, target);
                            }

                            Console.WriteLine("Config yüklendi ve client’lar eklendi.");
                        }
                        else
                        {
                            Console.WriteLine("Config içinde instance bulunamadı.");
                        }
                        break;

                    case "macro":
                        if (root == null)
                        {
                            Console.WriteLine("Önce config yükleyin (load komutu).");
                            break;
                        }

                        coord.StopAll();

                        if (string.IsNullOrWhiteSpace(arg))
                        {
                            Console.WriteLine("Macro parametresi gerekli: fast-rotation | safe-mode | debug");
                            break;
                        }

                        foreach (var inst in root.Instances)
                        {
                            var (engine, target) = RotationFactory.CreateKo4Fun(inst, evaluator, clock);

                            switch (arg)
                            {
                                case "fast-rotation":
                                    engine.Configure(new EngineOptions { SpeedMultiplier = 2.0 });
                                    break;
                                case "safe-mode":
                                    engine.Configure(new EngineOptions { ErrorTolerance = true });
                                    break;
                                case "debug":
                                    engine.Configure(new EngineOptions { VerboseLogging = true });
                                    break;
                                default:
                                    Console.WriteLine($"Bilinmeyen macro: {arg}");
                                    continue; // bu instance’ı atla
                            }

                            coord.Add(engine, target);


                        }

                        _ = Task.Run(async () => await coord.StartAllAsync());
                        Console.WriteLine($"Macro '{arg}' çalıştırıldı.");

                        Console.ReadKey(true);

                        // Tuşa basınca durdur
                        coord.StopAll();
                        Console.WriteLine("Simülasyon durduruldu.");

                        break;

                    case "exit":
                        Console.WriteLine("MageSim CLI kapatılıyor...");
                        return;

                    default:
                        Console.WriteLine("Bilinmeyen komut.");
                        break;
                }
            }
        }
    }
}

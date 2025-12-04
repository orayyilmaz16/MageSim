using MageSim.Application.Services;
using MageSim.Application.Simulation;
using MageSim.Infrastructure.Conditions;
using MageSim.Infrastructure.Config;
using MageSim.Infrastructure.Time;
using System;
using System.Threading.Tasks;

namespace MageSim.CLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== MageSim CLI (.NET Framework 4.8.1) ===");
            Console.WriteLine("Komutlar: start | stop | clients | load | exit");

            var coord = new Coordinator();
            var configService = new ConfigService("config/mage-config.json");
            var evaluator = new DefaultConditionEvaluator();
            var clock = new SystemClock();

            RootConfig root = null;

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine()?.Trim().ToLower();

                switch (input)
                {
                    case "start":
                        if (root == null || root.Instances == null || root.Instances.Count == 0)
                        {
                            Console.WriteLine("Önce config yükleyin (load komutu).");
                            break;
                        }

                        await coord.StartAllAsync(); // arka planda çalıştır
                        Console.WriteLine("Simülasyon başlatıldı.");
                        break;

                    case "stop":
                        coord.StopAll();
                        Console.WriteLine("Simülasyon durduruldu.");
                        break;

                    case "clients":
                        foreach (var client in coord.DummyClients)
                            Console.WriteLine($"- {client.Id}");

                        foreach (var engine in coord.RotationEngines)
                            Console.WriteLine($"- RotationEngine ({engine.GetHashCode()})");
                        break;

                    case "load":
                        root = configService.LoadAsync().Result;
                        coord.StopAll();

                        if (root?.Instances != null)
                        {
                            foreach (var inst in root.Instances)
                            {
                                // Burada hangi client tipini kullanacağınızı seçebilirsiniz:
                                // DummyClient için:
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

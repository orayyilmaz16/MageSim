using System;
using MageSim.Application.Simulation;
using MageSim.Application.Services;
using MageSim.Infrastructure.Config;
using MageSim.Infrastructure.Conditions;
using MageSim.Infrastructure.Time;     // IConditionEvaluator, IClock

namespace MageSim.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== MageSim CLI (.NET Framework 4.8.1) ===");
            Console.WriteLine("Komutlar: start | stop | clients | load | exit");

            var coord = new Coordinator();
            var configService = new ConfigService("config/mage-config.json");
            var evaluator = new DefaultConditionEvaluator(); // ✔ parametresiz
            var clock = new SystemClock();

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine()?.Trim().ToLower();

                switch (input)
                {
                    case "start":
                        coord.StartAllAsync().Wait();
                        Console.WriteLine("Simülasyon başlatıldı.");
                        break;

                    case "stop":
                        coord.StopAll();
                        Console.WriteLine("Simülasyon durduruldu.");
                        break;

                    case "clients":
                        foreach (var client in coord.Clients)
                            Console.WriteLine($"- {client.Id}");
                        break;

                    case "load":
                        var root = configService.LoadAsync().Result;
                        coord.StopAll();
                        if (root?.instances != null)
                        {
                            foreach (var inst in root.instances) // inst artık InstanceConfig tipinde
                            {
                                var client = RotationFactory.Create(inst, evaluator, clock);
                                coord.Add(client);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Config içinde instance bulunamadı.");
                        }


                        Console.WriteLine("Config yüklendi ve client’lar eklendi.");
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
using MageSim.Application.Simulation;
using MageSim.Infrastructure.Conditions;
using MageSim.Infrastructure.Config;
using MageSim.Infrastructure.Time;
using MageSim.Presentation.ViewModels;
using System;
using System.IO;
using System.Windows;

namespace MageSim.Presentation
{
    public partial class App : System.Windows.Application // Fully qualify Application to avoid ambiguity
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "mage-config.json");
            var configService = new ConfigService(configPath);
            var evaluator = new DefaultConditionEvaluator();
            var clock = new SystemClock();
            var coord = new Coordinator();

            var vm = new MainViewModel(configService, coord, evaluator, clock);
            var win = new MageSim.Presentation.Views.MainWindow { DataContext = vm };
            win.Show();
        }
    }
}
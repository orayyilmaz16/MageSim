// MageSim.Presentation/App.xaml.cs
using System;
using System.IO;
using System.Windows;

public partial class App : Application
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
        var win = new Views.MainWindow { DataContext = vm };
        win.Show();
    }
}
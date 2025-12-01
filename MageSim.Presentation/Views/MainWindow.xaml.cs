using System.Windows;
using MageSim.Application.Simulation;
using MageSim.Infrastructure.Conditions;
using MageSim.Infrastructure.Config;
using MageSim.Infrastructure.Time;
using MageSim.Presentation.ViewModels;

namespace MageSim.Presentation.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            
            DataContext = new MainViewModel(
                new ConfigService("config/mage-config.json"),
                new Coordinator(),
                new DefaultConditionEvaluator(),
                new SystemClock()
            );
        }
    }
}

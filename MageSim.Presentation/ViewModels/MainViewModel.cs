using MageSim.Application.Services;
using MageSim.Application.Simulation;
using MageSim.Domain.Abstractions;
using MageSim.Infrastructure.Config;
using MageSim.Presentation.Commands;
using MageSim.Integration; // Ko4Fun entegrasyonu
using MageSim.Domain.Skills;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MageSim.Presentation.ViewModels
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ClientViewModel> Clients { get; } = new ObservableCollection<ClientViewModel>();
        public ObservableCollection<string> Events { get; } = new ObservableCollection<string>();

        public ICommand StartAllCommand { get; }
        public ICommand StopAllCommand { get; }
        public ICommand LoadConfigCommand { get; }
        public ICommand SaveConfigCommand { get; }
        public ICommand StartKo4FunCommand { get; }
        public ICommand StopKo4FunCommand { get; }
        public ICommand RunMacroCommand { get; }

        private readonly ConfigService _config;
        private readonly Coordinator _coord;
        private readonly IConditionEvaluator _evaluator;
        private readonly IClock _clock;

        private RootConfig _root;
        public RootConfig Root
        {
            get => _root;
            private set
            {
                if (_root != value)
                {
                    _root = value;
                    OnPropertyChanged(nameof(Root));
                }
            }
        }

        public MainViewModel(ConfigService config, Coordinator coord, IConditionEvaluator evaluator, IClock clock)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _coord = coord ?? throw new ArgumentNullException(nameof(coord));
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));

            _coord.OnClientEvent += (id, ev) =>
                Events.Add($"[{DateTime.Now:HH:mm:ss}] {id}: {ev.Type} → {ev.Payload}");

          

            StartAllCommand = new RelayCommand(async _ => await SafeStartAsync());
            StopAllCommand = new RelayCommand(_ => Stop());
            LoadConfigCommand = new RelayCommand(async _ => await LoadAsync());
            SaveConfigCommand = new RelayCommand(async _ => await SaveAsync());
            StartKo4FunCommand = new RelayCommand(async _ => await LoadKo4FunAsync());
            StopKo4FunCommand = new RelayCommand(_ => Stop());
            RunMacroCommand = new RelayCommand<string>(async macro => await RunMacroAsync(macro));

           
        }

        /// <summary>
        /// Config yükle ve DummyClient’ları ekle.
        /// </summary>
        private async Task LoadAsync()
        {
            Root = await _config.LoadAsync();
            Clients.Clear();
            _coord.StopAll();

            foreach (var inst in Root.Instances)
            {
                var client = RotationFactory.CreateDummy(inst, _evaluator, _clock);
                _coord.Add(client);

                var vm = new ClientViewModel(inst.Id);
                vm.Bind(client);
                Clients.Add(vm);
            }
        }

        /// <summary>
        /// Config yükle ve Ko4Fun client’ları topluca ekle.
        /// </summary>
        private async Task LoadKo4FunAsync()
        {
            Root = await _config.LoadAsync();
            Clients.Clear();
            _coord.StopAll();

            foreach (var inst in Root.Instances)
            {
                var (engine, target) = RotationFactory.CreateKo4Fun(inst, _evaluator, _clock);
                _coord.Add(engine, target);

                var vm = new ClientViewModel(inst.Id);
                Clients.Add(vm);
            }

            await SafeStartAsync();
        }

        /// <summary>
        /// Macro çalıştırma: belirli senaryolar için toplu client başlatma.
        /// </summary>
        private async Task RunMacroAsync(string macro)
        {
            if (Root == null) return;

            Clients.Clear();
            _coord.StopAll();

            foreach (var inst in Root.Instances)
            {
                var (engine, target) = RotationFactory.CreateKo4Fun(inst, _evaluator, _clock);

                switch (macro?.ToLower())
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
                }

                _coord.Add(engine, target);
                Clients.Add(new ClientViewModel(inst.Id));
            }

            await SafeStartAsync();
            Events.Add($"Macro '{macro}' çalıştırıldı ({Clients.Count} client).");
        }

        private async Task SaveAsync()
        {
            if (Root != null)
                await _config.SaveAsync(Root);
        }

        private async Task SafeStartAsync()
        {
            try
            {
                await _coord.StartAllAsync();
            }
            catch (TaskCanceledException)
            {
                // iptal normal bir durum
            }
        }

        public void Stop()
        {
            _coord.StopAll();
            Clients.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

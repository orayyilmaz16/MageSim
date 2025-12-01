using MageSim.Application.Services;
using MageSim.Application.Simulation;
using MageSim.Domain.Abstractions;
using MageSim.Infrastructure.Config;
using MageSim.Presentation.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MageSim.Presentation.ViewModels
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ClientViewModel> Clients { get; }
            = new ObservableCollection<ClientViewModel>();

        public ObservableCollection<string> Events { get; }
            = new ObservableCollection<string>();

        public ICommand StartAllCommand { get; }
        public ICommand StopAllCommand { get; }
        public ICommand LoadConfigCommand { get; }
        public ICommand SaveConfigCommand { get; }

        private readonly ConfigService _config;
        private readonly Coordinator _coord;
        private readonly IConditionEvaluator _evaluator;
        private readonly IClock _clock;

        private RootConfig _root;
        public RootConfig Root
        {
            get => _root;
            set
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
            _config = config;
            _coord = coord;
            _evaluator = evaluator;
            _clock = clock;

            _coord.OnClientEvent += (id, ev) =>
                Events.Add(string.Format("[{0:HH:mm:ss}] {1}: {2} → {3}",
                    DateTime.Now, id, ev.Type, ev.Payload));

            StartAllCommand = new RelayCommand(() => { var _ = _coord.StartAllAsync(); });
            StopAllCommand = new RelayCommand(() => _coord.StopAll());
            LoadConfigCommand = new RelayCommand(async () => await Load());
            SaveConfigCommand = new RelayCommand(async () => await Save());
        }

        private async Task Load()
        {
            Root = await _config.LoadAsync();   // PropertyChanged tetiklenir
            Clients.Clear();

            foreach (var inst in Root.instances)
            {
                var client = RotationFactory.Create(inst, _evaluator, _clock);
                _coord.Add(client);

                var vm = new ClientViewModel(inst.id);
                vm.Bind(client);
                Clients.Add(vm);
            }
        }

        private async Task Save()
        {
            if (Root == null)
                return;

            await _config.SaveAsync(Root);
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

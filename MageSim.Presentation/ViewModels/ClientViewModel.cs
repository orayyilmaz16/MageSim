using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MageSim.Application.Simulation;
using MageSim.Domain.Events;

namespace MageSim.Presentation.ViewModels
{
    public sealed class ClientViewModel : INotifyPropertyChanged
    {
        public string Id { get; }
        // target-typed new() yerine açık tip
        public ObservableCollection<SkillViewModel> Skills { get; } = new ObservableCollection<SkillViewModel>();

        private int _mana;
        private string _state = "Idle";

        public int Mana
        {
            get { return _mana; }
            private set { _mana = value; OnPropertyChanged(nameof(Mana)); }
        }

        public string State
        {
            get { return _state; }
            private set { _state = value; OnPropertyChanged(nameof(State)); }
        }

        public ClientViewModel(string id) { Id = id; }

        public void Bind(DummyClient client)
        {
            Mana = client.Context.Mana;
            client.Context.OnEvent += ev =>
            {
                if (ev.Type == CombatEventType.StateChange)
                    State = ev.Payload;
                else if (ev.Type == CombatEventType.Cast)
                    Mana = client.Context.Mana;
            };

            foreach (var s in client.Skills)
            {
                Skills.Add(new SkillViewModel
                {
                    Name = s.Name,
                    Key = s.Key,
                    CooldownMs = (int)s.Cooldown.TotalMilliseconds,
                    Mana = s.ManaCost,
                    Condition = s.ConditionDsl
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string n)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(n));
        }
    }
}

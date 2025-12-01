using System;
using System.ComponentModel;

namespace MageSim.Presentation.ViewModels
{
    public sealed class SkillViewModel : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _key = string.Empty;
        private int _cooldownMs;
        private int _mana;
        private string _condition = string.Empty;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Key
        {
            get => _key;
            set
            {
                if (_key != value)
                {
                    _key = value;
                    OnPropertyChanged(nameof(Key));
                }
            }
        }

        public int CooldownMs
        {
            get => _cooldownMs;
            set
            {
                if (_cooldownMs != value)
                {
                    _cooldownMs = value;
                    OnPropertyChanged(nameof(CooldownMs));
                }
            }
        }

        public int Mana
        {
            get => _mana;
            set
            {
                if (_mana != value)
                {
                    _mana = value;
                    OnPropertyChanged(nameof(Mana));
                }
            }
        }

        public string Condition
        {
            get => _condition;
            set
            {
                if (_condition != value)
                {
                    _condition = value;
                    OnPropertyChanged(nameof(Condition));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

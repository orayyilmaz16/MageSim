using MageSim.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MageSim.Application.Simulation
{
    // MageSim.Application/Simulation/Coordinator.cs
    public sealed class Coordinator
    {
        // target-typed new() yerine açık tip kullanımı
        private readonly List<DummyClient> _clients = new List<DummyClient>();
        private CancellationTokenSource _cts;

        // nullable reference tip yerine klasik event tanımı
        public event Action<string, CombatEvent> OnClientEvent;

        public void Add(DummyClient client)
        {
            client.Context.OnEvent += ev =>
            {
                var handler = OnClientEvent;
                if (handler != null)
                {
                    handler(client.Id, ev);
                }
            };
            _clients.Add(client);
        }

        public async Task StartAllAsync()
        {
            _cts = new CancellationTokenSource();
            var tasks = _clients.Select(c => c.StartAsync(_cts.Token));
            await Task.WhenAll(tasks);
        }

        public void StopAll()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts = null;
            }
        }

        public void Broadcast(string message)
        {
            foreach (var c in _clients)
            {
                c.Context.Emit(new CombatEvent(CombatEventType.Warning, message));
            }
        }

        public IEnumerable<DummyClient> Clients
        {
            get { return _clients; }
        }
    }
}
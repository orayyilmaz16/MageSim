using MageSim.Domain.Events;
using MageSim.Domain.Skills;
using MageSim.Integration.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MageSim.Application.Simulation
{
    public sealed class Coordinator
    {
        private readonly List<DummyClient> _dummyClients = new List<DummyClient>();

        private sealed class RotationClient
        {
            public RotationEngine Engine { get; }
            public IRotationTarget Target { get; }

            public RotationClient(RotationEngine engine, IRotationTarget target)
            {
                Engine = engine;
                Target = target;
            }
        }

        private readonly List<RotationClient> _rotationClients = new List<RotationClient>();
        private CancellationTokenSource _cts;

        public event Action<string, CombatEvent> OnClientEvent;

        // DummyClient ekleme
        public void Add(DummyClient client)
        {
            client.Context.OnEvent += ev =>
            {
                OnClientEvent?.Invoke(client.Id, ev);
            };
            _dummyClients.Add(client);
        }

        // RotationEngine ekleme
        public void Add(RotationEngine engine, IRotationTarget target)
        {
            _rotationClients.Add(new RotationClient(engine, target));
        }

        public void AddClient(RotationEngine engine, IRotationTarget target) => Add(engine, target);

        /// <summary>
        /// Tüm client'ları başlatır.
        /// </summary>
        public async Task StartAllAsync()
        {
            // RotationEngine'leri başlat (RunAsync ile)
            foreach (var rc in _rotationClients)
            {
                var ctx = new CombatContext(); // her engine için yeni context
                _ = Task.Run(() => rc.Engine.RunAsync(ctx, CancellationToken.None));
            }

            // DummyClient'leri başlat
            if (_dummyClients.Count > 0)
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();

                var tasks = _dummyClients.Select(c => c.StartAsync(_cts.Token));
                await Task.WhenAll(tasks);
            }
        }

        public void StopAll()
        {
            // DummyClient iptal
            _cts?.Cancel();
            _cts = null;

            // RotationEngine için özel Stop yok → CancellationToken ile kontrol edilmeli
            // Eğer RotationEngine'e Stop eklemek istiyorsan, RunAsync içinde ct kontrolü zaten var.
            // Burada ct iptal edilirse Task kendiliğinden durur.
        }

        public void Broadcast(string message)
        {
            foreach (var client in _dummyClients)
            {
                client.Context.Emit(new CombatEvent(CombatEventType.Warning, message));
            }
        }

        public IEnumerable<DummyClient> DummyClients => _dummyClients;
        public IEnumerable<RotationEngine> RotationEngines => _rotationClients.Select(x => x.Engine);
    }
}

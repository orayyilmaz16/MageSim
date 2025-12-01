using MageSim.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MageSim.Infrastructure.Time
{
    // MageSim.Infrastructure/Time/SystemClock.cs
    public sealed class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public Task Delay(TimeSpan delay, CancellationToken ct) => Task.Delay(delay, ct);
    }

}

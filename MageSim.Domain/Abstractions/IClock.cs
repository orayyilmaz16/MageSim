using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MageSim.Domain.Abstractions
{
    public interface IClock
    {
        DateTime UtcNow { get; }
        Task Delay(TimeSpan delay, CancellationToken ct);
    }

}

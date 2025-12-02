using System;
using System.Threading;
using System.Threading.Tasks;

namespace MageSim.Domain.Abstractions
{
    public interface IClock
    {
        DateTime UtcNow { get;}
        Task Delay(TimeSpan delay, CancellationToken ct);
    }

}

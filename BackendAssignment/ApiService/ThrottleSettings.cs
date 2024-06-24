using System;

namespace CleoAssignment.ApiService;

public class ThrottleSettings
{
    /// <summary>
    /// The interval over which the MaxRequests are tracked and periodically reset. 
    /// </summary>
    public required TimeSpan ThrottleInterval { get; init; }

    /// <summary>
    /// The maximum number of requests allowed in the ThrottleInterval by IP address.
    /// </summary>
    public required int MaxRequestsPerIp { get; init; }

    /// <summary>
    /// The amount of time an IP address is banned for after exceeding the MaxRequests in single ThrottleInterval.
    /// </summary>
    public required TimeSpan BanTimeOut { get; init; }

    /// <summary>
    /// Time from which the first ThrottleInterval starts.
    /// </summary>
    public DateTime IntervalRootUtc => DateTime.UnixEpoch;
}

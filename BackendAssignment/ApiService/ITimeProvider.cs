using System;

namespace CleoAssignment.ApiService;

public interface ITimeProvider
{
    public DateTime UtcNow { get; }
}
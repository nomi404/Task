using System;

namespace CleoAssignment.ApiService;

public static class ApiServiceFactory
{
    public static IApiService<T> CreateApiService<T>(ThrottleSettings throttleSettings,
                                                     IResourceProvider<T> resourceProvider,
                                                     ITimeProvider timeProvider)
    {
        return new ApiService<T>(throttleSettings, resourceProvider, timeProvider);
    }
}

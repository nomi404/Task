using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CleoAssignment.ApiService
{
    public class ApiService<T> : IApiService<T>
    {
        private readonly ThrottleSettings _throttleSettings;
        private readonly IResourceProvider<T> _resourceProvider;
        private readonly ITimeProvider _timeProvider;
        private readonly ConcurrentDictionary<string, T> _cache = new();
        private readonly SemaphoreSlim _updateSemaphore = new(1, 1);
        private readonly ConcurrentDictionary<string, (DateTime LastRequestTime, int RequestCount)> _throttleTracker = new();
        private readonly ConcurrentDictionary<string, DateTime> _bannedIps = new();

        public ApiService(ThrottleSettings throttleSettings,
                                  IResourceProvider<T> resourceProvider,
                                  ITimeProvider timeProvider)
        {
            _throttleSettings = throttleSettings ?? throw new ArgumentNullException(nameof(throttleSettings));
            _resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public Task<GetResponse<T>> GetResource(GetRequest request)
        {
            if (IsBanned(request.IpAddress))
            {
                return Task.FromResult(new GetResponse<T>(false, default, ErrorType.Banned));
            }

            if (IsThrottled(request.IpAddress))
            {
                return Task.FromResult(new GetResponse<T>(false, default, ErrorType.Throttled));
            }

            lock (_cache)
            {
                if (_cache.TryGetValue(request.ResourceId, out var resource))
                {
                    return Task.FromResult(new GetResponse<T>(true, resource, null));
                }

                try
                {
                    resource = _resourceProvider.GetResource(request.ResourceId);
                    _cache[request.ResourceId] = resource;
                    return Task.FromResult(new GetResponse<T>(true, resource, null));
                }
                catch
                {
                    return Task.FromResult(new GetResponse<T>(false, default, ErrorType.ResourceNotFound));
                }
            }
        }



        public async Task<AddOrUpdateResponse> AddOrUpdateResource(AddOrUpdateRequest<T> request)
        {
            if (IsBanned(request.IpAddress))
            {
                return new AddOrUpdateResponse(false, ErrorType.Banned);
            }

            if (IsThrottled(request.IpAddress))
            {
                return new AddOrUpdateResponse(false, ErrorType.Throttled);
            }

            await _updateSemaphore.WaitAsync();
            try
            {
                _resourceProvider.AddOrUpdateResource(request.ResourceId, request.Resource);
                _cache[request.ResourceId] = request.Resource;
                return new AddOrUpdateResponse(true, null);
            }
            catch
            {
                return new AddOrUpdateResponse(false, ErrorType.UpdateFailed);
            }
            finally
            {
                _updateSemaphore.Release();
            }
        }


        private bool IsBanned(string ipAddress)
        {
            if (_bannedIps.TryGetValue(ipAddress, out var banEndTime))
            {
                if (_timeProvider.UtcNow < banEndTime)
                {
                    return true;
                }
                _bannedIps.TryRemove(ipAddress, out _);
            }
            return false;
        }

        private bool IsThrottled(string ipAddress)
        {
            var now = _timeProvider.UtcNow;
            if (!_throttleTracker.TryGetValue(ipAddress, out var entry))
            {
                entry = (now, 0);
            }

            if (now - entry.LastRequestTime > _throttleSettings.ThrottleInterval)
            {
                entry = (now, 0);
            }

            entry.RequestCount++;
            entry.LastRequestTime = now;
            _throttleTracker[ipAddress] = entry;
            if (entry.RequestCount > _throttleSettings.MaxRequestsPerIp)
            {
                _bannedIps[ipAddress] = now + _throttleSettings.BanTimeOut;
                return true;
            }

            return false;
        }

    }
}

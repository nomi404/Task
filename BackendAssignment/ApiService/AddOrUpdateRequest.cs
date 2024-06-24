namespace CleoAssignment.ApiService;

public record AddOrUpdateRequest<T>(string IpAddress, string Email, string ResourceId, T Resource);

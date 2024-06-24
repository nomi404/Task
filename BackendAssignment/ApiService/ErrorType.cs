namespace CleoAssignment.ApiService;

public enum ErrorType
{
    None,
    Throttled,
    Banned,
    ResourceNotFound,
    UpdateFailed,
    UnknownError
}

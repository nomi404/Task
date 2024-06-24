namespace CleoAssignment.ApiService;

public interface IResourceProvider<T>
{
    // usually in real word scenario those would be I/O async methods,
    // but for sake of simplifying the assignment, we will consider them as synchronous
    public T GetResource(string id);

    public void AddOrUpdateResource(string id, T resource);
}

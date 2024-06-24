using System.Threading.Tasks;

namespace CleoAssignment.ApiService;

public interface IApiService<TResource>
{
    Task<GetResponse<TResource>> GetResource(GetRequest request);

    Task<AddOrUpdateResponse> AddOrUpdateResource(AddOrUpdateRequest<TResource> request);
}

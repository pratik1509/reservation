using ProtoDefinitions;

namespace ApiApplication.Infrastructure.Abstractions
{
    public interface IApiClientGrpc
    {
        Task<showListResponse> GetAll();
        Task<showListResponse> Search(string searchText);
        Task<showResponse> GetById(string id);
    }
}
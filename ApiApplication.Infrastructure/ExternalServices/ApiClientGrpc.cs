using ApiApplication.Infrastructure.Abstractions;
using Grpc.Core;
using Grpc.Net.Client;
using ProtoDefinitions;

namespace ApiApplication.Infrastructure.ExternalServices
{
    public class ApiClientGrpc : IApiClientGrpc
    {
        private const string ApiKey = "68e5fbda-9ec9-4858-97b2-4a8349764c63";
        private readonly MoviesApi.MoviesApiClient _client;

        public ApiClientGrpc()
        {
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var channel = GrpcChannel.ForAddress("https://api:443", new GrpcChannelOptions
            {
                HttpHandler = httpHandler
            });

            _client = new MoviesApi.MoviesApiClient(channel);
        }

        private Metadata GetHeaders() => new Metadata { { "X-Apikey", ApiKey } };

        public async Task<showListResponse> GetAll()
        {
            var all = await _client.GetAllAsync(new Empty(), GetHeaders());
            all.Data.TryUnpack<showListResponse>(out var data);
            return data;
        }

        public async Task<showListResponse> Search(string searchText)
        {
            var request = new SearchRequest { Text = searchText };
            var response = await _client.SearchAsync(request, GetHeaders());

            response.Data.TryUnpack<showListResponse>(out var data);
            return data;
        }

        public async Task<showResponse> GetById(string id)
        {
            var request = new IdRequest { Id = id };
            var response = await _client.GetByIdAsync(request, GetHeaders());

            response.Data.TryUnpack<showResponse>(out var data);
            return data;
        }
    }
}

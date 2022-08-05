namespace Cybersource.Data
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Cybersource.Models;
    using Cybersource.Services;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Vtex.Api.Context;

    public class CybersourceRepository : ICybersourceRepository
    {
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IIOServiceContext _context;
        private readonly string _applicationName;

        public CybersourceRepository(IVtexEnvironmentVariableProvider environmentVariableProvider, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IIOServiceContext context)
        {
            this._environmentVariableProvider = environmentVariableProvider ??
                                                throw new ArgumentNullException(nameof(environmentVariableProvider));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                               throw new ArgumentNullException(nameof(clientFactory));

            this._context = context ??
                               throw new ArgumentNullException(nameof(context));

            this._applicationName =
                $"{this._environmentVariableProvider.ApplicationVendor}.{this._environmentVariableProvider.ApplicationName}";
        }

        public async Task<SendAntifraudDataResponse> GetAntifraudData(string id)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://vbase.{this._environmentVariableProvider.Region}.vtex.io/{this._httpContextAccessor.HttpContext.Request.Headers[CybersourceConstants.VTEX_ACCOUNT_HEADER_NAME]}/{this._httpContextAccessor.HttpContext.Request.Headers[CybersourceConstants.HEADER_VTEX_WORKSPACE]}/buckets/{this._applicationName}/{CybersourceConstants.BUCKET_ANTIFRAUD}/files/{id}"),
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[CybersourceConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(CybersourceConstants.AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _context.Vtex.Logger.Warn("GetAntifraudData", null, $"Id '{id}' Not Found.");
                    return null;
                }

                _context.Vtex.Logger.Error("GetAntifraudData", null, 
                "Error", null, 
                new[] 
                { 
                    ("id", id), 
                });
                
                return null;
            }

            SendAntifraudDataResponse antifraudDataResponse =  JsonConvert.DeserializeObject<SendAntifraudDataResponse>(responseContent);
            _context.Vtex.Logger.Debug("GetAntifraudData", null, id, new[] { ("responseContent", responseContent) });

            return antifraudDataResponse;
        }

        public async Task SaveAntifraudData(string id, SendAntifraudDataResponse antifraudDataResponse)
        {
            if (antifraudDataResponse == null)
            {
                antifraudDataResponse = new SendAntifraudDataResponse();
            }

            var jsonSerializedCreatePaymentRequest = JsonConvert.SerializeObject(antifraudDataResponse);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"http://vbase.{this._environmentVariableProvider.Region}.vtex.io/{this._httpContextAccessor.HttpContext.Request.Headers[CybersourceConstants.VTEX_ACCOUNT_HEADER_NAME]}/{this._httpContextAccessor.HttpContext.Request.Headers[CybersourceConstants.HEADER_VTEX_WORKSPACE]}/buckets/{this._applicationName}/{CybersourceConstants.BUCKET_ANTIFRAUD}/files/{id}"),
                Content = new StringContent(jsonSerializedCreatePaymentRequest, Encoding.UTF8, CybersourceConstants.APPLICATION_JSON)
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[CybersourceConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(CybersourceConstants.AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _context.Vtex.Logger.Error("SaveAntifraudData", null, "Did not save Antifraud Data.", null, new[] { ("Id", id), ("AntifraudDataResponse", jsonSerializedCreatePaymentRequest) });
            }
            else
            {
                _context.Vtex.Logger.Debug("SaveAntifraudData", null, "Saved Antifraud Data.", new[] { ("Id", id), ("AntifraudDataResponse", jsonSerializedCreatePaymentRequest) });
            }
        }
    }
}

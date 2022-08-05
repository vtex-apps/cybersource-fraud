using Cybersource.Data;
using Microsoft.AspNetCore.Http;
using Cybersource.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vtex.Api.Context;
using Newtonsoft.Json;

namespace Cybersource.Services
{
    public class VtexApiService : IVtexApiService
    {
        private readonly IIOServiceContext _context;
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _applicationName;

        public VtexApiService(IIOServiceContext context, IVtexEnvironmentVariableProvider environmentVariableProvider, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory)
        {
            this._context = context ??
                            throw new ArgumentNullException(nameof(context));

            this._environmentVariableProvider = environmentVariableProvider ??
                                                throw new ArgumentNullException(nameof(environmentVariableProvider));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                               throw new ArgumentNullException(nameof(clientFactory));

            this._applicationName =
                $"{this._environmentVariableProvider.ApplicationVendor}.{this._environmentVariableProvider.ApplicationName}";
        }

        public async Task<ResponseWrapper> ForwardRequest(string url, string requestBody)
        {
            Console.WriteLine($"ForwardRequest '{url}'");
            ResponseWrapper responseWrapper = null;
            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"http://{url}"),
                    Content = new StringContent(requestBody, Encoding.UTF8, CybersourceConstants.APPLICATION_JSON)
                };

                request.Headers.Add(CybersourceConstants.USE_HTTPS_HEADER_NAME, "true");
                string authToken = this._httpContextAccessor.HttpContext.Request.Headers[CybersourceConstants.HEADER_VTEX_CREDENTIAL];
                if (authToken != null)
                {
                    request.Headers.Add(CybersourceConstants.AUTHORIZATION_HEADER_NAME, authToken);
                    request.Headers.Add(CybersourceConstants.VTEX_ID_HEADER_NAME, authToken);
                    request.Headers.Add(CybersourceConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
                }

                var client = _clientFactory.CreateClient();
                HttpResponseMessage responseMessage = await client.SendAsync(request);
                string responseContent = await responseMessage.Content.ReadAsStringAsync();
                Console.WriteLine($"ForwardRequest [{responseMessage.IsSuccessStatusCode}] {responseContent}");
                responseWrapper = new ResponseWrapper
                {
                    IsSuccess = responseMessage.IsSuccessStatusCode,
                    ResponseText = responseContent
                };

                _context.Vtex.Logger.Debug("ForwardRequest", null, null, new[] { ("url", url), ("requestBody", requestBody), ("responseWrapper", JsonConvert.SerializeObject(responseWrapper)) });
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("ForwardRequest", null, 
                "Error: ", ex, 
                new[] 
                { 
                    ("url", url),
                    ("requestBody", requestBody)
                });
            }

            return responseWrapper;
        }
    }
}

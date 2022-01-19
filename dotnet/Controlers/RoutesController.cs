namespace service.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Vtex.Api.Context;
    using System.IO;
    using Cybersource.Services;
    using Cybersource.Models;
    using Cybersource.Data;

    public class RoutesController : Controller
    {
        private readonly IIOServiceContext _context;
        private readonly IVtexApiService _vtexApiService;

        public RoutesController(IIOServiceContext context, IVtexApiService vtexApiService)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._vtexApiService = vtexApiService ?? throw new ArgumentNullException(nameof(vtexApiService));
        }

        /// <summary>
        /// http://{{providerApiEndpoint}}/transactions
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> SendAntifraudData()
        {
            SendAntifraudDataResponse sendAntifraudDataResponse = null;
            if ("post".Equals(HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                string forwardUrl = $"{HttpContext.Request.Headers[CybersourceConstants.FORWARDED_HOST]}/{CybersourceConstants.MainAppName}/payment-provider/transactions";
                string bodyAsText = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var response = await _vtexApiService.ForwardRequest(forwardUrl, bodyAsText);
                if(response.IsSuccess)
                {
                    sendAntifraudDataResponse = JsonConvert.DeserializeObject<SendAntifraudDataResponse>(response.ResponseText);
                }
            }

            return Json(sendAntifraudDataResponse);
        }

        /// <summary>
        /// http://{{providerApiEndpoint}}/pre-analysis
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> SendAntifraudPreAnalysisData()
        {
            SendAntifraudDataResponse sendAntifraudDataResponse = null;
            if ("post".Equals(HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                string forwardUrl = $"{HttpContext.Request.Headers[CybersourceConstants.FORWARDED_HOST]}/{CybersourceConstants.MainAppName}/payment-provider/pre-analysis";
                string bodyAsText = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var response = await _vtexApiService.ForwardRequest(forwardUrl, bodyAsText);
                if (response.IsSuccess)
                {
                    sendAntifraudDataResponse = JsonConvert.DeserializeObject<SendAntifraudDataResponse>(response.ResponseText);
                }
            }

            return Json(sendAntifraudDataResponse);
        }

        /// <summary>
        /// http://{{providerApiEndpoint}}/transactions/transactions.id
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> GetAntifraudStatus(string transactionId)
        {
            SendAntifraudDataResponse sendAntifraudDataResponse = null;
            if ("post".Equals(HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                string forwardUrl = $"{HttpContext.Request.Headers[CybersourceConstants.FORWARDED_HOST]}/{CybersourceConstants.MainAppName}/payment-provider/transactions/{transactionId}";
                string bodyAsText = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var response = await _vtexApiService.ForwardRequest(forwardUrl, bodyAsText);
                if (response.IsSuccess)
                {
                    sendAntifraudDataResponse = JsonConvert.DeserializeObject<SendAntifraudDataResponse>(response.ResponseText);
                }
            }

            return Json(sendAntifraudDataResponse);
        }

        public string PrintHeaders()
        {
            string headers = "--->>> Headers <<<---\n";
            foreach (var header in HttpContext.Request.Headers)
            {
                headers += $"{header.Key}: {header.Value}\n";
            }
            return headers;
        }
    }
}

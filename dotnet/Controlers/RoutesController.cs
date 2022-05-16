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
    using System.Collections.Generic;
    using System.Diagnostics;

    public class RoutesController : Controller
    {
        private readonly IIOServiceContext _context;
        private readonly IVtexApiService _vtexApiService;
        private readonly ICybersourceRepository _cybersourceRepository;

        public RoutesController(IIOServiceContext context, IVtexApiService vtexApiService, ICybersourceRepository cybersourceRepository)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._vtexApiService = vtexApiService ?? throw new ArgumentNullException(nameof(vtexApiService));
            this._cybersourceRepository = cybersourceRepository ?? throw new ArgumentNullException(nameof(cybersourceRepository));
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
                Stopwatch sw = new Stopwatch();
                sw.Start();
                try
                {
                    string bodyAsText = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                    SendAntifraudDataRequest sendAntifraudDataRequest = JsonConvert.DeserializeObject<SendAntifraudDataRequest>(bodyAsText);
                    string forwardUrl = $"{sendAntifraudDataRequest.Store}.myvtex.com/{CybersourceConstants.MainAppName}/payment-provider/transactions";
                    ResponseWrapper responseWrapper = await _vtexApiService.ForwardRequest(forwardUrl, bodyAsText);
                    if (responseWrapper.IsSuccess)
                    {
                        sendAntifraudDataResponse = JsonConvert.DeserializeObject<SendAntifraudDataResponse>(responseWrapper.ResponseText);
                        await this._cybersourceRepository.SaveAntifraudData(sendAntifraudDataRequest.Id, sendAntifraudDataResponse);
                        sendAntifraudDataResponse.Status = CybersourceConstants.VtexAntifraudStatus.Received;
                    }
                }
                catch(Exception ex)
                {
                    _context.Vtex.Logger.Error("SendAntifraudData", null, "Error forwarding request", ex);
                }

                sw.Stop();
                _context.Vtex.Logger.Debug("SendAntifraudData", null, $"Returning in {sw.ElapsedMilliseconds} ms", new[] { ("sendAntifraudDataResponse", JsonConvert.SerializeObject(sendAntifraudDataResponse)) });
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
                Stopwatch sw = new Stopwatch();
                sw.Start();
                try
                {
                    string bodyAsText = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                    SendAntifraudDataRequest sendAntifraudDataRequest = JsonConvert.DeserializeObject<SendAntifraudDataRequest>(bodyAsText);
                    string forwardUrl = $"{sendAntifraudDataRequest.Store}.myvtex.com/{CybersourceConstants.MainAppName}/payment-provider/pre-analysis";
                    ResponseWrapper responseWrapper = await _vtexApiService.ForwardRequest(forwardUrl, bodyAsText);
                    if(responseWrapper.IsSuccess)
                    {
                        sendAntifraudDataResponse = JsonConvert.DeserializeObject<SendAntifraudDataResponse>(responseWrapper.ResponseText);
                        await this._cybersourceRepository.SaveAntifraudData(sendAntifraudDataRequest.Id, sendAntifraudDataResponse);
                    }
                }
                catch (Exception ex)
                {
                    _context.Vtex.Logger.Error("SendAntifraudPreAnalysisData", null, "Error forwarding request", ex);
                }

                sw.Stop();
                _context.Vtex.Logger.Debug("SendAntifraudPreAnalysisData", null, $"Returning in {sw.ElapsedMilliseconds} ms", new[] { ("sendAntifraudDataResponse", JsonConvert.SerializeObject(sendAntifraudDataResponse)) });
            }

            return Json(sendAntifraudDataResponse);
        }

        /// <summary>
        /// http://{{providerApiEndpoint}}/transactions/transactions.id
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> GetAntifraudStatus(string transactionId)
        {
            SendAntifraudDataResponse getAntifraudStatusResponse = null;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                getAntifraudStatusResponse = await this._cybersourceRepository.GetAntifraudData(transactionId);
                if (getAntifraudStatusResponse == null)
                {
                    getAntifraudStatusResponse = new SendAntifraudDataResponse
                    {
                        Id = transactionId,
                        Status = CybersourceConstants.VtexAntifraudStatus.Undefined
                    };
                }
            }
            catch(Exception ex)
            {
                _context.Vtex.Logger.Error("GetAntifraudStatus", null, $"Error getting antifraud status id:{transactionId}", ex);
            }

            sw.Stop();
            _context.Vtex.Logger.Debug("GetAntifraudStatus", transactionId, $"Returned {getAntifraudStatusResponse.Status} in {sw.Elapsed.TotalMilliseconds} ms ", new[] { ("getAntifraudStatusResponse", JsonConvert.SerializeObject(getAntifraudStatusResponse)) });

            return Json(getAntifraudStatusResponse);
        }

        public JsonResult Manifest()
        {
            Manifest manifest = new Manifest
            {
                CustomFields = new List<CustomField>
                {
                    new CustomField
                    {
                        Name = "Company Name",
                        Type = "text"
                    },
                    new CustomField
                    {
                        Name = "Company Tax Id",
                        Type = "text"
                    }
                }
            };

            return Json(manifest);
        }
    }
}

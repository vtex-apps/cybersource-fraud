using Cybersource.Models;
using System.Threading.Tasks;

namespace Cybersource.Services
{
    public interface IVtexApiService
    {
        Task<ResponseWrapper> ForwardRequest(string url, string requestBody);
    }
}
using Cybersource.Models;
using System.Threading.Tasks;

namespace Cybersource.Data
{
    public interface ICybersourceRepository
    {
        Task<SendAntifraudDataResponse> GetAntifraudData(string id);
        Task SaveAntifraudData(string id, SendAntifraudDataResponse antifraudDataResponse);
    }
}
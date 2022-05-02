using Cybersource.Data;
using Cybersource.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Vtex
{
    public class StartupExtender
    {
        public void ExtendConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ICybersourceRepository, CybersourceRepository>();
            services.AddTransient<IVtexApiService, VtexApiService>();
            services.AddSingleton<IVtexEnvironmentVariableProvider, VtexEnvironmentVariableProvider>();
            services.AddHttpContextAccessor();
            services.AddHttpClient();
        }
    }
}
using Microsoft.Extensions.Configuration;

namespace CRMwithAD.Module.Services
{
    public static class AppSettingsProvider
    {
        public static IConfiguration Configuration { get; private set; }

        public static void Initialize(IConfiguration config)
        {
            Configuration = config;
        }
    }
}

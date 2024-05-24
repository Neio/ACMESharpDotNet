using System.Diagnostics.CodeAnalysis;
using ACMESharp.Enrollment;

namespace ACMEWeb
{
    public static class AcmeExtensions
    {
        public static IServiceCollection AddAcmeHandler(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<AcmeOptions>(config.GetSection("AcmeOptions"));
            services.AddHostedService<AcmeHostedService>();
            
            services.AddSingleton<IStorage, FileStorage>();
            services.AddSingleton<IChallengeProvider, HttpChallaneProvider>();

            return services;
        }

    }
}

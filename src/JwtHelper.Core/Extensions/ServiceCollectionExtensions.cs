using JwtHelper.Core.Abstractions;
using JwtHelper.Core.Types;
using Microsoft.Extensions.DependencyInjection;

namespace JwtHelper.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJwtHelper(
            this IServiceCollection services, TokenOptions tokenOptions) =>
            services.AddTransient<IJwtTokenGenerator>(_ =>
                    new DefaultJwtTokenGenerator(tokenOptions))
                .AddTransient<IJwtTokenValidator, DefaultJwtTokenValidator>();
    }
}
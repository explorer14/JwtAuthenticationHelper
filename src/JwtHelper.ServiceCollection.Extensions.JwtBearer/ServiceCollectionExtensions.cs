using JwtHelper.Core;
using JwtHelper.Core.Abstractions;
using JwtHelper.Core.Extensions;
using JwtHelper.Core.Types;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JwtHelper.ServiceCollection.Extensions.JwtBearer
{
    /// <summary>
    ///   Simple extension class to encapsulate data protection and cookie auth boilerplate.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJwtAuthenticationForAPI(
            this IServiceCollection services,
            TokenOptions tokenOptions)
        {
            if (tokenOptions == null)
            {
                throw new ArgumentNullException(
                    $"{nameof(tokenOptions)} is a required parameter. " +
                    $"Please make sure you've provided a valid instance with the appropriate values configured.");
            }

            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>(serviceProvider =>
                new JwtTokenGenerator(tokenOptions));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            }).
            AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters =
                tokenOptions.ToTokenValidationParams();
            });

            return services;
        }
    }
}
using JwtHelper.Core;
using JwtHelper.Core.Abstractions;
using JwtHelper.Core.Extensions;
using JwtHelper.Core.Types;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace JwtHelper.ServiceCollection.Extensions.Cookies
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJwtAuthenticationWithProtectedCookie(
            this IServiceCollection services,
            TokenOptions tokenOptions,
            string applicationDiscriminator = null,
            AuthUrlOptions authUrlOptions = null)
        {
            if (tokenOptions == null)
            {
                throw new ArgumentNullException(
                    $"{nameof(tokenOptions)} is a required parameter. " +
                    $"Please make sure you've provided a valid instance with the appropriate values configured.");
            }

            var hostingEnvironment = services.BuildServiceProvider()
                .GetService<IHostEnvironment>();
            // The JwtAuthTicketFormat representing the cookie needs an
            // IDataProtector and IDataSerialiser to correctly encrypt/decrypt
            // and serialise/deserialise the payload respectively. This
            // requirement is enforced by ISecureDataFormat interface in ASP.NET
            // Core. Read more about ASP.NET Core Data Protection API here: https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/
            // NB: This is only required if you're using JWT with Cookie based
            // authentication, for cookieless auth (such as with a Web API) the
            // data protection and serialisation dependencies won't be needed.
            // You simply need to set the validation params and add the token
            // generator dependencies and use the right authentication extension below.

            var applicationName = $"{applicationDiscriminator ?? hostingEnvironment.ApplicationName}";

            services.AddDataProtection(options =>
                        options.ApplicationDiscriminator = applicationName)
                    .SetApplicationName(applicationName);

            services.AddScoped<IDataSerializer<AuthenticationTicket>, TicketSerializer>();

            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>(
                serviceProvider =>
                new JwtTokenGenerator(
                    tokenOptions));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme =
                    CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme =
                    CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                // cookie expiration should be set the same as the token expiry
                // (the default is 5 mins). The token generator doesn't provide
                // auto-refresh of an expired token so the user will be logged
                // out the next time they try to access a secured endpoint. They
                // will simply have to re-login and acquire a new token and by
                // extension a new cookie. Perhaps in the future I can add some
                // kind of hooks in the token generator that can let the
                // referencing application know that the token has expired and
                // the developer can then request a new token without the user
                // having to re-login.
                options.ExpireTimeSpan = TimeSpan.FromMinutes(tokenOptions.TokenExpiryInMinutes);

                // Specify the TicketDataFormat to use to validate/create the
                // ASP.NET authentication ticket. Its important that the same
                // validation parameters are passed to this class so that the
                // token validation works correctly. The framework will call the
                // appropriate methods in JwtAuthTicketFormat based on whether
                // the cookie is being sent out or coming in from a previously
                // authenticated user. Please bear in mind that if the incoming
                // token is invalid (may be it was tampered or spoofed) the
                // Unprotect() method in JwtAuthTicketFormat will simply return
                // null and the authentication will fail.
                options.TicketDataFormat = new JwtAuthTicketFormat(
                    tokenOptions.ToTokenValidationParams(),
                    services.BuildServiceProvider()
                        .GetService<IDataSerializer<AuthenticationTicket>>(),
                    services.BuildServiceProvider()
                        .GetDataProtector(new[]
                        {
                            $"{applicationName}-Auth1"
                        }));

                options.LoginPath = authUrlOptions != null ?
                    new PathString(authUrlOptions.LoginPath)
                    : new PathString("/Account/Login");
                options.LogoutPath = authUrlOptions != null ?
                    new PathString(authUrlOptions.LogoutPath)
                    : new PathString("/Account/Logout");
                options.AccessDeniedPath = options.LoginPath;
                options.ReturnUrlParameter = authUrlOptions?.ReturnUrlParameter ?? "returnUrl";
            });

            return services;
        }
    }

    /// <summary>
    ///   A simple structure to store the configured login/logout paths and the
    ///   name of the return url parameter
    /// </summary>
    public sealed class AuthUrlOptions
    {
        /// <summary>
        ///   The login path to redirect the user to incase of unauthenticated
        ///   requests. Default is "/Account/Login"
        /// </summary>
        public string LoginPath { get; set; }

        /// <summary>
        ///   The path to redirect the user to once they have logged out.
        ///   Default is "/Account/Logout"
        /// </summary>
        public string LogoutPath { get; set; }

        /// <summary>
        ///   The path to redirect the user to following a successful
        ///   authentication attempt. Default is "returnUrl"
        /// </summary>
        public string ReturnUrlParameter { get; set; }
    }
}
using JwtAuthenticationHelper;
using JwtAuthenticationHelper.Abstractions;
using JwtAuthenticationHelper.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace JwtTokenAuthRefImplementation.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }

        public IHostingEnvironment @Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // retrieve the configured token params and establish a TokenValidationParameters object,
            // we are going to need this later.
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["Token:SigningKey"]));
            var validationParams = new TokenValidationParameters()
            {
                ClockSkew = TimeSpan.Zero,

                ValidateAudience = true,
                ValidAudience = Configuration["Token:Audience"],

                ValidateIssuer = true,
                ValidIssuer = Configuration["Token:Issuer"],

                IssuerSigningKey = signingKey,
                ValidateIssuerSigningKey = true,

                RequireExpirationTime = true,
                ValidateLifetime = true
            };

            // The JwtAuthTicketFormat representing the cookie needs an IDataProtector and
            // IDataSerialiser to correctly encrypt/decrypt and serialise/deserialise the payload
            // respectively. This requirement is enforced by ISecureDataFormat interface in ASP.NET
            // Core. Read more about ASP.NET Core Data Protection API here: https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/
            // NB: This is only required if you're using JWT with Cookie based authentication, for
            //     cookieless auth (such as with a Web API) the data protection and serialisation
            //     dependencies won't be needed. You simply need to set the validation params and add
            //     the token generator dependencies and use the right authentication extension below.
            services.AddDataProtection(options => options.ApplicationDiscriminator = $"{Environment.ApplicationName}")
                .SetApplicationName($"{Environment.ApplicationName}");

            services.AddScoped<IDataSerializer<AuthenticationTicket>, TicketSerializer>();

            // Now add the IJwtTokenGenerator dependency passing in the same validation parameters as
            // set up at the beginning.
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>(serviceProvider =>
                new JwtTokenGenerator(validationParams.ToTokenOptions()));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                // cookie expiration should be set the same as the token expiry (the default is 5
                // mins). The token generator doesn't provide auto-refresh of an expired token so the
                // user will be logged out the next time they try to access a secured endpoint. They
                // will simply have to re-login and acquire a new token and by extension a new cookie.
                // Perhaps in the future I can add some kind of hooks in the token generator that can
                // let the referencing application know that the token has expired and the developer
                // can then request a new token without the user having to re-login.
                options.Cookie.Expiration = TimeSpan.FromMinutes(5);
                // Specify the TicketDataFormat to use to validate/create the ASP.NET authentication
                // ticket. Its important that the same validation parameters are passed to this class
                // so that the token validation works correctly. The framework will call the
                // appropriate methods in JwtAuthTicketFormat based on whether the cookie is being
                // sent out or coming in from a previously authenticated user. Please bear in mind
                // that if the incoming token is invalid (may be it was tampered or spoofed) the
                // Unprotect() method in JwtAuthTicketFormat will simply return null and the
                // authentication will fail.
                options.TicketDataFormat = new JwtAuthTicketFormat(validationParams,
                    services.BuildServiceProvider().GetService<IDataSerializer<AuthenticationTicket>>(),
                    services.BuildServiceProvider().GetDataProtector(new[] { $"{Environment.ApplicationName}-Auth1" }));

                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = options.LoginPath;
                options.ReturnUrlParameter = "returnUrl";
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
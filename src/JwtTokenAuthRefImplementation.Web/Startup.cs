﻿using JwtAuthenticationHelper.Extensions;
using JwtAuthenticationHelper.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JwtTokenAuthRefImplementation.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var tokenOptions = new TokenOptions(
                Configuration["Token:Audience"],
                Configuration["Token:Issuer"],
                Configuration["Token:SigningKey"]);

            services.AddJwtAuthenticationWithProtectedCookie(
                tokenOptions);
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequiresAdmin",
                    policy =>
                    policy.RequireClaim("HasAdminRights"));
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
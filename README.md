# JwtAuthenticationHelper

A simple Json Web Token authentication helper library that allows you to generate access tokens easily for both ASP.NET Core web apps (cookie based auth) and Web APIs (stateless token based auth).

# Intended Usage

In the sample application I have only used the library to generate JWTs for use in cookie authentication but the same generator class can be used to generate tokens for Web APIs as well in a cookieless scenario.

# Setup

## Add the parameters needed for token generation and validations in the appSettings.json file:

"Token": {
  "Issuer": "Token.WebApp",
  "Audience": "Token.WebApp.Clients",
  "SigningKey": "f47b558d-7654-458c-99f2-13b190ef0199"
}

The signing key can be any sufficiently unique minimum 23-character long string (GUIDs are a fine candidate). 

DO NOT expose this key outside of the server, best practice would be to store this key securely (for e.g. in Azure Key Vault or a similar service)

Startup.cs:

## Enable authentication

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    ...
    
    app.UseAuthentication();

    app.UseMvc(routes =>
    {
        routes.MapRoute(
            name: "default",
            template: "{controller=Home}/{action=Index}/{id?}");
    });
}

## Add required services:

public void ConfigureServices(IServiceCollection services)
{            
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
    
    services.AddDataProtection(options => options.ApplicationDiscriminator = $"{Environment.ApplicationName}")
        .SetApplicationName($"{Environment.ApplicationName}");
    services.AddScoped<IDataSerializer<AuthenticationTicket>, TicketSerializer>();

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
        options.Cookie.Expiration = TimeSpan.FromMinutes(5);     
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

## Add the appropriate dependencies in the auth controller:

Please see the AccountController.cs file in the reference project to see how the token generator is used to issue access tokens after a successful authentication attempt. The issued JWT will contain all the claims about the user
in addition to the default JWT claims. For subsequent requests, you will be able to extract the user info (like username, first name and last name) from this token and that information will be available via the HttpContext.User
property which is the point of using a token based approach to authentication i.e. no server side state.
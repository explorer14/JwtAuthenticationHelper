# JwtAuthenticationHelper

A simple Json Web Token authentication helper library that allows you to generate access tokens easily for both ASP.NET Core web apps (cookie based auth) and Web APIs (stateless token based auth). 

## NB: 

This library is not yet available as a nuget package, I am working on that. In the mean time just clone the repo, build from source and add as either a project reference or create your own nuget package for your private nuget feed.

# Intended Usage

In the sample application I have only used the library to generate JWTs for use in cookie authentication but the same generator class can be used to generate tokens for Web APIs as well in a cookieless scenario.

# Setup

## Add the parameters needed for token generation and validations in the appSettings.json file:

"Token": {
  "Issuer": "Token.WebApp",
  "Audience": "Token.WebApp.Clients",
  "SigningKey": "f47b558d-7654-458c-99f2-13b190ef0199"
}

The signing key can be any sufficiently unique minimum 23-character long string (GUIDs are a fine candidate). The issuer and audience can be any string that makes sense in the context of your application, I just use the same names
as my application for issuer and for audience I just append the word "Clients". If you come up with a better scheme let me know.

DO NOT expose this key outside of the server, best practice would be to store this key securely (for e.g. in Azure Key Vault or a similar service)

## Startup.cs:

### Enable authentication

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

### Add required services:

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
    
	// the custom ticket format used later will require an IDataProtector and an IDataSerializer to able to 
	// properly secure and read the authentication ticket, so add these dependencies here.
    services.AddDataProtection(options => options.ApplicationDiscriminator = $"{Environment.ApplicationName}")
        .SetApplicationName($"{Environment.ApplicationName}");
    services.AddScoped<IDataSerializer<AuthenticationTicket>, TicketSerializer>();

	// Add the IJwtTokenGenerator dependency here passing the token options (extension method is included in the library for convenience)
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
		// I would probably set the cookie to expire at the same time as the token which is 5 minutes by default
        options.Cookie.Expiration = TimeSpan.FromMinutes(5);     
		// provide our ticket data format for the cookie auth system to use.
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

### Add the appropriate dependencies in the auth controller:

Please see the AccountController.cs file in the reference project to see how the token generator is used to issue access tokens after a successful authentication attempt. You can pass in custom claims to the JWT generator whcih it will add to the set of default JWT claims. For subsequent requests, you will then be able to extract the user info (like username, first name and last name and any claims) from this token and that information will be available via the HttpContext.User property which is the point of using a token based approach to authentication i.e. no server side state.
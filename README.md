# JwtAuthenticationHelper

A simple Json Web Token authentication helper library to generate access tokens easily and reusably for both ASP.NET Core web apps (cookie based auth) and Web APIs (stateless token based auth). 

## NB: 

This library is not yet available as a nuget package. In the mean time just clone the repo, build from source and add as either a project reference or create your own nuget package for your private nuget feed.

# Intended Usage

In the sample application I have only used the library to generate JWTs for use in cookie authentication but the same generator class can be used to generate tokens for Web APIs as well in a cookieless scenario.

# Setup

### In version 2.1 a new convenience extension method has been introduced that encapsulates all the data protection and cookie auth boiler plate so that the developer only needs to call this single extension method in their Startup.cs::ConfigureServices method to add JWT Auth helper and use the IJwtTokenGenerator in the controllers. This extension method takes the following parameters:

a) A mandatory instance of TokenValidationParameters. If its null, an exception will be thrown.

b) Application discriminator string to be used by data protection API internally to keep the encryption keys isolated per application. If no value is passed in then by default it will use IHostingEnvironment.ApplicationName.

c) Instance of AuthUrlOptions class that can be used to specify the login/logout path and the returnUrl parameter name. The defaults are the same as that of out of the box ASP.NET Core MVC cookie authentication i.e.
login path = "/Account/Login", logout path = "/Account/Logout" and return url param = "returnUrl".

## Add the parameters needed for token generation and validations in the appSettings.json file:
```
"Token": {
  "Issuer": "Token.WebApp",
  "Audience": "Token.WebApp.Clients",
  "SigningKey": "f47b558d-7654-458c-99f2-13b190ef0199"
}
```

The signing key can be any sufficiently unique minimum 23-character long string (GUIDs are a fine candidate). The issuer and audience can be any string that makes sense in the context of your application, I just use the same names
as my application for issuer and for audience I just append the word "Clients". If you come up with a better scheme let me know.

DO NOT expose this key outside of the server, best practice would be to store this key securely (for e.g. in Azure Key Vault or a similar service)

## Startup.cs:

### Enable authentication
```
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{        
    app.UseAuthentication();

    app.UseMvc(routes =>
    {
        routes.MapRoute(
            name: "default",
            template: "{controller=Home}/{action=Index}/{id?}");
    });
}
```
### Add Jwt Auth service via the new extension method (NEW):
```
public void ConfigureServices(IServiceCollection services)
{            
	var validationParams = new TokenValidationParameters
    {
        ClockSkew = TimeSpan.Zero,

        ValidateAudience = true,
        ValidAudience = Configuration["Token:Audience"],

        ValidateIssuer = true,
        ValidIssuer = Configuration["Token:Issuer"],

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["Token:SigningKey"])),
        ValidateIssuerSigningKey = true,

        RequireExpirationTime = true,
        ValidateLifetime = true
    };
			
    services.AddJwtAuthenticationWithProtectedCookie(validationParams);

    services.AddMvc();
}
```

You can still add all the boilerplate manually if you need to tweak it further for your purposes. This convenience extension method is just a quick way to get up and running with reasonable defaults.

### Add the appropriate dependencies in the auth controller:
Please see the AccountController.cs file in the reference project to see how the token generator is used to issue access tokens after a successful authentication attempt. You can pass in custom claims to the JWT generator which it will add to the set of default JWT claims. For subsequent requests, you will then be able to extract the user info (like username, first name and last name and any claims) from this token and that information will be available via the HttpContext.User property which is the point of using a token based approach to authentication i.e. no server side state.
```
string firstName = httpContext.User?.FindFirst(ClaimTypes.GivenName).Value;
string lastName = httpContext.User?.FindFirst(ClaimTypes.Surname).Value;
string email = httpContext.User?.Identity.Name;
```

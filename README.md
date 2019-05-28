# JwtAuthenticationHelper

A simple Json Web Token authentication helper library to generate access tokens easily and reusably for both ASP.NET Core web apps (cookie based auth) and Web APIs (stateless token based auth). This library exposes the JWT auth as a middleware that can be plugged into ASP.NET Core pipeline like other middleware.

## NB: 

This library is not yet available as a nuget package. In the mean time just clone the repo, build from source and add as either a project reference or create your own nuget package for your private nuget feed.

# Intended Usage

In the sample application I have only used the library to generate JWTs for use in cookie authentication but the same generator class can be used to generate tokens for Web APIs as well in a cookieless scenario.

# Setup

## BREAKING CHANGES:
In version 3.0.0, the middleware bootstrapping has been simplified so that the `TokenValidationParameters` instance is no longer required to be provided by consumers, instead a lighter weight `TokenOptions` instance that can be hydrated from `appsettings.json`, has been introduced. This does make it a breaking change, if you have forked this repo, please make sure you rebase the changes onto your forks.

Below is a quick how-to for the library/package:

1. Add the parameters needed for token generation and validations in the appSettings.json file (`TokenOptions` class is the similar structure):

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

2. Enable authentication

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
3. Add Jwt Auth service via the updated extension method (NEW):

```
public void ConfigureServices(IServiceCollection services)
{            
	var tokenOptions = new TokenOptions(
                Configuration["Token:Audience"],
                Configuration["Token:Issuer"],
                Configuration["Token:SigningKey"]);

    services.AddJwtAuthenticationWithProtectedCookie(
                tokenOptions);

    services.AddMvc();
}
```

You can still add all the boilerplate manually if you need to tweak it further for your purposes. This convenience extension method is just a quick way to get up and running with reasonable defaults.

4. Add the appropriate dependencies in the auth controller:

Please see the AccountController.cs file in the reference project to see how the token generator is used to issue access tokens after a successful authentication attempt. You can pass in custom claims to the JWT generator which it will add to the set of default JWT claims. For subsequent requests, you will then be able to extract the user info (like username, first name and last name and any claims) from this token and that information will be available via the HttpContext.User property which is the point of using a token based approach to authentication i.e. no server side state.

```
string firstName = httpContext.User?.FindFirst(ClaimTypes.GivenName).Value;
string lastName = httpContext.User?.FindFirst(ClaimTypes.Surname).Value;
string email = httpContext.User?.Identity.Name;
```

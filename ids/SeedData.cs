using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ids
{
    public class SeedData
    {
        public static void EnsureSeedData(WebApplication app)
        {
            using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetService<PersistedGrantDbContext>().Database.Migrate();
                var context = scope.ServiceProvider.GetService<ConfigurationDbContext>();
                context.Database.Migrate();
                EnsureSeedData(context);
                EnsureUsers(scope);
            }
        }
        private static void EnsureSeedData(ConfigurationDbContext context)
        {
            //CLIENTS
            if (!context.Clients.Any())
            {
                foreach (var client in ConfigIDS.Clients.ToList())
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }
            //IDENTITY RESOURCES
            if (!context.IdentityResources.Any())
            {
                foreach (var resource in ConfigIDS.IdentityResources.ToList())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            //API SCOPES
            if (!context.ApiScopes.Any())
            {
                foreach (var resource in ConfigIDS.ApiScopes.ToList())
                {
                    context.ApiScopes.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            //API RESOURCES
            if (!context.ApiResources.Any())
            {
                foreach (var resource in ConfigIDS.ApiResources.ToList())
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            //IDENTITY PROVIDERS
            if (!context.IdentityProviders.Any())
            {
                context.IdentityProviders.Add(new OidcProvider
                {
                    Scheme = "demoidsrv",
                    DisplayName = "IdentityServer",
                    Authority = "https://demo.duendesoftware.com",
                    ClientId = "login"
                }.ToEntity());
                context.SaveChanges();
            }
        }

        private static void EnsureUsers(IServiceScope scope)
        {
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var alice = userMgr.FindByNameAsync("alice").Result;
            if (alice == null)
            {
                alice = new IdentityUser
                {
                    UserName = "alice",
                    Email = "alice@gmail.com",
                    EmailConfirmed = true,
                };
                var result = userMgr.CreateAsync(alice, "Pass123!").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                result = userMgr.AddClaimsAsync(alice, new Claim[]
                {
                    new Claim(JwtClaimTypes.Name, "Alice Smith"),
                    new Claim(JwtClaimTypes.GivenName,"Alice"),
                    new Claim(JwtClaimTypes.FamilyName,"Smith"),
                    new Claim(JwtClaimTypes.WebSite, "http:alice.com"),
                }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
            }
            var bob = userMgr.FindByNameAsync("bob").Result;
            if (bob == null)
            {
                bob = new IdentityUser
                {
                    UserName = "bob",
                    Email = "BobSmith@email.com",
                    EmailConfirmed = true
                };
                var result = userMgr.CreateAsync(bob, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                result = userMgr.AddClaimsAsync(bob, new Claim[]
                {
                     new Claim(JwtClaimTypes.Name, "Bob Smith"),
                     new Claim(JwtClaimTypes.GivenName, "Bob"),
                     new Claim(JwtClaimTypes.FamilyName, "Smith"),
                     new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                     new Claim("location", "somewhere")
                }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
            }
        }
    }
}

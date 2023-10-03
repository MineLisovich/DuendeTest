using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using IdentityModel;
using System.Security.Claims;
using System.Text.Json;
using static Duende.IdentityServer.Models.IdentityResources;

namespace ids
{
    public class ConfigIDS
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource
                {
                    Name = "role",
                    UserClaims = new List<string> {"role"}
                }
            };
        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("weatherApiResurs.read"),
                new ApiScope("weatherApiResurs.write")
            };
        public static IEnumerable<ApiResource> ApiResources =>
            new ApiResource[]
            {
                new ApiResource("weatherApiResurs")
                {
                    Scopes = new List<string> { "weatherApiResurs.read", "weatherApiResurs.write" },
                    ApiSecrets = new List<Secret> {new Secret("ScopeSecret".Sha256())},
                    UserClaims = new List<string>{"role"}
                }
            };
        public static IEnumerable<Client> Clients =>
            new[]
            {
                //m2m client
                new Client
                {
                    ClientId = "m2m.client",
                    ClientName = "Clientishe",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = {new Secret("SuperSecretPassword".Sha256())},
                    AllowedScopes = { "weatherApiResurs.read", "weatherApiResurs.write" }
                },
                // interactive client using code flow + pkce
                new Client
                {
                    ClientId = "interactive",
                    ClientSecrets = {new Secret("SuperSecretPassword".Sha256())},
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = {"https://localhost:5444/signin-oidc"},
                    FrontChannelLogoutUri = "https://localhost:5444/signout-oidc",
                    PostLogoutRedirectUris = {"https://localhost:5444/signout-callback-oidc"},
                    AllowOfflineAccess = true,
                    AllowedScopes = {"openid", "profile", "weatherApiResurs.read"},
                    RequirePkce = true,
                    RequireConsent = false,
                    AllowPlainTextPkce = false
                }
            };
    }
}

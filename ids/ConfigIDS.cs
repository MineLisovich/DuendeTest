using Duende.IdentityServer.Models;


namespace ids
{
    public class ConfigIDS
    {
        //IdentityResource - позволяет смоделировать область которая позволит клиентскому приложению
        //просматривать под множеством утверждений(claim) о пользователе
        //Profile() - позволяет видеть клиентскому приложению утверждение(claim) о пользователе (имя, фамилия и тд.)
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
        //ApiScope - Области которые может использовать клиентское приложение
        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("weatherApiResurs.read"),
                new ApiScope("weatherApiResurs.write")
            };
        //ApiResource - позволяет смоделировать доступ ко всему защищённому ресурсу.
        //API с отдельными уровнями разрешений(облостями) к которым клиентское приложение может запросить доступ
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
        //Client - IdentityServer нужно знать каким клиентским приложениям можно использовать его (список приложений)
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

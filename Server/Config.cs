namespace Server
{
    using IdentityModel;
    using IdentityServer4.Models;

    public class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources() => new List<IdentityResource>()
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

        public static IEnumerable<ApiScope> ApiScopes =>
            new[]
            {
                new ApiScope("CoffeeAPI"), 
            };


        public static IEnumerable<Client> Clients =>
            new[]
            {

                new Client
                {
                    ClientId = "interactive",
                    ClientSecrets = {new Secret("ClientSecret1".ToSha256())},
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = {"https://localhost:5444/signin-oidc"},
                    FrontChannelLogoutUri = "https://localhost:5444/signout-oidc",
                    PostLogoutRedirectUris = { "https://localhost:5444/signout-callback-oidc" },
                    AllowOfflineAccess = true,
                    AllowedScopes = {"openid", "profile", "CoffeeAPI"},
                    RequireConsent = false,
                },
            };
    }
}

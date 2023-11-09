namespace WebClient.Services.Interfaces
{
    using IdentityModel.Client;

    public interface ITokenService
    {
        Task<TokenResponse> GetTokenAsync(string scope);
    }
}

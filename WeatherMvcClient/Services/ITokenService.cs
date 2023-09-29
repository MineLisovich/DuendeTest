using IdentityModel.Client;

namespace WeatherMvcClient.Services
{
    public interface ITokenService
    {
        Task<TokenResponse> GetTokenAsync(string scope);
    }
}

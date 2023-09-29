using IdentityModel.Client;
using Microsoft.Extensions.Options;

namespace WeatherMvcClient.Services
{
    public class TokenService : ITokenService
    {
        private readonly ILogger<TokenService> _logger;
        private readonly IOptions<IdentityServerSettings> _options;
        private readonly DiscoveryDocumentResponse _discoveryDocument;

        public TokenService(ILogger<TokenService> logger, IOptions<IdentityServerSettings> options)
        {
            _logger = logger;
            _options = options;
            
            using var httpClient = new HttpClient();
            _discoveryDocument = httpClient.GetDiscoveryDocumentAsync(_options.Value.DiscoveryUrl).Result;
            if (_discoveryDocument.IsError)
            {
                _logger.LogError($"Error to get discovery document. Error is: {_discoveryDocument.Error}");
                throw new Exception("Error to get discovery document", _discoveryDocument.Exception);
            }
        }

        public async Task<TokenResponse> GetTokenAsync(string scope)
        {
            using var httpClient = new HttpClient();
            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = _discoveryDocument.TokenEndpoint,
                ClientId = _options.Value.ClientName,
                ClientSecret = _options.Value.ClientPassword,
                Scope = scope
            });

            if (tokenResponse.IsError)
            {
                _logger.LogError($"Error to get token {tokenResponse.Error}");
                throw new Exception("Error to get token ", tokenResponse.Exception);
            }
            return tokenResponse;
        }
    }
}

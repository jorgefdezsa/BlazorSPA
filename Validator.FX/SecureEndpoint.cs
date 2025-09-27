namespace Validator.FX
{
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net;
    using System.Text.Json;

    public class SecureEndpoint
    {
        private readonly ILogger<SecureEndpoint> _logger;
        private readonly IConfiguration _config;

        private const string AuthorizationHeader = "Authorization";
        private const string BearerPrefix = "Bearer ";

        public SecureEndpoint(ILogger<SecureEndpoint> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        [Function("SecureFunction")]
        public async Task<HttpResponseData> Run(
                  [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
                  FunctionContext context)
        {
            var response = req.CreateResponse();

            var token = ExtractBearerToken(req);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Encabezado Authorization no presente o mal formado.");
                response.StatusCode = HttpStatusCode.Unauthorized;
                await response.WriteStringAsync("Encabezado Authorization no presente o mal formado.");
                return response;
            }

            var tenantId = _config["AzureAd:TenantId"];
            var audience = _config["AzureAd:Audience"];

            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(audience))
            {
                _logger.LogError("Configuración faltante: AzureAd:TenantId o AzureAd:Audience.");
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync("Configuración de AzureAd incompleta.");
                return response;
            }

            var validationParams = new TokenValidationParameters
            {
                ValidAudience = audience,
                ValidIssuer = $"https://login.microsoftonline.com/{tenantId}/v2.0",
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                IssuerSigningKeys = await GetSigningKeysAsync(tenantId)
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, validationParams, out var validatedToken);

                var claims = principal.Claims.ToDictionary(c => c.Type, c => c.Value);
                var json = JsonSerializer.Serialize(new
                {
                    Usuario = claims.TryGetValue("name", out var name) ? name : principal.Identity?.Name,
                    Claims = claims
                }, new JsonSerializerOptions { WriteIndented = true });

                _logger.LogInformation("Token válido para usuario: {Usuario}", claims["preferred_username"]);
                response.StatusCode = HttpStatusCode.OK;
                await response.WriteStringAsync(json);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning("Token inválido: {Error}", ex.Message);
                response.StatusCode = HttpStatusCode.Unauthorized;
                await response.WriteStringAsync($"Token inválido: {ex.Message}");
            }

            return response;
        }


        private static string? ExtractBearerToken(HttpRequestData req)
        {
            if (req.Headers.TryGetValues(AuthorizationHeader, out var values))
            {
                var raw = values.FirstOrDefault();
                if (!string.IsNullOrEmpty(raw) && raw.StartsWith(BearerPrefix))
                {
                    return raw.Substring(BearerPrefix.Length);
                }
            }
            return null;
        }

        private static async Task<IEnumerable<SecurityKey>> GetSigningKeysAsync(string tenantId)
        {
            var configManager = new Microsoft.IdentityModel.Protocols.ConfigurationManager<OpenIdConnectConfiguration>(
                $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever());

            var config = await configManager.GetConfigurationAsync(CancellationToken.None);
            return config.SigningKeys;
        }
    }
}

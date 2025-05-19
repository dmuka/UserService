using Bitwarden.Sdk;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Vault;

public class SecretsConfigurationProvider(
    string identityUrl,
    string apiUrl,
    string accessToken, 
    string organizationId)
    : ConfigurationProvider
{
    public override void Load()
    {
        var secrets = LoadSecretsAsync();
            
        foreach (var secret in secrets)
        {
            var configKey = secret.Key.Replace(":", "__").Replace("-", "_");
            Data[configKey] = secret.Value;
        }
    }

    private List<SecretResponse> LoadSecretsAsync()
    {
        try
        {
            var bitwardenSettings = new BitwardenSettings
            {
                IdentityUrl = identityUrl,
                ApiUrl = apiUrl
            };

            using var client = new BitwardenClient(bitwardenSettings);

            client.AccessTokenLogin(accessToken);
                
            var secrets = client.Secrets.List(Guid.Parse(organizationId));

            return secrets.Data.Select(secret => client.Secrets.Get(secret.Id)).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load secrets from Secrets Manager.", ex);
        }
    }
}

public class SecretsConfigurationSource : IConfigurationSource
{
    public string IdentityUrl { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new SecretsConfigurationProvider(IdentityUrl, ApiUrl, AccessToken, OrganizationId);
    }
}
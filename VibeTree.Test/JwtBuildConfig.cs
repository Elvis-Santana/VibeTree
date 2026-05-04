using Microsoft.Extensions.Configuration;

namespace VibeTree.Test;

public static class JwtBuildConfig
{

    public static IConfiguration BuildConfig(
        string secret = "chave_de_teste_muito_longa_e_secreta_12345",
        string issuer = "http://localhost"
    )
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"JwtSettings:secret_key", secret},
             {"JwtSettings:Issuer", issuer}
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings )
            .Build();
    }
}
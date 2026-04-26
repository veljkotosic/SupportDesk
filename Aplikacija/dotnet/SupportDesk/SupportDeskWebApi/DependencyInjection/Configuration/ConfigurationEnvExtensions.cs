namespace SupportDeskWebApi.DependencyInjection.Configuration;

public static class ConfigurationEnvExtensions
{
    private const string DbConnStringKey = "DB_CONN_STRING";
    
    private const string JwtIssuer = "JWT_ISSUER";
    private const string JwtAudience = "JWT_AUDIENCE";
    private const string JwtKey = "JWT_KEY";
    private const string JwtExpirationMinutes = "JWT_EXPIRATION_MINUTES";
    private const string JwtRefreshExpirationDays = "JWT_REFRESH_TOKEN_EXPIRATION_DAYS";

    extension(IConfiguration configuration)
    {
        public string GetEnvConnectionString()
        {
            return configuration.GetEnvForSure(DbConnStringKey);
        }

        public string GetEnvJwtIssuer()
        {
            return configuration.GetEnvForSure(JwtIssuer);
        }

        public string GetEnvJwtAudience()
        {
            return configuration.GetEnvForSure(JwtAudience);
        }

        public string GetEnvJwtKey()
        {
            return configuration.GetEnvForSure(JwtKey);
        }

        public int GetEnvJwtExpirationMinutes()
        {
            return configuration.GetEnvInt(configuration.GetEnvOrDefault(JwtExpirationMinutes, "60"));
        }

        public int GetEnvJwtRefreshExpirationDays()
        {
            return configuration.GetEnvInt(configuration.GetEnvOrDefault(JwtRefreshExpirationDays, "7"));
        }

        private string GetEnvForSure(string key)
        {
            var value = configuration[key];

            if (value is null)
            {
                throw new ConfigurationException(key);
            }
        
            return value;
        }
        
        private string GetEnvOrDefault(string key, string defaultValue)
        {
            return configuration.GetValue(key, defaultValue);
        }
        
        private int GetEnvInt(string value)
        {
            if (int.TryParse(value, out var result))
            {
                return result;
            }
            else
            {
                throw new ConfigurationException($"Value '{value}' is not a valid integer.");
            }
        }
    }
}
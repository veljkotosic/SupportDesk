namespace SupportDesk.Infrastructure.DependencyInjection.Configuration;

public sealed class ConfigurationException : Exception
{
    public ConfigurationException(string key)
        : base($"Env configuration error, key '{key}' not set.")
    {
    }
}
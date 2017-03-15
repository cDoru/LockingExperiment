using System;
using System.Configuration;
using LockingWebApp.Locks.Contracts;

namespace LockingWebApp.Locks.Configuration
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        public string GetConfigurationValue(string key)
        {
            switch (key)
            {
                case ConfigurationKeys.DbConnection:
                {
                    return ConfigurationManager.AppSettings[ConfigurationKeys.DbConnection];
                }
                case ConfigurationKeys.EncryptKey:
                {
                    return ConfigurationManager.AppSettings[ConfigurationKeys.EncryptKey];
                }
                case ConfigurationKeys.EncryptVector:
                {
                    return ConfigurationManager.AppSettings[ConfigurationKeys.EncryptVector];
                }
                default:
                    throw new InvalidOperationException("Configuration key not handled");
            }
        }
    }
}
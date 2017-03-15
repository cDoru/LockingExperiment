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
                default:
                    throw new InvalidOperationException("Configuration key not handled");
            }
        }
    }
}
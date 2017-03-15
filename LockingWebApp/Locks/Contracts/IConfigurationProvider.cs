namespace LockingWebApp.Locks.Contracts
{
    public interface IConfigurationProvider
    {
        string GetConfigurationValue(string key);
    }
}
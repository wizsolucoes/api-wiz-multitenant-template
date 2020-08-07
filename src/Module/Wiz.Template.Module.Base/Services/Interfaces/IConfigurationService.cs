namespace Wiz.Template.Module.Base.Services.Interfaces
{
    public interface IConfigurationService
    {         
        string Get(string squad,string tenantId, string key);
        T Get<T>(string squad,string tenantId, string key) where T : class;
        T Save<T>(string squad, string tenantId, string key, T value) where T : class;

    }
}
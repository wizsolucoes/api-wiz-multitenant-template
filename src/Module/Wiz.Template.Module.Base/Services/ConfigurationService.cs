using Newtonsoft.Json;
using Wiz.Template.Module.Base.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Azure.Data.AppConfiguration;
using System;
using Microsoft.AspNetCore.Hosting;

namespace Wiz.Template.Module.Base.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostEnviroment;

        public ConfigurationService(IConfiguration configuration, IHostingEnvironment hostEnviroment)
        {
            _configuration = configuration;
            _hostEnviroment = hostEnviroment;
        }

        public string Get(string squad,string tenantId, string key) {
            string settings = _configuration.GetSection($"{squad}:{tenantId}:{key}").Value;
            return settings;
        }

        public T Get<T>(string squad,string tenantId, string key) where T : class {
            string config = _configuration.GetSection($"{squad}:{tenantId}:{key}").Value;
            T settings = null;
            if(!string.IsNullOrWhiteSpace(config)){
                settings = JsonConvert.DeserializeObject<T>(config);
            }
            return settings;
        }

        public T Save<T>(string squad, string tenantId, string key, T value) where T : class
        {
            string connection = _configuration.GetSection("ConnectionStrings:AppConfig").Value;
            var client = new ConfigurationClient(connection);
            var settingToCreate = new ConfigurationSetting($"{squad}:{tenantId}:{key}", JsonConvert.SerializeObject(value), label: _hostEnviroment.EnvironmentName.ToLower());
            ConfigurationSetting setting = client.SetConfigurationSetting(settingToCreate);

            var sentinelSettings = new ConfigurationSetting($"{squad}:Settings:Sentinel", DateTime.UtcNow.Ticks.ToString(), label: _hostEnviroment.EnvironmentName.ToLower() );
            ConfigurationSetting sentinel = client.SetConfigurationSetting(sentinelSettings);

            return value;
        }
    }
}
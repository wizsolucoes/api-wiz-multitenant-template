using System.Collections.Generic;
using Newtonsoft.Json;

namespace Wiz.Template.Module.Base.ViewModels.Configuration
{
    [JsonObject]
    public class ConfigurationViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; } 
        [JsonProperty("theme")]
        public ThemeViewModel Theme { get; set; } 
        [JsonProperty("logoImageUrl")]
        public string LogoImageUrl { get; set; } 
        [JsonProperty("features")]
        public List<string> Features { get; set; } 
    }
}
using Newtonsoft.Json;

namespace Wiz.Template.Module.Base.ViewModels.Configuration
{
    [JsonObject]
    public class ThemeViewModel
    {
        [JsonProperty("primaryColor")]
        public string PrimaryColor { get; set; } 
    }
}
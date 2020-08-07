using Newtonsoft.Json;

namespace Wiz.Template.Module.Base.ViewModels.Corporate
{
    [JsonObject]
    public class CorporateViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("nome")]
        public string Nome { get; set; }
        [JsonProperty("propA")]
        public int PropA { get; set; }
    }
}
using System.Collections.Generic;

namespace Wiz.Template.Module.Base.ViewModels.Configuration
{
    public class TenantViewModel
    {
        public string Id { get; set; }
        public string Dns { get; set; }
        public Dictionary<string, string> Items { get; set; }
    }
}
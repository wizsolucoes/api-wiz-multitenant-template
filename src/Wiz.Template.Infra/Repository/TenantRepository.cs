using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Wiz.Multitenant.Core.Common;
using Wiz.Multitenant.Core.Common.Interfaces;

namespace Wiz.Template.Infra.Repository
{
    public class TenantRepository : ITenantStore<Tenant>
    {
        private readonly IConfiguration _configuration;

        public TenantRepository(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public async Task<Tenant> GetTenantAsync(string identifier)
        {

            string config = _configuration.GetSection($"devz:All:Tenants").Value;
            List<Tenant> tenantArray = null;

            if (!string.IsNullOrWhiteSpace(config))
            {
                tenantArray = JsonConvert.DeserializeObject<List<Tenant>>(config);
            }

            var tenant = tenantArray?.SingleOrDefault(t => t.Dns == identifier);

            if (identifier != null && tenant == null)
            {
                //Dev only
                tenant = new Tenant
                {
                    Id = "8080",
                    Dns = "localhost",
                    Items = {
                        {"module", "Wiz.Template.Module.Base.Bootstrap, Wiz.Template.Module.Base"},
                        {"financingMethod", "Wiz.Template.Module.Base.Services.SacFinancingMethodService, Wiz.Template.Module.Base"}
                    }
                };
            }

            return await Task.FromResult(tenant);
        }
    }
}
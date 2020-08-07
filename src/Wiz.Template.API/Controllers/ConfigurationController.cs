using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Wiz.Multitenant.Core.Common;
using Wiz.Multitenant.Core.Common.Service;
using Wiz.Template.Domain.Settings;
using Wiz.Template.Module.Base.Services.Interfaces;
using Wiz.Template.Module.Base.ViewModels.Configuration;

namespace Wiz.Template.API.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    [Route("api/v1/configuration")]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationService _configurationService;
        private readonly TenantAccessService<Tenant> _tenantService;

        public ConfigurationController(IConfigurationService configurationService,TenantAccessService<Tenant> tenantService)
        {
            this._configurationService = configurationService;
            this._tenantService = tenantService;
        }

        /// <summary>
        /// Busca configuração para o front.
        /// </summary>
        /// <returns>Configuração.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ConfigurationViewModel>>> Get()
        {
            Tenant t = await this._tenantService.GetTenantAsync();

            ConfigurationViewModel configuration = this._configurationService.Get<ConfigurationViewModel>(Startup.SQUAD, t.Id, "WebApp");

            return Ok(configuration);
        }

        [HttpPost("webapp")]
        public async Task<ActionResult<IEnumerable<ConfigurationViewModel>>> PutWebApp(ConfigurationViewModel model)
        {
            Tenant t = await this._tenantService.GetTenantAsync();

            ConfigurationViewModel configuration = this._configurationService.Save<ConfigurationViewModel>(Startup.SQUAD, t.Id, "WebApp", model );

            return Ok(configuration);
        }

        [HttpPut("webapp")]
        public async Task<ActionResult<IEnumerable<ConfigurationViewModel>>> PostWebApp(ConfigurationViewModel model)
        {
            Tenant t = await this._tenantService.GetTenantAsync();

            ConfigurationViewModel configuration = this._configurationService.Save<ConfigurationViewModel>(Startup.SQUAD, t.Id, "WebApp", model );

            return Ok(configuration);
        }

    }
}
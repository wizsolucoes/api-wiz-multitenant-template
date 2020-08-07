using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wiz.Multitenant.Core.Common;
using Wiz.Multitenant.Core.Common.Service;
using Wiz.Template.Module.Base.Services.Interfaces;
using Wiz.Template.Module.Base.ViewModels.Corporate;

namespace Wiz.Template.API.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    [Route("api/v1/corporate")]
    public class CorporateController : ControllerBase
    {
        private readonly ICorporateService _corporateService;
        private readonly TenantAccessService<Tenant> _tenantService;
        private readonly IFinancingMethodService _financingMethodService;

        public CorporateController(ICorporateService corporateService, IFinancingMethodService financingMethodService, TenantAccessService<Tenant> tenantService)
        {
            this._corporateService = corporateService;
            this._tenantService = tenantService;
            this._financingMethodService = financingMethodService;
        }

        /// <summary>
        /// Busca configuração para o front.
        /// </summary>
        /// <returns>Configuração.</returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("simulate")]
        public async Task<ActionResult<SimulatedLoanViewModel>> PostSimulate(SimulateViewModel simulate)
        {
            SimulatedLoanViewModel model = null;
     
                model = this._financingMethodService.Calculate(simulate);

            return Ok(model);
        }

        /// <summary>
        /// Busca configuração para o front.
        /// </summary>
        /// <returns>Configuração.</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<CorporateViewModel>> Post(OpViewModel op)
        {
            CorporateViewModel model = null;
            
                model = this._corporateService.Get(op.A, op.B);
            

            return Ok(model);
        }
    }
}
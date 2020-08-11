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
        private readonly IFinancingMethodService _financingMethodService;

        public CorporateController(ICorporateService corporateService, IFinancingMethodService financingMethodService)
        {
            this._corporateService = corporateService;
            this._financingMethodService = financingMethodService;
        }

        /// <summary>
        /// Simula um emprestimo.
        /// </summary>
        /// <returns>Configuração.</returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("simulate")]
        public ActionResult<SimulatedLoanViewModel> PostSimulate(SimulateViewModel simulate)
        {
            SimulatedLoanViewModel model = this._financingMethodService.Calculate(simulate);

            return Ok(model);
        }

        /// <summary>
        /// Realiza cálculo com base na regra de negócio.
        /// </summary>
        /// <returns>Configuração.</returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult<CorporateViewModel> Post(OpViewModel op)
        {
            CorporateViewModel model = this._corporateService.Get(op.A, op.B);


            return Ok(model);
        }
    }
}
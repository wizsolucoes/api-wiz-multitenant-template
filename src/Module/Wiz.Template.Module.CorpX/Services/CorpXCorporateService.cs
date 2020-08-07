using Wiz.Template.Module.Base.Services.Interfaces;
using Wiz.Template.Module.Base.ViewModels.Corporate;
using Wiz.Template.Module.CorpX.ViewModels.Corporate;

namespace Wiz.Template.Module.CorpX.Services
{
    public class CorpXCorporateService: ICorporateService
    {
        public CorporateViewModel Get(int a, int b){
            return new CorpXCorporateViewModel(){
                Id = 2,
                Nome = "CorpX",
                PropA = a * b,
                PropB = 1000
            };
        }

        public SimulatedLoanViewModel SaveProspect(SimulateViewModel prospect)
        {
            throw new System.NotImplementedException();
        }
    }
}
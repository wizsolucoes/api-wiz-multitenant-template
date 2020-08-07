using Wiz.Template.Module.Base.Services.Interfaces;
using Wiz.Template.Module.Base.ViewModels.Corporate;

namespace Wiz.Template.Module.Base.Services
{
    public class CorporateService : ICorporateService
    {
        public CorporateViewModel Get(int a, int b){
            return new CorporateViewModel(){
                Id = 1,
                Nome = "Base",
                PropA = (a + b)
            };
        }
    }
}
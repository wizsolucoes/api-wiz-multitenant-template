using Wiz.Template.Module.Base.ViewModels.Corporate;

namespace Wiz.Template.Module.Base.Services.Interfaces
{
    public interface IFinancingMethodService
    {
        SimulatedLoanViewModel Calculate(SimulateViewModel prospect);
    }
}
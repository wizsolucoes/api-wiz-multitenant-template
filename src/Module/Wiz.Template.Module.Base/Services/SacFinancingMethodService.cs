using System.Collections.Generic;
using System.Linq;
using Wiz.Template.Module.Base.Services.Interfaces;
using Wiz.Template.Module.Base.ViewModels.Corporate;

namespace Wiz.Template.Module.Base.Services
{
public class SacFinancingMethodService : IFinancingMethodService
    {
        public SimulatedLoanViewModel Calculate(SimulateViewModel prospect)
        {

            SimulatedLoanViewModel model = new SimulatedLoanViewModel();

            model.AmountFinanced = prospect.LoanAmount;
            model.Method = "SAC";
            model.Quantity = prospect.Quantity;
            
            model.Itens = new List<RowItemViewModel>();

            double i = 5.0 / 100.0;
            Calc(model.Itens,model.AmountFinanced, i  ,model.AmountFinanced, model.Quantity);

            model.Installment = model.Itens.FirstOrDefault()?.Installment ?? 0;

            return model;
        }

        private void Calc(List<RowItemViewModel> list, double amountFinanced, double i , double pv, int n){
            
            RowItemViewModel item = new RowItemViewModel();
            item.Number = list.Count + 1;
            //PRICE

            item.Tax = i * pv;
            item.Amortization = amountFinanced / n;
            
            item.Installment = item.Amortization +  item.Tax;
            item.OutstandingBalance = pv - item.Amortization;

            list.Add(item);

            if(item.OutstandingBalance > 0){
                Calc(list,amountFinanced, i , item.OutstandingBalance, n);
            }else{
                item.OutstandingBalance = 0;
            }
        }
    }
}
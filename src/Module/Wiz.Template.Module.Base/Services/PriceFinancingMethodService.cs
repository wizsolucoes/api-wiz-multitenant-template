using System;
using System.Collections.Generic;
using System.Linq;
using Wiz.Template.Module.Base.Services.Interfaces;
using Wiz.Template.Module.Base.ViewModels.Corporate;

namespace Wiz.Template.Module.Base.Services
{
public class PriceFinancingMethodService : IFinancingMethodService
    {
        public SimulatedLoanViewModel Calculate(SimulateViewModel prospect)
        {

            SimulatedLoanViewModel model = new SimulatedLoanViewModel();

            model.AmountFinanced = prospect.LoanAmount;
            model.Method = "Price";
            model.Quantity = prospect.Quantity;
            
            model.Itens = new List<RowItemViewModel>();

            Calc(model.Itens,model.AmountFinanced, (1.5 / 100),model.AmountFinanced, model.Quantity);

            model.Installment = model.Itens.FirstOrDefault()?.Installment ?? 0;

            return model;
        }

        private void Calc(List<RowItemViewModel> list, double amountFinanced, double i , double pv, int n){
            
            RowItemViewModel item = new RowItemViewModel();
            item.Number = list.Count + 1;
            //PRICE
            //double i =  taxaJurosMensal / 100;

            //valor parcela
            item.Installment = Convert.ToDouble(pv * ((Math.Pow((1 + i), n) * i) / (Math.Pow((1+i), n) - 1)));

            item.Tax = i * amountFinanced;
            item.Amortization = item.Installment - item.Tax;
            
            item.OutstandingBalance = amountFinanced-item.Amortization;

            list.Add(item);
            if(item.OutstandingBalance > 0){
                Calc(list,item.OutstandingBalance, i , pv, n);
            }else{
                item.OutstandingBalance = 0;
            }
        }
    }
}
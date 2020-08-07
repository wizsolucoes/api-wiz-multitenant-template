using System.Collections.Generic;

namespace Wiz.Template.Module.Base.ViewModels.Corporate
{
    public class SimulatedLoanViewModel
    {
        public string Method { get; set; }
        public double AmountFinanced { get; set; }
        public double Installment { get; set; }
        public int Quantity { get; set; }

        public List<RowItemViewModel> Itens {get;set;}
    }
}
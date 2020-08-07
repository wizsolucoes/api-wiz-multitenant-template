using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Wiz.Template.Module.Base.ViewModels.Customer
{
    public class CustomerNameViewModel
    {
        public CustomerNameViewModel() { }

        public CustomerNameViewModel(string name)
        {
            Name = name;
        }

        [FromRoute(Name = "name")]
        [Required(ErrorMessage = "Nome é obrigatório")]
        public string Name { get; set; }
    }
}
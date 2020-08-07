using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Wiz.Template.Module.Base.ViewModels.Customer
{
    public class CustomerIdViewModel
    {
        public CustomerIdViewModel() { }

        public CustomerIdViewModel(int id)
        {
            Id = id;
        }

        [FromRoute(Name = "id")]
        [Required(ErrorMessage = "Id é obrigatório")]
        public int Id { get; set; }
    }
}
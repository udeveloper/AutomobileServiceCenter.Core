using ASC.WebMVC.Models.AccountViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.WebMVC.Areas.Accounts.Models
{
    public class ServiceEngineerRegistrationViewModel : RegisterViewModel
    {
        public string UserName { get; set; }
        public bool IsEdit { get; set; }
        public bool IsActive { get; set; }
    }
}

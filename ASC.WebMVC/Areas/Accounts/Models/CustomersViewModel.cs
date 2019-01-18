using ASC.WebMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.WebMVC.Areas.Accounts.Models
{
    public class CustomersViewModel
    {
        public List<ApplicationUser> Customers { get; set; }
        public CustomerRegistrationViewModel Registration { get; set; }
    }
}

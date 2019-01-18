using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.WebMVC.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string Nombres { get; set; }
    }
}

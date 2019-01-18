using ASC.WebMVC.Configuration;
using ASC.WebMVC.Models;
using ElCamino.AspNetCore.Identity.AzureTable.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace ASC.WebMVC.Data
{
    public interface IIdentitySeed
    {
        Task Seed(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IOptions<ApplicationSettings> options);
    }


    public class IdentitySeed : IIdentitySeed
    {
        public async Task Seed(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IOptions<ApplicationSettings> options)
        {
            // Get All comma-separated roles
            var roles = options.Value.Roles.Split(new char[] { ',' });

            // Create roles if they are not existed
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    ApplicationRole storageRole = new ApplicationRole
                    {
                        Name = role
                    };
                    IdentityResult roleResult = await roleManager.CreateAsync(storageRole);
                }
            }

            // Create admin if he is not existed
            var admin = await userManager.FindByEmailAsync(options.Value.AdminEmail);
            if (admin == null)
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = options.Value.AdminEmail,
                    Email = options.Value.AdminEmail,
                    EmailConfirmed = true,                   
                };

                IdentityResult result =  userManager.CreateAsync(user, "Admin.1234").GetAwaiter().GetResult();
                await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", options.Value.AdminEmail));
                await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", "True"));
                await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("Nombres", options.Value.AdminName));

                // Add Admin to Admin roles
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            // Create a service engineer if he is not existed
            var engineer = await userManager.FindByEmailAsync(options.Value.EngineerEmail);
            if (engineer == null)
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = options.Value.EngineerEmail,
                    Email = options.Value.EngineerEmail,
                    EmailConfirmed = true,
                    LockoutEnabled = false
                };

                IdentityResult result = await userManager.CreateAsync(user, "Enginer.1234");
                await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", options.Value.EngineerEmail));
                await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", "True"));
                await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("Nombres", options.Value.EngineerName));

                // Add Service Engineer to Engineer role
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Engineer");
                }
            }
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Insightly_project.Models.utils
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndAdmin(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Admin", "TeamMember", "Client" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var admin = await userManager.FindByEmailAsync("admin@insightly.com");
            if (admin == null)
            {
                admin = new ApplicationUser { UserName = "admin@insightly.com", Email = "admin@insightly.com", EmailConfirmed = true };
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            var member = await userManager.FindByEmailAsync("member@insightly.com");
            if (member == null)
            {
                member = new ApplicationUser { UserName = "member@insightly.com", Email = "member@insightly.com", EmailConfirmed = true };
                await userManager.CreateAsync(member, "Member@123");
                await userManager.AddToRoleAsync(member, "TeamMember");
            }

            var client = await userManager.FindByEmailAsync("client@insightly.com");
            if (client == null)
            {
                client = new ApplicationUser { UserName = "client@insightly.com", Email = "client@insightly.com", EmailConfirmed = true };
                await userManager.CreateAsync(client, "Client@123");
                await userManager.AddToRoleAsync(client, "Client");
            }
        }
    }

}

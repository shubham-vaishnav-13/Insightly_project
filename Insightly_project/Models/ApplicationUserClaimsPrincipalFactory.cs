using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Insightly_project.Models
{
    public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public ApplicationUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            var displayName = user?.Name ?? user?.UserName ?? user?.Email ?? string.Empty;

            if (!string.IsNullOrEmpty(displayName))
            {
                var existingNameClaim = identity.FindFirst(ClaimTypes.Name);
                if (existingNameClaim != null)
                {
                    identity.RemoveClaim(existingNameClaim);
                }
                identity.AddClaim(new Claim(ClaimTypes.Name, displayName));

                var friendlyClaim = identity.FindFirst("name");
                if (friendlyClaim != null)
                {
                    identity.RemoveClaim(friendlyClaim);
                }
                identity.AddClaim(new Claim("name", displayName));
            }

            return identity;
        }
    }
}

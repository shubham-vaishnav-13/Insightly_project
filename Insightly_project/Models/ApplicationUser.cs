using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Insightly_project.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            ProjectUsers = new HashSet<ProjectUser>();
        }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // Join entities linking this user to projects
        public ICollection<ProjectUser> ProjectUsers { get; set; }
    }
}

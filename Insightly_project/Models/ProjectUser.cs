using System;

namespace Insightly_project.Models
{
    // Join entity required for many-to-many in EF Core 3.1
    public class ProjectUser
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}

using System;

namespace Insightly_project.Models
{
    public class TaskItemUser
    {
        public int TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}

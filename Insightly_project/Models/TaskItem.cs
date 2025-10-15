using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Insightly_project.Models
{
    public enum TaskStatus
    {
        Pending,
        InProgress,
        Completed
    }

    public class TaskItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Task title is required")]
        [StringLength(200, MinimumLength = 3,ErrorMessage = "Task title must be between 3 and 200 characters")]
        public string Title { get; set; }

        [Required]
        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [ForeignKey("Project")]
        [Display(Name = "Project")]
        public int ProjectId { get; set; }

        public Project Project { get; set; }

        public ICollection<TaskItemUser> TaskItemUsers { get; set; } = new HashSet<TaskItemUser>();
    }
}

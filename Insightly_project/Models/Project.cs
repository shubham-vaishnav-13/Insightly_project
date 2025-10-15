using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Insightly_project.Models
{
    public class Project
    {
        public Project()
        {
            Tasks = new HashSet<TaskItem>();
            ProjectUsers = new HashSet<ProjectUser>();
        }
        
        public int Id { get; set; }

        [Required(ErrorMessage = "Project name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Project name must be between 3 and 100 characters")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description can be max 500 characters")]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<TaskItem> Tasks { get; set; }

    public ICollection<ProjectUser> ProjectUsers { get; set; }
    }
}

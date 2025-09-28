using System.Collections.Generic;
using Insightly_project.Models;

namespace Insightly_project.Models.ViewModels
{
    public class AdminDashboardStatsViewModel
    {
        // User counts
        public int TotalUsers { get; set; }
        public int AdminCount { get; set; }
        public int TeamMemberCount { get; set; }
        public int ClientCount { get; set; }

        // Project metrics
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; } // projects with EndDate null or in future
        public int ProjectsLast30Days { get; set; }

        // Task metrics
        public double CompletionPercent { get; set; }

        public List<Project> RecentProjects { get; set; } = new List<Project>();
        public List<TaskItem> OverdueTaskSamples { get; set; } = new List<TaskItem>();
    }
}

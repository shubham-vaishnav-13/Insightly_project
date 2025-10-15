using System.Collections.Generic;
using Insightly_project.Models;

namespace Insightly_project.Models.ViewModels
{
    public class AdminDashboardStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int AdminCount { get; set; }
        public int TeamMemberCount { get; set; }
        public int ClientCount { get; set; }

        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; } 
        public int ProjectsLast30Days { get; set; }

        public double CompletionPercent { get; set; }

        public List<Project> RecentProjects { get; set; } = new List<Project>();
        public List<TaskItem> OverdueTaskSamples { get; set; } = new List<TaskItem>();
    }
}

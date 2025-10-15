using System;
using System.Linq;
using System.Threading.Tasks;
using Insightly_project.Models;
using Insightly_project.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// This Controller is use to display admin dashboard statistics

namespace Insightly_project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.UtcNow;
            var cutoff = now.AddDays(-30);

            // User counts 
            var users = _userManager.Users;
            int totalUsers = await users.CountAsync();

            // Count users per role
            async Task<int> CountRoleAsync(string role)
            {
                if (!await _roleManager.RoleExistsAsync(role)) return 0;
                var inRole = await _userManager.GetUsersInRoleAsync(role);
                return inRole.Count;
            }

            int adminCount = await CountRoleAsync("Admin");
            int teamMemberCount = await CountRoleAsync("TeamMember");
            int clientCount = await CountRoleAsync("Client");

            // Projects
            var projectsQuery = _context.Projects.AsNoTracking();
            int totalProjects = await projectsQuery.CountAsync();
            int activeProjects = await projectsQuery.CountAsync(p => !p.EndDate.HasValue || p.EndDate.Value >= now.Date);
            int projectsLast30 = await projectsQuery.CountAsync(p => p.CreatedAt >= cutoff);

            // Tasks
            var tasksQuery = _context.TaskItems.AsNoTracking();
            int totalTasks = await tasksQuery.CountAsync();
            int completedTasks = await tasksQuery.CountAsync(t => t.Status == Insightly_project.Models.TaskStatus.Completed);
            double completionPercent = totalTasks == 0 ? 0 : Math.Round((double)completedTasks / totalTasks * 100, 1);

            // Recent projects (last 5 created)
            var recentProjects = await projectsQuery
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Overdue tasks (top 5 past due and not completed)
            var overdueTasks = await _context.TaskItems
                .Include(t => t.Project)
                .Where(t => t.DueDate.HasValue && t.DueDate < now.Date && t.Status != Insightly_project.Models.TaskStatus.Completed)
                .OrderBy(t => t.DueDate)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();

            var vm = new AdminDashboardStatsViewModel
            {
                TotalUsers = totalUsers,
                AdminCount = adminCount,
                TeamMemberCount = teamMemberCount,
                ClientCount = clientCount,
                TotalProjects = totalProjects,
                ActiveProjects = activeProjects,
                ProjectsLast30Days = projectsLast30,
                CompletionPercent = completionPercent,
                RecentProjects = recentProjects,
                OverdueTaskSamples = overdueTasks
            };

            return View(vm);
        }
    }
}

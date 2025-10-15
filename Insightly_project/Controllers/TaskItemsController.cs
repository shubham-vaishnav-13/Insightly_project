using Insightly_project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Insightly_project.Controllers
{
    [Authorize(Roles = "Admin,TeamMember,Client")]
    public class TaskItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskItemsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin,TeamMember,Client")]
        public async Task<IActionResult> Index()
        {
            var query = _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.TaskItemUsers)
                .ThenInclude(tu => tu.User)
                .AsQueryable();

            if (User.IsInRole("TeamMember"))
            {
                var userId = _userManager.GetUserId(User);
                query = query.Where(t => t.TaskItemUsers.Any(tu => tu.UserId == userId));
            }

            var list = await query
                .OrderByDescending(t => t.DueDate)
                .ThenBy(t => t.Title)
                .ToListAsync();
            return View(list);
        }

        [Authorize(Roles = "Admin,TeamMember,Client")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.TaskItemUsers)
                    .ThenInclude(tu => tu.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            if (User.IsInRole("TeamMember"))
            {
                var userId = _userManager.GetUserId(User);
                if (!taskItem.TaskItemUsers.Any(tu => tu.UserId == userId))
                {
                    return Forbid();
                }
            }

            return View(taskItem);
        }

        // Only Admins can create/edit/delete tasks
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
            ViewData["Assignees"] = new MultiSelectList(Enumerable.Empty<SelectListItem>()); // dynamic
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Title,Status,DueDate,ProjectId")] TaskItem taskItem, string[] selectedAssignees)
        {
            if (ModelState.IsValid)
            {
                _context.Add(taskItem);
                await _context.SaveChangesAsync();

                // add assignees
                if (selectedAssignees != null && selectedAssignees.Length > 0)
                {
                    var validIds = GetTeamMembersForProject(taskItem.ProjectId).Select(u => u.Id).ToHashSet();
                    foreach (var uid in selectedAssignees.Distinct())
                    {
                        if (validIds.Contains(uid))
                        {
                            _context.TaskItemUsers.Add(new TaskItemUser { TaskItemId = taskItem.Id, UserId = uid });
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", taskItem.ProjectId);
            if (taskItem.ProjectId != 0)
            {
                var teamMembers = GetTeamMembersForProject(taskItem.ProjectId);
                ViewData["Assignees"] = new MultiSelectList(teamMembers, "Id", "UserName", selectedAssignees);
            }
            return View(taskItem);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItems
                .Include(t => t.TaskItemUsers)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (taskItem == null)
            {
                return NotFound();
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", taskItem.ProjectId);
            var teamMembers = GetTeamMembersForProject(taskItem.ProjectId);
            var existing = taskItem.TaskItemUsers.Select(tu => tu.UserId).ToArray();
            ViewData["Assignees"] = new MultiSelectList(teamMembers, "Id", "UserName", existing);
            return View(taskItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Status,DueDate,ProjectId")] TaskItem taskItem, string[] selectedAssignees)
        {
            if (id != taskItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.TaskItems
                        .Include(t => t.TaskItemUsers)
                        .FirstOrDefaultAsync(t => t.Id == id);
                    if (existing == null) return NotFound();

                    // update scalar fields
                    existing.Title = taskItem.Title;
                    existing.Status = taskItem.Status;
                    existing.DueDate = taskItem.DueDate;
                    // Project change optional (if allowed)
                    existing.ProjectId = taskItem.ProjectId;

                    var validIds = GetTeamMembersForProject(existing.ProjectId).Select(u => u.Id).ToHashSet();
                    var desired = (selectedAssignees ?? Array.Empty<string>()).Where(id2 => validIds.Contains(id2)).Distinct().ToHashSet();
                    var current = existing.TaskItemUsers.Select(tu => tu.UserId).ToHashSet();

                    // Remove unselected
                    var removeList = existing.TaskItemUsers.Where(tu => !desired.Contains(tu.UserId)).ToList();
                    foreach (var r in removeList)
                        _context.TaskItemUsers.Remove(r);

                    // Add new
                    foreach (var addId in desired.Except(current))
                        _context.TaskItemUsers.Add(new TaskItemUser { TaskItemId = existing.Id, UserId = addId });

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskItemExists(taskItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", taskItem.ProjectId);
            var members = GetTeamMembersForProject(taskItem.ProjectId);
            ViewData["Assignees"] = new MultiSelectList(members, "Id", "UserName", selectedAssignees);
            return View(taskItem);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            _context.TaskItems.Remove(taskItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Allow TeamMember to mark task as completed/incomplete only if assigned
        [HttpPost]
        [Authorize(Roles = "Admin,TeamMember")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int id, bool completed, string returnUrl = null)
        {
            var taskItem = await _context.TaskItems
                .Include(t => t.TaskItemUsers)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (taskItem == null) return NotFound();

            if (User.IsInRole("TeamMember"))
            {
                var userId = _userManager.GetUserId(User);
                if (!taskItem.TaskItemUsers.Any(tu => tu.UserId == userId))
                {
                    return Forbid();
                }
            }

            var desiredStatus = completed ? Insightly_project.Models.TaskStatus.Completed : Insightly_project.Models.TaskStatus.InProgress;
            if (taskItem.Status != desiredStatus)
            {
                taskItem.Status = desiredStatus;
                await _context.SaveChangesAsync();
                TempData["Success"] = desiredStatus == Insightly_project.Models.TaskStatus.Completed ? "Task marked completed." : "Task moved to In Progress.";
            }
            else
            {
                TempData["Info"] = "No change in task status.";
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        private bool TaskItemExists(int id) => _context.TaskItems.Any(e => e.Id == id);

        private IEnumerable<ApplicationUser> GetTeamMembersForProject(int projectId)
        {
            // Only return users assigned to the project who have the TeamMember role
            // Note: IdentityDbContext exposes Roles and UserRoles sets
            var teamRoleId = _context.Roles
                .Where(r => r.Name == "TeamMember")
                .Select(r => r.Id)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(teamRoleId))
            {
                // If role not configured, return empty to avoid assigning clients by mistake
                return Enumerable.Empty<ApplicationUser>();
            }

            var teamMemberIds = _context.UserRoles
                .Where(ur => ur.RoleId == teamRoleId)
                .Select(ur => ur.UserId);

            var users = _context.ProjectUsers
                .Where(pu => pu.ProjectId == projectId && teamMemberIds.Contains(pu.UserId))
                .Select(pu => pu.User)
                .OrderBy(u => u.UserName)
                .ToList();

            return users;
        }

        // AJAX: fetch team members for a project
        [HttpGet]
        [Authorize(Roles = "Admin,TeamMember")]
        public IActionResult GetTeamMembers(int projectId)
        {
            if (!_context.Projects.Any(p => p.Id == projectId))
            {
                return NotFound();
            }
            var users = GetTeamMembersForProject(projectId)
                .Select(u => new { u.Id, u.UserName })
                .ToList();
            return Json(users);
        }
    }
}

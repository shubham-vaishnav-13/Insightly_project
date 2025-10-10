using Insightly_project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Insightly_project.Controllers
{
    [Authorize(Roles = "Admin,TeamMember,Client")]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin,TeamMember,Client")]
        // GET: Projects
        public async Task<IActionResult> Index(string q)
        {
            // Base query including join table for filtering
            var query = _context.Projects
                .Include(p => p.ProjectUsers)
                .AsQueryable();

            // Team members only see projects they are assigned to
            if (User.IsInRole("TeamMember"))
            {
                var userId = _userManager.GetUserId(User);
                query = query.Where(p => p.ProjectUsers.Any(pu => pu.UserId == userId));
            }
            // Clients also only see projects explicitly assigned to them (same ProjectUsers table)
            else if (User.IsInRole("Client"))
            {
                var userId = _userManager.GetUserId(User);
                query = query.Where(p => p.ProjectUsers.Any(pu => pu.UserId == userId));
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var like = $"%{q.Trim()}%";
                query = query.Where(p => EF.Functions.Like(p.Name, like) || (p.Description != null && EF.Functions.Like(p.Description, like)));
            }

            ViewData["q"] = q;
            var list = await query
                .OrderBy(p => p.Name)
                .ToListAsync();
            return View(list);
        }

        [Authorize(Roles = "Admin,TeamMember,Client")]
        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.ProjectUsers)
                    .ThenInclude(pu => pu.User)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.TaskItemUsers)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            if (User.IsInRole("TeamMember"))
            {
                var userId = _userManager.GetUserId(User);
                // Ensure team member is assigned to this project
                if (!project.ProjectUsers.Any(pu => pu.UserId == userId))
                {
                    return Forbid();
                }
                // Filter tasks to only those assigned to the user
                project.Tasks = project.Tasks
                    .Where(t => t.TaskItemUsers.Any(tu => tu.UserId == userId))
                    .ToList();
            }
            else if (User.IsInRole("Client"))
            {
                var userId = _userManager.GetUserId(User);
                // Ensure client is assigned to this project; if not forbid
                if (!project.ProjectUsers.Any(pu => pu.UserId == userId))
                {
                    return Forbid();
                }
                // Business rule: client can view the whole project and all its tasks (no per-task filtering)
            }
            // Build separate lists for view display
            var teamMemberIds = (await _userManager.GetUsersInRoleAsync("TeamMember")).Select(u => u.Id).ToHashSet();
            var clientIds = (await _userManager.GetUsersInRoleAsync("Client")).Select(u => u.Id).ToHashSet();
            ViewBag.TeamMembersList = project.ProjectUsers.Where(pu => teamMemberIds.Contains(pu.UserId)).Select(pu => pu.User).OrderBy(u => u.UserName).ToList();
            ViewBag.ClientsList = project.ProjectUsers.Where(pu => clientIds.Contains(pu.UserId)).Select(pu => pu.User).OrderBy(u => u.UserName).ToList();

            return View(project);
        }

        [Authorize(Roles = "Admin")]
        // GET: Projects/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        // POST: Projects/Create
        public async Task<IActionResult> Create([Bind("Id,Name,Description,StartDate,EndDate,CreatedAt")] Project project)
        {
            // Simple date validation
            if (project.EndDate.HasValue && project.EndDate < project.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date cannot be earlier than start date.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        [Authorize(Roles = "Admin")]
        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        // POST: Projects/Edit/5
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,StartDate,EndDate,CreatedAt")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            // Simple date validation
            if (project.EndDate.HasValue && project.EndDate < project.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date cannot be earlier than start date.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
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
            return View(project);
        }

        [Authorize(Roles = "Admin")]
        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        // POST: Projects/Delete/5
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }

        [Authorize(Roles = "Admin")]
        // GET: Projects/AssignTeamMembers/5
        public async Task<IActionResult> AssignTeamMembers(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.ProjectUsers)
                    .ThenInclude(pu => pu.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            // Get all users with TeamMember role
            var teamMembers = await _userManager.GetUsersInRoleAsync("TeamMember");
            
            // Create a list of SelectListItem for team members
            var teamMemberItems = teamMembers.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = string.IsNullOrWhiteSpace(u.Name) ? u.UserName : $"{u.Name} - {u.UserName}",
                Selected = project.ProjectUsers.Any(t => t.UserId == u.Id)
            }).ToList();

            ViewBag.TeamMembers = teamMemberItems;
            ViewBag.ProjectId = project.Id;
            ViewBag.ProjectName = project.Name;

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        // POST: Projects/AssignTeamMembers/5
        public async Task<IActionResult> AssignTeamMembers(int id, string[] selectedTeamMembers)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            var selectedSet = selectedTeamMembers == null ? new HashSet<string>() : new HashSet<string>(selectedTeamMembers);

            // Existing assignments separated by role to avoid deleting clients here
            var teamMemberIds = (await _userManager.GetUsersInRoleAsync("TeamMember")).Select(u => u.Id).ToHashSet();

            // Remove unselected ONLY for team members (do not touch clients via this action)
            var toRemove = project.ProjectUsers
                .Where(pu => teamMemberIds.Contains(pu.UserId) && !selectedSet.Contains(pu.UserId))
                .ToList();
            foreach (var rem in toRemove)
            {
                _context.ProjectUsers.Remove(rem);
            }

            // Existing user ids
            var existing = project.ProjectUsers.Select(pu => pu.UserId).ToHashSet();

            // Add new selections
            foreach (var userId in selectedSet)
            {
                if (!existing.Contains(userId))
                {
                    _context.ProjectUsers.Add(new ProjectUser { ProjectId = project.Id, UserId = userId });
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Team members assigned successfully.";
            return RedirectToAction(nameof(Details), new { id = project.Id });
        }

        [Authorize(Roles = "Admin")]
        // GET: Projects/AssignClients/5
        public async Task<IActionResult> AssignClients(int? id)
        {
            if (id == null) return NotFound();
            var project = await _context.Projects
                .Include(p => p.ProjectUsers)
                .ThenInclude(pu => pu.User)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (project == null) return NotFound();

            var clients = await _userManager.GetUsersInRoleAsync("Client");
            var clientItems = clients.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = string.IsNullOrWhiteSpace(u.Name) ? u.UserName : $"{u.Name} - {u.UserName}",
                Selected = project.ProjectUsers.Any(pu => pu.UserId == u.Id)
            }).ToList();

            ViewBag.Clients = clientItems;
            ViewBag.ProjectId = project.Id;
            ViewBag.ProjectName = project.Name;
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        // POST: Projects/AssignClients/5
        public async Task<IActionResult> AssignClients(int id, string[] selectedClients)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (project == null) return NotFound();

            var selectedSet = selectedClients == null ? new HashSet<string>() : new HashSet<string>(selectedClients);

            var clientIds = (await _userManager.GetUsersInRoleAsync("Client")).Select(u => u.Id).ToHashSet();

            // Remove unselected clients only
            var toRemove = project.ProjectUsers
                .Where(pu => clientIds.Contains(pu.UserId) && !selectedSet.Contains(pu.UserId))
                .ToList();
            foreach (var rem in toRemove)
            {
                _context.ProjectUsers.Remove(rem);
            }

            var existingClientAssignments = project.ProjectUsers.Where(pu => clientIds.Contains(pu.UserId)).Select(pu => pu.UserId).ToHashSet();
            foreach (var clientId in selectedSet)
            {
                if (!existingClientAssignments.Contains(clientId))
                {
                    _context.ProjectUsers.Add(new ProjectUser { ProjectId = project.Id, UserId = clientId });
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Clients assigned successfully.";
            return RedirectToAction(nameof(Details), new { id = project.Id });
        }
    }
}

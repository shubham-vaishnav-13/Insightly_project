using Insightly_project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

// This Controller is used for admin user management functions
// This is used for listing users, assigning roles, and deleting users

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // List all users
    public IActionResult Users()
    {
        var users = _userManager.Users.ToList();
        return View(users);
    }

    // GET: Assign role form
    public async Task<IActionResult> AssignRole(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = _roleManager.Roles.Select(r => r.Name).ToList();
        var userRoles = await _userManager.GetRolesAsync(user);

        ViewBag.Roles = roles;
        ViewBag.UserRoles = userRoles;

        return View(user);
    }

    // POST: Assign role
    [HttpPost]
    public async Task<IActionResult> AssignRole(string id, string role)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        // Remove existing roles
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        // Assign new role
        if (!string.IsNullOrEmpty(role))
            await _userManager.AddToRoleAsync(user, role);

    // Force all active sessions to refresh claims on next request
        await _userManager.UpdateSecurityStampAsync(user);


        TempData["Success"] = "Role updated.";

        return RedirectToAction("Users");
    }

    // POST: Delete user
    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction("Users");
        }

        // Prevent admin from deleting themselves
        var currentUser = await _userManager.GetUserAsync(User);
        if (user.Id == currentUser.Id)
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction("Users");
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = $"User {user.Email} has been deleted successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to delete user: " + string.Join(", ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction("Users");
    }
}

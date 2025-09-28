# Insightly Project API Guide

This document captures the server-side endpoints exposed by the Insightly ASP.NET Core MVC application. All routes follow the default MVC pattern unless explicitly noted.

_Last updated: 2025-09-28_

## Routing & Conventions

- **Base URL:** `https://<host>/`
- **Route pattern:** `{controller}/{action}/{id?}`
- Unless otherwise specified, routes are resolved using the controller and action names.
- HTML form posts use antiforgery validation. When calling endpoints programmatically, include a valid antiforgery token (or disable validation for API usage).

## Authentication & Authorization

- The site uses ASP.NET Core Identity with cookie-based authentication.
- Role-based authorization is enforced via `[Authorize(Roles = "...")]` on controllers/actions.
- Actions without `[Authorize]` are publicly accessible.

## Endpoint Catalog

### AccountController (authentication & public)

| Method | Route | Auth | Description | Parameters / Notes |
| --- | --- | --- | --- | --- |
| GET | `/Account/Register` | Anonymous | Displays the registration form. | — |
| POST | `/Account/Register` | Anonymous | Creates a new user account and signs them in. | Form: `Name`, `Email`, `Password`, `ConfirmPassword`. Redirects to Home on success. |
| GET | `/Account/Login` | Anonymous | Displays login form. | Optional query: `returnUrl`. |
| POST | `/Account/Login` | Anonymous | Attempts to sign in and redirects based on primary role (Admin/TeamMember/Client) or `returnUrl`. | Form: `Email`, `Password`, `RememberMe`, hidden `ReturnUrl`. |
| GET | `/Account/Logout` | Authenticated | Shows logout confirmation page. | Optional query: `returnUrl` (local only). |
| POST | `/Account/Logout` | Authenticated | Signs out current user and redirects to Home. | Anti-forgery token required. |
| GET | `/Account/AccessDenied` | Authenticated | (Provided by Identity UI Path) Displays access denied page. | Mapped via cookie middleware `AccessDeniedPath`. |

### AdminController (Admin role)

| Method | Route | Auth | Description | Parameters / Notes |
| --- | --- | --- | --- | --- |
| GET | `/Admin/Users` | Admin | Lists all registered users. | — |
| GET | `/Admin/AssignRole` | Admin | Shows role assignment form. | Query: `id` (user id). Populates ViewBag roles. |
| POST | `/Admin/AssignRole` | Admin | Replaces user’s roles with supplied role (clears existing). | Form: `id`, `role`. Updates security stamp to refresh claims. |

### ClientDashboardController (Client role)

| Method | Route | Auth | Description | Parameters / Notes |
| --- | --- | --- | --- | --- |
| GET | `/ClientDashboard/Index` | Client | Displays the client dashboard landing page. | — |

### TeamMemberDashboardController (TeamMember role)

| Method | Route | Auth | Description | Parameters / Notes |
| --- | --- | --- | --- | --- |
| GET | `/TeamMemberDashboard/Index` | TeamMember | Displays the team member dashboard landing page. | — |

### AdminDashboardController (Admin role)

| Method | Route | Auth | Description | Parameters / Notes |
| --- | --- | --- | --- | --- |
| GET | `/AdminDashboard/Index` | Admin | Aggregated platform metrics: user counts by role, project stats, recent projects, overdue tasks. | Returns view model `AdminDashboardStatsViewModel`. |

### HomeController (public)

| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| GET | `/` (Home/Index) | Anonymous | Landing page. |
| GET | `/Home/Privacy` | Anonymous | Privacy policy page. |
| GET | `/Home/Error` | Anonymous | Error details page. Response is not cached. |

### ProjectsController (class-level `Authorize(Roles = "Admin,TeamMember,Client")`)

| Method | Route | Auth | Description | Parameters / Notes |
| --- | --- | --- | --- | --- |
| GET | `/Projects/Index` | Admin, TeamMember, Client | Lists visible projects. | Query: `q` optional search (LIKE on Name/Description). TeamMember/Client restricted to assigned projects. |
| GET | `/Projects/Details/{id}` | Admin, TeamMember, Client | Shows project details & related tasks and participants. | TeamMember: only if assigned; tasks filtered to those assigned to user. Client: must be assigned; sees all tasks. |
| GET | `/Projects/Create` | Admin | Returns project creation form. | — |
| POST | `/Projects/Create` | Admin | Creates a new project. | Form: `Name`, `Description`, `StartDate`, `EndDate`, `CreatedAt`. |
| GET | `/Projects/Edit/{id}` | Admin | Returns edit form. | `{id}` required. |
| POST | `/Projects/Edit/{id}` | Admin | Updates project metadata. | Same fields as create. Concurrency exceptions possible. |
| GET | `/Projects/Delete/{id}` | Admin | Shows delete confirmation. | `{id}` required. |
| POST | `/Projects/Delete/{id}` | Admin | Deletes project. | Form posts to `/Projects/Delete/{id}` (ActionName="Delete"). |
| GET | `/Projects/AssignTeamMembers/{id}` | Admin | Multi-select team member assignment UI. | Populates `ViewBag.TeamMembers`. `{id}` required. |
| POST | `/Projects/AssignTeamMembers/{id}` | Admin | Replaces team member assignments. | Form: `selectedTeamMembers[]` (user ids). Preserves client links. |
| GET | `/Projects/AssignClients/{id}` | Admin | Multi-select client assignment UI. | Populates `ViewBag.Clients`. |
| POST | `/Projects/AssignClients/{id}` | Admin | Replaces client assignments. | Form: `selectedClients[]` (user ids). Preserves team member links. |

### TaskItemsController (class-level `Authorize(Roles = "Admin,TeamMember,Client")`)

| Method | Route | Auth | Description | Parameters / Notes |
| --- | --- | --- | --- | --- |
| GET | `/TaskItems/Index` | Admin, TeamMember, Client | Lists tasks with project and assignee info. | TeamMember: only own assigned tasks. Client: full list (read-only). |
| GET | `/TaskItems/Details/{id}` | Admin, TeamMember, Client | Task detail with project & assignees. | TeamMember must be assigned or gets 403. |
| GET | `/TaskItems/Create` | Admin | Displays creation form. | Populates projects dropdown; assignees loaded dynamically. |
| POST | `/TaskItems/Create` | Admin | Creates task & optional assignments. | Form: `Title`, `Status`, `DueDate`, `ProjectId`, `selectedAssignees[]`. Filters assignees to project team. |
| GET | `/TaskItems/Edit/{id}` | Admin | Edit form with existing assignees. | — |
| POST | `/TaskItems/Edit/{id}` | Admin | Updates scalar fields & assignment list. | Same form fields as create. Validates membership. |
| GET | `/TaskItems/Delete/{id}` | Admin | Delete confirmation. | — |
| POST | `/TaskItems/Delete/{id}` | Admin | Deletes task. | ActionName="Delete" POST. |
| POST | `/TaskItems/SetStatus` | Admin, TeamMember | Updates status (Completed / InProgress). | Form: `id`, `completed` (bool), optional `returnUrl`. TeamMember must be assignee. |
| GET | `/TaskItems/GetTeamMembers` | Admin, TeamMember | JSON list of project team members. | Query: `projectId` (int). Response: `[{ id, userName }]`. 404 if project not found. |


## Notes

- Anti-forgery: All POST (non-JSON) form submissions require `__RequestVerificationToken`.
- Payload format: Standard `application/x-www-form-urlencoded` form posts (unless explicitly JSON like `GetTeamMembers` response).
- Role refresh: Security stamp is validated every request so role changes take effect immediately without re-login.
- Search: Project index supports basic case-insensitive LIKE search via `q` query parameter.
- Authorization nuances:
	- Team members & clients only see/enter projects to which they are assigned.
	- Team members can only view tasks they are assigned to (except admins).
	- Clients have read-only access to tasks and cannot modify.
<!-- Real-time features (e.g., SignalR) planned but not yet implemented, so omitted from active endpoint list. -->

## Change Log

- 2025-09-28: Added full inventory (Register, Logout variants, AccessDenied, AdminDashboard metrics, AssignClients, SetStatus, search param) and clarified authorization nuances. Removed placeholder SignalR section until implemented.

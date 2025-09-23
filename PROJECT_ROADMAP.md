# Insightly Project — Final Year Roadmap

Date: 2025-09-23
Team size: 3 members

This document analyzes the current ASP.NET Core MVC project and proposes a concrete roadmap of features, tasks, and milestones suitable for a 3-person final year project. It also highlights quick wins and critical fixes.

## 1) Project overview

- Purpose: Lightweight project and task management with three roles: Admin, Team Member, Client.
- Tech stack:
  - ASP.NET Core 3.1 MVC + Razor Pages (Identity UI)
  - Entity Framework Core (SQL Server LocalDB)
  - ASP.NET Core Identity (role-based auth)
  - Bootstrap (Bootswatch Cosmo), Bootstrap Icons, jQuery
  - SignalR hub scaffolded but not wired

## 2) What’s implemented today

- Database & EF Core
  - `ApplicationDbContext` extends `IdentityDbContext<ApplicationUser>`
  - Entities: `Project`, `TaskItem` with relations (Project 1—* TaskItems)
  - Migrations present under `Migrations/` and connection string configured to LocalDB

- Identity & Roles
  - Identity registered with `ApplicationUser`, `IdentityRole`
  - Roles seeded: Admin, TeamMember, Client
  - Seed users: admin@insightly.com (Admin), member@insightly.com (TeamMember), client@insightly.com (Client)
  - Password policy configured; security stamp validation set to 0 (role changes take effect immediately)
  - Razor Pages for Identity enabled (`endpoints.MapRazorPages()`); login/register links in `_LoginPartial`

- Controllers & Views
  - ProjectsController: CRUD; access mostly for Admin/TeamMember
  - TaskItemsController: CRUD; view actions allow Client; create/edit/delete for Admin/TeamMember
  - AdminController: list users and assign a role (replaces existing roles)
  - HomeController: Index/Privacy/Error
  - Dashboards (views exist): `Views/AdminDashboard/Index.cshtml`, `Views/TeamMemberDashboard/Index.cshtml`, `Views/ClientDashboard/Index.cshtml`
    - Controllers exist for TeamMemberDashboard, ClientDashboard
    - AdminDashboardController appears missing (see Gaps)

- UI & Layout
  - `_Layout` with role-aware nav
  - `_LoginPartial` with links to Identity’s default pages
  - `_Alerts` partial for TempData messages

- SignalR
  - `Hubs/RoleHub.cs` exists with a simple `Ping()` method
  - Not registered in Startup (see Gaps)

## 3) Gaps, inconsistencies, and quick fixes

- Missing AdminDashboardController
  - AccountController redirects Admin to `AdminDashboard/Index`, but controller isn’t present (404 risk). Add `AdminDashboardController` with `[Authorize(Roles="Admin")]`.

- AccountController vs Identity UI
  - `_LoginPartial` uses Razor Pages (Identity Area) for login/register, while `AccountController.Login` exists without a corresponding MVC Login view in the repo. Either:
    1) Remove/ignore the custom `AccountController` and use Identity UI entirely, or
    2) Add MVC login view and route the navbar to it.

- SignalR not wired
  - `services.AddSignalR()` and `endpoints.MapHub<RoleHub>("/roleHub")` are missing. Also no client-side connection code.

- Authorization scoping
  - TaskItems Index/Details allow Client access but show all tasks; no scoping to “their” projects. Projects Index/Details locked to Admin/TeamMember; Clients can’t view their own projects.
  - Introduce ownership/visibility rules and filter queries by user/role.

- Data model for assignments
  - `TaskItem` lacks `AssignedToUserId`. Add assignment to team members; add `CreatedByUserId` for audit.

- Enum persistence
  - `TaskItem.Status` is an enum but EF configuration sets `HasMaxLength(50)` on `Status`. If you want string storage, configure `HasConversion<string>()`; otherwise remove the length constraint.

- No activity/comments/attachments
  - For collaboration, add task comments and optional file attachments.

- No tests or CI
  - Add minimal unit/integration tests and optional GitHub Actions.

## 4) Proposed feature roadmap (3 members)

Prioritize features that demonstrate full-stack skills, real-time UX, security, and software engineering practices.

### Milestone A: Stabilize and foundations (week 1)

- Fix Admin dashboard routing
  - Add `AdminDashboardController` (Index) with summary stats: total projects, open tasks, overdue tasks
- Decide auth flow
  - Prefer Identity UI (Remove unused MVC Login or add matching views)
- Wire SignalR
  - `services.AddSignalR()` and `MapHub<RoleHub>("/roleHub")`
  - Add client script to connect and handle a `RoleChanged` or `Pong` event (show toast + optional auto-refresh)
- Data model adjustments
  - Add `AssignedToUserId` (FK to `AspNetUsers`) and optional `CreatedByUserId`
  - For `TaskItem.Status`: configure enum-to-string conversion
- Scope access
  - Ensure Clients see only their projects and tasks (introduce project ownership or Client-Project link)

Deliverables:
- EF migration for schema changes, updated controllers/queries, working admin dashboard, SignalR connection snippet

### Milestone B: Collaboration & productivity (weeks 2–3)

- Task assignment & filters
  - Assign tasks to team members; add filters by project, status, assignee, due date; pagination/sorting
- Comments (Task discussion)
  - `TaskComment` entity with text, author, createdAt; show under Task Details
- Real-time updates
  - Broadcast via SignalR when a task is created/updated/assigned; show in-page notifications
- Client portal read-only
  - Clients can view their projects, tasks, and statuses, but cannot modify

Deliverables:
- Updated UI (forms + tables) with filters, comments UI, real-time toasts

### Milestone C: UX polish, reporting, and quality (weeks 4–5)

- Kanban board
  - Drag-and-drop columns (Pending/InProgress/Completed); update status via AJAX + SignalR broadcast
- Basic analytics
  - Summaries per project (task counts by status, velocity chart)
- Attachments
  - Allow file uploads per task (store in wwwroot/uploads or cloud; enforce size/type, scan on upload if possible)
- Email notifications (optional)
  - On assignment or due-date reminders (use `IEmailSender` or SMTP)

Deliverables:
- Kanban page, charts (Chart.js), upload handling with validation

### Milestone D: Hardening & delivery (week 6)

- Authorization policies + data scoping tests
- Validation, error pages, logging (Serilog), audit trail
- Seed demo data for presentation
- Dockerfile + simple GitHub Actions CI (
  - build, run tests, optionally publish artifacts)
- README with setup/run instructions and demo accounts

Deliverables:
- Passing tests, CI badge, deployment-ready app

## 5) Detailed tasks by role

- Developer 1 — Backend & Data
  - EF migrations (AssignedTo, Comments, Attachments)
  - Query scoping by user/role; policies/handlers
  - SignalR hubs and server-side broadcasts
  - Email service integration (optional)

- Developer 2 — Frontend & UX
  - Admin/TeamMember/Client dashboards and nav
  - Filters, sorting, pagination
  - Kanban board drag-and-drop; toasts/modals
  - Charts with Chart.js
  - Attachment UI

- Developer 3 — QA, DevOps & Integration
  - Unit/integration tests (controllers, EF in-memory)
  - Seeders for demo data
  - CI workflow (GitHub Actions); Dockerfile
  - Logging, error handling, security headers, input validation

## 6) Suggested schema additions

- Task assignment
  - TaskItem: `AssignedToUserId (string, FK)`, `CreatedByUserId (string, FK)`
- Comments
  - TaskComment: Id, TaskItemId (FK), Text, CreatedByUserId (FK), CreatedAt
- Attachments (optional)
  - TaskAttachment: Id, TaskItemId (FK), FileName, ContentType, Size, Path/Url, UploadedByUserId, UploadedAt
- Project ownership
  - Project: `ClientUserId (string, FK)` or a junction if multiple clients per project

Apply via EF migrations and update controllers/queries accordingly.

## 7) Security and correctness checklist

- Align auth UI: don’t mix Identity UI and custom MVC login unless necessary
- Fix enum persistence (use `HasConversion<string>()` or store int consistently)
- Validate model inputs server-side; use `[Bind]` carefully
- Per-role data filtering at query level
- Anti-forgery in all forms (already present on POSTs)
- File upload validation (size/type) and store outside web root if possible
- Rate-limiting/admin-only actions protected

## 8) Testing strategy

- Unit tests: controller actions with fake user principal; service methods if extracted
- Integration tests: EF Core InMemory/SQLite; CRUD flows; role-based access scenarios
- UI smoke: minimal Selenium/Playwright optional

## 9) How to run (local)

- Prerequisites: .NET Core SDK 3.1, SQL Server LocalDB (default on Windows)
- Create/Update DB: on first run, EF will use existing migrations; otherwise add/update via `dotnet ef` (optional)
- Default accounts (seeded):
  - Admin: admin@insightly.com / Admin@123
  - Team Member: member@insightly.com / Member@123
  - Client: client@insightly.com / Client@123

## 10) Demo plan

1) Login as Admin; show dashboards, manage users/roles, create a project and tasks, assign to a member
2) Login as TeamMember; see assigned tasks, update status, drag & drop on Kanban
3) Login as Client; view only their projects, observe real-time updates when team updates tasks
4) Show charts and exports; show logs and tests passing in CI

## 11) Backlog (nice-to-have)

- Multi-tenant support (organizations)
- Calendar/scheduling view
- Export to CSV/PDF; email digests
- Internationalization (i18n)
- Dark mode

---

## Appendix: Known issues to fix first

- Add `AdminDashboardController` to match existing view and redirects
- Register and map SignalR; add client code
- Reconcile AccountController vs Identity UI
- Add data scoping so Clients don’t see other customers’ data
- Fix EF enum mapping for `TaskItem.Status`

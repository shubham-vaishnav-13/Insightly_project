# Insightly Project

Lightweight role-based project & task management platform built with ASP.NET Core MVC 3.1 and Entity Framework Core. Supports three personas (Admin, Team Member, Client) with scoped visibility, task assignment, and extensible roadmap for collaboration features.

> For the full, always up-to-date endpoint table see: [API_DOCUMENTATION.md](./API_DOCUMENTATION.md)

---
## ✨ Core Features
- User authentication & role-based authorization (Identity + Roles: Admin, TeamMember, Client)
- Project management (CRUD, assign team members & clients)
- Task management (CRUD, multi-user assignment via junction table, status tracking)
- Dashboards per role (Admin metrics, Team Member view, Client view)
- Immediate role change propagation (security stamp validation each request)
- Search (project name/description)
- JSON auxiliary endpoint for dynamic team member population
- TempData alert messaging & structured layout partials
- Data seeding (roles + three default users)
- Clean separation of concerns via EF models & navigation properties

## 🧩 Planned / Roadmap Highlights
See `PROJECT_ROADMAP.md` for full milestone plan:
- Real-time updates (SignalR hub already scaffolded: `RoleHub`)
- Task comments, attachments, Kanban board, analytics, email notifications
- Improved scoping for clients & richer filters for tasks
- CI/CD pipeline & Dockerization

---
## 👥 Roles & Permissions Matrix (Current)
| Capability | Admin | Team Member | Client |
| --- | --- | --- | --- |
| View own dashboard | ✅ | ✅ | ✅ |
| Manage users / roles | ✅ | ❌ | ❌ |
| Create/Edit/Delete Projects | ✅ | ❌ | ❌ |
| Assign Team Members to Project | ✅ | ❌ | ❌ |
| Assign Clients to Project | ✅ | ❌ | ❌ |
| View Projects | ✅ (all) | ✅ (assigned only) | ✅ (assigned only) |
| Create/Edit/Delete Tasks | ✅ | ❌ (future: limited) | ❌ |
| View Tasks | ✅ (all) | ✅ (assigned only) | ✅ (read-only) |
| Change Task Status | ✅ | ✅ (if assigned) | ❌ |
| Fetch Team Members JSON | ✅ | ✅ | ❌ |

---
## 🏗 Architecture Overview
- Presentation: ASP.NET Core MVC (Controllers + Razor Views)
- Identity: `ApplicationUser` extends built-in Identity user + roles
- Data Access: EF Core DbContext `ApplicationDbContext`
- Entities:
  - `Project` (1—* `TaskItem`, many-to-many with `ApplicationUser` via `ProjectUser`)
  - `TaskItem` (many-to-many with `ApplicationUser` via `TaskItemUser`)
  - Linking tables: `ProjectUser`, `TaskItemUser`
- SignalR: `RoleHub` (Ping diagnostic; not yet registered in `Startup`)
- Migrations tracked under `Migrations/`
- Seeding helper: `Models/utils/DataSeeder.cs`

### Data Relationships (current)
```
ApplicationUser --(ProjectUser)--> Project --(TaskItem)--> TaskItem --(TaskItemUser)--> ApplicationUser
```

---
## ⚙️ Setup & Run (Local)
### Prerequisites
- .NET Core SDK 3.1 (LTS runtime for this project)
- SQL Server LocalDB (default on Windows) or adjust connection string `BusinessCon` in `appsettings.json`

### 1. Restore & Build
```powershell
dotnet restore
dotnet build
```

### 2. Apply Migrations / Create DB
Database will be created automatically on first run if migrations are applied programmatically. If you need CLI control:
```powershell
# Install EF tools if missing
dotnet tool install --global dotnet-ef
# (Optional) Add a migration if you introduce changes
dotnet ef migrations add <Name>
# Update database
dotnet ef database update
```

### 3. Run the App
```powershell
dotnet run --project .\Insightly_project\Insightly_project.csproj
```
Navigate to: `https://localhost:5001/` (or the HTTPS port shown in console)

### 4. Seed Users & Roles
If not automatically seeded (depends where `DataSeeder` is invoked—add during startup if needed):
```csharp
// Example snippet (Startup.Configure after scope creation)
// await DataSeeder.SeedRolesAndAdmin(scope.ServiceProvider);
```
Default credentials (if seeded):
- Admin: `admin@insightly.com` / `Admin@123`
- Team Member: `member@insightly.com` / `Member@123`
- Client: `client@insightly.com` / `Client@123`

---
## 🔐 Security Notes
- Anti-forgery enforced on all form POSTs (`[ValidateAntiForgeryToken]`)
- Security stamp validation interval = 0 ⇒ role changes effective immediately
- Role-based trimming of project/task visibility (TeamMember & Client restricted)
- Consider adding authorization policies for finer-grained data scoping and ownership

---
## 🛣 Application Flow
1. User logs in (redirects based on role to respective dashboard)
2. Admin creates a Project, assigns Team Members / Clients
3. Admin creates Task Items, optionally assigns team members
4. Team Members view assigned tasks (filtered) and update status
5. Clients view their assigned projects & associated tasks (read-only)
6. Future: SignalR pushes notifications when roles or tasks change

---
## 🧪 Testing (Recommended Next Steps)
- Add xUnit or MSTest project for controller & repository logic
- Use EF Core InMemory or SQLite for integration tests
- Mock `UserManager`/`SignInManager` for auth-related tests

---
## 📡 API Summary
Core MVC endpoints are structured by controller/action convention. A concise reference is maintained in [API_DOCUMENTATION.md](./API_DOCUMENTATION.md). That file includes:
- Account (Register/Login/Logout/AccessDenied)
- Admin (User list, role assignment)
- Dashboards
- Projects (CRUD + Assign Team Members / Clients + search)
- TaskItems (CRUD + Status updates + team member JSON)
- SignalR Hub (planned mapping)

---
## 🚀 Enabling SignalR (Optional Enhancement)
Add to `Startup.ConfigureServices`:
```csharp
services.AddSignalR();
```
Add hub mapping in `Startup.Configure` inside `UseEndpoints`:
```csharp
endpoints.MapHub<RoleHub>("/roleHub");
```
Simple client example:
```javascript
const conn = new signalR.HubConnectionBuilder().withUrl('/roleHub').build();
await conn.start();
await conn.invoke('Ping');
conn.on('Pong', () => console.log('Role hub responsive'));
```

---
## 🗺 Roadmap Snapshot
(See full detail in `PROJECT_ROADMAP.md`)
- Milestone A: Stabilize (Admin dashboard stats, SignalR wiring, access scoping)
- Milestone B: Collaboration (comments, filtering, real-time task updates)
- Milestone C: UX polish (Kanban, analytics, attachments)
- Milestone D: Hardening (tests, CI/CD, Docker, logging)

---
## 🧱 Tech Stack
| Layer | Technology |
| --- | --- |
| Framework | ASP.NET Core 3.1 MVC |
| Auth | ASP.NET Core Identity + Roles |
| Data | Entity Framework Core (SQL Server / LocalDB) |
| UI | Razor Views, Bootstrap, jQuery |
| Real-time | SignalR (planned) |
| ORM Tooling | dotnet-ef migrations |

---
## 📂 Project Structure (Key)
```
Controllers/       # MVC controllers (Account, Admin, Projects, TaskItems, Dashboards)
Models/            # Entities, Identity extensions, ViewModels
Views/             # Razor views per controller + shared layout/partials
Migrations/        # EF Core migration history
Hubs/              # SignalR hubs (RoleHub)
wwwroot/           # Static assets (css, js, lib)
API_DOCUMENTATION.md
PROJECT_ROADMAP.md
README.md (this file)
```

---
## 🧯 Troubleshooting
| Issue | Possible Fix |
| ----- | ------------ |
| DB connection errors | Verify LocalDB installed; adjust `BusinessCon` connection string. |
| Role redirects incorrect | Confirm users have exactly one role or adjust redirect logic. |
| 403 errors for Team Member | Ensure they are assigned to the project/task via junction tables. |
| SignalR 404 | Add `services.AddSignalR()` + endpoint mapping. |

---
## 🤝 Contributions
For student/team collaboration: create feature branches, open PRs referencing roadmap items, ensure migrations are additive and named meaningfully.

---
## 📜 License
Specify a license (e.g., MIT) here if making the project public.

---
## ✅ At a Glance
- Secure, role-aware project/task platform
- Extensible data model with junction tables
- Ready for real-time and collaboration enhancements
- Documented APIs in `API_DOCUMENTATION.md`

> Next improvement suggestion: register SignalR hub + add a light test suite.

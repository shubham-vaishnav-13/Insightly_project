# Insightly Project

Lightweight role-based project & task management platform built with ASP.NET Core MVC 3.1 and Entity Framework Core.

---

## ▶️ Demo
- Watch a short demo: https://youtu.be/N-zglUTem6Y?si=pqGYsl3I6CwKsTkY

---

## ✨ Implemented Features (only)
- User authentication & role-based authorization using ASP.NET Core Identity (roles: Admin, TeamMember, Client).
- Project management (CRUD by Admin; assigned Team Members & Clients).
- Task management:
    - Tasks are created/managed by Admin.
    - Tasks support many-to-many assignment to users via junction table (`TaskItemUser`).
    - Assigned Team Members can update task status.
- Role-aware dashboards (Admin, Team Member, Client) with scoped visibility.
- Project search by name/description.
- Data seeding for roles and default users (seeded credentials may be present in the project).
- EF Core models, migrations, and repository patterns used for data access.

> Note: Features listed above are implemented. Other items mentioned previously (real-time updates, task comments, attachments, Kanban board, advanced client scoping, extensive client task editing, analytics, email notifications, CI/CD, Dockerization) are roadmap items and are not implemented in this codebase.

---
## 🧩 Planned / Roadmap Highlights
- Real-time updates (future: add SignalR hub & notifications)
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
| View Tasks | ✅ (all) | ✅ (assigned only) | ❌ |
| Change Task Status | ✅ | ✅ (if assigned) | ❌ |

---
## 🏗 Architecture Overview

- **Presentation Layer:** ASP.NET Core MVC  
    &nbsp;&nbsp;• Controllers and Razor Views for UI and routing

- **Identity & Roles:**  
    &nbsp;&nbsp;• `ApplicationUser` extends ASP.NET Core Identity  
    &nbsp;&nbsp;• Role-based access (Admin, Team Member, Client)

- **Data Access:**  
    &nbsp;&nbsp;• Entity Framework Core with `ApplicationDbContext`

- **Core Entities:**  
    &nbsp;&nbsp;• `Project`  
    &nbsp;&nbsp;&nbsp;&nbsp;– One-to-many: `Project` → `TaskItem`  
    &nbsp;&nbsp;&nbsp;&nbsp;– Many-to-many: `Project` ↔ `ApplicationUser` via `ProjectUser`  
    &nbsp;&nbsp;• `TaskItem`  
    &nbsp;&nbsp;&nbsp;&nbsp;– Many-to-many: `TaskItem` ↔ `ApplicationUser` via `TaskItemUser`  
    &nbsp;&nbsp;• Linking tables: `ProjectUser`, `TaskItemUser`

- **Migrations:**  
    &nbsp;&nbsp;• Tracked in `Migrations/` directory

- **Data Seeding:**  
    &nbsp;&nbsp;• Helper class: `Models/utils/DataSeeder.cs`

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
Open **Package Manager Console** in Visual Studio and run:
```powershell
# Restore NuGet packages
Update-Package -reinstall
# Build the solution
Build
```

### 2. Apply Migrations / Create DB
Database will be created automatically on first run if migrations are applied programmatically. For manual control via Package Manager Console:
```powershell
# Add a migration (if you introduce changes)
Add-Migration <Name>
# Update database
Update-Database
```

### 3. Run the App
Press **F5** or click **Start** in Visual Studio to launch the application.
Navigate to: `https://localhost:5001/` (or the HTTPS port shown in the output window)

### 4. Seed Users & Roles
If not automatically seeded (depends where `DataSeeder` is invoked—add during startup if needed):

Default credentials (if seeded):
- Admin: `admin@insightly.com` / `Admin@123`
- Team Member: `member@insightly.com` / `Member@123`
- Client: `client@insightly.com` / `Client@123`


---
## 🔐 Security Notes
- Anti-forgery enforced on all form POSTs (`[ValidateAntiForgeryToken]`)
- Role-based trimming of project/task visibility (TeamMember & Client restricted)
- Consider adding authorization policies for finer-grained data scoping and ownership

---
## 🛣 Application Flow
1. User logs in (redirects based on role to respective dashboard)
2. Admin creates a Project, assigns Team Members / Clients
3. Admin creates Task Items, optionally assigns team members
4. Team Members view assigned tasks (filtered) and update status
5. Clients view their assigned projects & associated tasks (read-only)

---
## 🧪 Testing (Recommended Next Steps)
- Add xUnit or MSTest project for controller & repository logic
- Use EF Core InMemory or SQLite for integration tests
- Mock `UserManager`/`SignInManager` for auth-related tests

---
## 📡 API Summary
Core MVC endpoints are structured by controller/action convention. 
- Account (Register/Login/Logout/AccessDenied)
- Admin (User list, role assignment)
- Dashboards
- Projects (CRUD + Assign Team Members / Clients + search)
- TaskItems (CRUD + Status updates + team member JSON)

---


## 🧱 Tech Stack
| Layer | Technology |
| --- | --- |
| Framework | ASP.NET Core 3.1 MVC |
| Auth | ASP.NET Core Identity + Roles |
| Data | Entity Framework Core (SQL Server / LocalDB) |
| UI | Razor Views, Bootstrap, jQuery |
| Real-time | (Planned: SignalR) |
| ORM Tooling | dotnet-ef migrations |

---
## 📂 Project Structure (Key)
```
Controllers/       # MVC controllers (Account, Admin, Projects, TaskItems, Dashboards)
Models/            # Entities, Identity extensions, ViewModels
Views/             # Razor views per controller + shared layout/partials
Migrations/        # EF Core migration history
wwwroot/           # Static assets (css, js, lib)
API_DOCUMENTATION.md
README.md (this file)
```

---
## 🤝 Contributions
For student/team collaboration: create feature branches, open PRs referencing roadmap items, ensure migrations are additive and named meaningfully.

---
## � Project Authors
Primary creators and maintainers:

- Shubham Vaishnav – [@shubham-vaishnav-13](https://github.com/shubham-vaishnav-13)
- Nimit Rangani – [@nimit-ddu](https://github.com/nimit-ddu)
- Kunjal Virapariya – [@Kunjal-82](https://github.com/Kunjal-82)

Feel free to reach out via GitHub issues for questions or collaboration proposals.

---
## �📜 License
Specify a license (e.g., MIT) here if making the project public.

---
## ✅ At a Glance
- Secure, role-aware project/task platform
- Extensible data model with junction tables
- Ready for planned real-time and collaboration enhancements


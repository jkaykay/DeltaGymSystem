# Delta Gym System

A gym management system built with ASP.NET Core (.NET 10). The solution consists of two projects:

| Project | Description | Default URL |
|---|---|---|
| **GymSystem.Api** | REST API — Identity, JWT authentication, EF Core + SQL Server | `https://localhost:7183` |
| **GymSystem.Web** | MVC front-end — cookie authentication, consumes the API | `https://localhost:7202` |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, or full instance)
- (Optional) [Visual Studio 2022 17.14+](https://visualstudio.microsoft.com/) with the **ASP.NET and web development** workload

---

## 1. Clone the Repository

```bash
git clone https://github.com/jkaykay/DeltaGymSystem.git
cd DeltaGymSystem
```

---

## 2. Restore Dependencies

From the solution root directory, restore NuGet packages for both projects:

```bash
dotnet restore
```

---

## 3. Find Your SQL Server Connection String

You need a connection string for a SQL Server instance. Common examples:

| Instance Type | Example Connection String |
|---|---|
| **LocalDB** | `Server=(localdb)\MSSQLLocalDB;Database=DeltaGymDb;Trusted_Connection=True;MultipleActiveResultSets=true` |
| **SQL Server Express** | `Server=.\SQLEXPRESS;Database=DeltaGymDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True` |
| **Named instance** | `Server=YOUR_SERVER_NAME;Database=DeltaGymDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True` |

> **Tip:** Open **SQL Server Object Explorer** in Visual Studio (View → SQL Server Object Explorer) to find your server name, or run `sqllocaldb info` in a terminal to list LocalDB instances.

---

## 4. Configure User Secrets

Sensitive configuration values are stored in **user secrets** and are **not** committed to source control. Each project has its own secrets store.

### GymSystem.Api

Navigate to the API project and set the required secrets:

```bash
cd GymSystem.Api

# Database connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=DeltaGymDb;Trusted_Connection=True;MultipleActiveResultSets=true"

# JWT settings
dotnet user-secrets set "Jwt:Key" "YourSuperSecretKeyThatIsAtLeast32Characters!"
dotnet user-secrets set "Jwt:Issuer" "GymSystem.Api"
dotnet user-secrets set "Jwt:Audience" "GymSystem.Web"
dotnet user-secrets set "Jwt:ExpiryInMinutes" "60"

# Seed admin password
dotnet user-secrets set "SeedAdmin:Password" "Admin@123456"

cd ..
```

### GymSystem.Web

Navigate to the Web project and set the required secrets:

```bash
cd GymSystem.Web

# Base URL of the API (must match the API launch URL)
dotnet user-secrets set "GymApi:BaseUrl" "https://localhost:7183"

cd ..
```

or....
> **Visual Studio shortcut:** Right-click a project in Solution Explorer → **Manage User Secrets** to open the `secrets.json` file directly.

### Example secrets.json for GymSystem.Api

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=DeltaGymDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32Characters!",
    "Issuer": "GymSystem.Api",
    "Audience": "GymSystem.Web",
    "ExpiryInMinutes": "60"
  },
  "SeedAdmin": {
    "Password": "Admin@123456"
  }
}
```

### Example secrets.json for GymSystem.Web

```json
{
  "GymApi": {
    "BaseUrl": "https://localhost:7183"
  }
}
```

---

## 5. Apply Database Migrations

Apply the existing migration to create the database:

```bash
dotnet ef database update --project GymSystem.Api
```

This creates the database specified in your connection string, applies the **Initial User Migration**, and the application will seed default roles (`Admin`, `Staff`, `Trainer`, `Member`) and a default admin account on first run.

### Default Admin Credentials

| Field | Value |
|---|---|
| Username | `admin` |
| Email | `admin@localhost` |
| Password | *(the value you set in `SeedAdmin:Password`)* |

---

## 6. Run the Application

Both projects must be running simultaneously — the Web front-end calls the API.

### Option A — Visual Studio (recommended)

1. Right-click the **solution** in Solution Explorer → **Configure Startup Projects...**
2. Select **Multiple startup projects**
3. Set both **GymSystem.Api** and **GymSystem.Web** to **Start**
4. Press **F5** (or Ctrl+F5 for without debugging)

### Option B — Command Line

Open two separate terminals from the solution root:

**Terminal 1 — API:**

```bash
dotnet run --project GymSystem.Api --launch-profile https
```

**Terminal 2 — Web:**

```bash
dotnet run --project GymSystem.Web --launch-profile https
```

Then open your browser and navigate to `https://localhost:7202`.

---

## Project Structure

```
DeltaGymSystem/
+-- GymSystem.Api/              # REST API
|   +-- Controllers/            # API endpoints (Auth, Members, Staff)
|   +-- Data/                   # DbContext and seed data
|   +-- DTOs/                   # Request/response models
|   +-- Migrations/             # EF Core migrations
|   +-- Models/                 # Domain models (ApplicationUser)
|   +-- Services/               # Token generation service
+-- GymSystem.Web/              # MVC front-end
|   +-- Areas/
|   |   +-- Management/         # Admin/staff management area
|   |   +-- Member/             # Member-facing area
|   |   +-- Trainer/            # Trainer-facing area
|   +-- Controllers/            # Public controllers (Home, About)
|   +-- DTOs/                   # API response DTOs
|   +-- Services/               # API consumption services
|   +-- ViewModels/             # Shared view models
+-- README.md
```

---

## Troubleshooting

| Problem | Solution |
|---|---|
| `Connection string 'DefaultConnection' not found` | Ensure user secrets are set for **GymSystem.Api** (see step 4) |
| `Jwt:Key not found in configuration` | Ensure `Jwt:Key` is set in API user secrets |
| `GymApi:BaseUrl is not configured` | Ensure user secrets are set for **GymSystem.Web** (see step 4) |
| API returns 401 Unauthorized | Check that `Jwt:Issuer` and `Jwt:Audience` match between the API config and the tokens being sent |
| `dotnet ef` not found | Run `dotnet tool install --global dotnet-ef` |
| Database migration fails | Verify your connection string points to a valid SQL Server instance |


---
## Git Workflow

### Branch Structure

This project follows a branching strategy:
- **main** - Production-ready code
- **development** - Integration branch for features
- **feature/*** - Individual feature branches

### Working on a Feature

#### 1. Checkout to Development Branch

Start by pulling the latest changes from the development branch:
```bash
git checkout development
git pull origin development
```

#### 2. Create or Checkout to a Feature Branch

**Create a new feature branch:**
```bash
git checkout -b feature/your-feature-name
```
**Or switch to an existing feature branch:**
```bash
git checkout feature/your-feature-name
```
Use descriptive branch names, e.g.: `feature/user-authentication`, `feature/gym-class-scheduling`

#### 3. Commit Your Changes
Make your changes, then stage and commit them:
```bash
git add .
git commit -m "Brief description of changes"
```

#### 4. Push Your Feature Branch
```bash
git push origin feature/your-feature-name
```
If pushing for the first time, you may need to set the upstream:
```bash
git push -u origin feature/your-feature-name
```

### 5. Initiating a Pull Request

1. **Push your feature branch** to the remote repository (see Step 4 above)

2. **Open a Pull Request on GitHub:**
   - Go to [https://github.com/jkaykay/delta-gym-system](https://github.com/jkaykay/delta-gym-system)
   - Click the **"Pull requests"** tab
   - Click **"New pull request"**
   - Set the base branch to **development** and compare branch to **feature/your-feature-name**
   - Add a descriptive title and description of your changes
   - Click **"Create pull request"**

3. **Request code review** from team members and address any feedback

4. **Merge the PR** once approved

### 6. Syncing with Development (Merging)

If development branch has been updated and you want to pull those changes into your feature branch:
```bash
git fetch origin
git merge origin/development
```

**Merge conflicts?**
- Use `git status` to see conflicting files
- Resolve conflicts in your editor, then `git add` and `git commit`

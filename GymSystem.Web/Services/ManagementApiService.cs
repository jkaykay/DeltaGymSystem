using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services
{
    /// <summary>
    /// Implements <see cref="IManagementApiService"/>.
    /// This is the largest service in the web layer — it provides every CRUD operation
    /// that Management area controllers need (members, staff, trainers, branches, tiers,
    /// rooms, classes, sessions, subscriptions, payments, bookings, equipment, schedules,
    /// attendances, and QR code scanning).
    /// All methods call the backend REST API via a preconfigured HttpClient ("GymApi").
    /// </summary>
    public class ManagementApiService : IManagementApiService
    {
        // The HttpClient pre-loaded with the API base URL and token handler.
        private readonly HttpClient _http;

        // Constructor — DI injects the factory; we create the named client once.
        public ManagementApiService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("GymApi");
        }

        // =================================================================
        // MEMBERS — CRUD + toggle active + totals + recent sign-ups
        // =================================================================

        /// <summary>
        /// Fetches a paged, searchable, filterable list of gym members from the API.
        /// Query parameters are built dynamically based on which filters the user applied.
        /// </summary>
        public async Task<PagedResult<UserDTO>> GetMembersAsync(int page = 1, int pageSize = 10,
            string? search = null, bool? active = null,
            DateTime? joinedFrom = null, DateTime? joinedTo = null,
            string? sortBy = null, string? sortDir = null)
        {

            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (active.HasValue)
                queryParams.Add($"active={active.Value}");
            if (joinedFrom.HasValue)
                queryParams.Add($"joinedFrom={joinedFrom.Value:yyyy-MM-dd}");
            if (joinedTo.HasValue)
                queryParams.Add($"joinedTo={joinedTo.Value:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(sortBy))
                queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
            if (!string.IsNullOrWhiteSpace(sortDir))
                queryParams.Add($"sortDir={Uri.EscapeDataString(sortDir)}");

            var url = $"api/member?{string.Join("&", queryParams)}";

            var result = await _http.GetFromJsonAsync<PagedResult<UserDTO>>(url);
            return result ?? new PagedResult<UserDTO>();
        }

        /// <summary>Fetches a single member by their unique ID.</summary>
        public async Task<UserDTO?> GetMemberAsync(string id)
        {
            var response = await _http.GetAsync($"api/member/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserDTO>();
        }

        /// <summary>Creates a new gym member via POST api/member.</summary>
        public async Task<bool> CreateMemberAsync(CreateMemberViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/member", new
            {
                model.Email,
                UserName = model.Username,
                model.FirstName,
                model.LastName,
                model.Password,
                model.PhoneNumber
            });
            return response.IsSuccessStatusCode;
        }

        /// <summary>Updates an existing member's details via PUT api/member/{id}.</summary>
        public async Task<bool> UpdateMemberAsync(string id, EditMemberViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/member/{id}", new
            {
                model.Email,
                model.FirstName,
                model.LastName,
                model.Active,
                model.PhoneNumber
            });
            return response.IsSuccessStatusCode;
        }

        /// <summary>Toggles a member's active/inactive status via POST api/member/{id}/toggle-active.</summary>
        public async Task<bool> ToggleMemberActiveAsync(string id)
        {
            var response = await _http.PostAsync($"api/member/{id}/toggle-active", null);
            return response.IsSuccessStatusCode;
        }

        /// <summary>Permanently deletes a member from the system.</summary>
        public async Task<bool> DeleteMemberAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/member/{id}");
            return response.IsSuccessStatusCode;
        }

        /// <summary>Fetches all members (for dropdowns) by requesting a large page size.</summary>
        public async Task<List<UserDTO>> GetAllMembersAsync()
        {
            var result = await _http.GetFromJsonAsync<PagedResult<UserDTO>>(
                "api/member?page=1&pageSize=999");
            return result?.Items ?? new List<UserDTO>();
        }

        // =================================================================
        // STAFF — CRUD + totals
        // =================================================================

        /// <summary>Fetches a paged list of staff members with optional search and date filters.</summary>
        public async Task<PagedResult<UserDTO>> GetStaffAsync(int page = 1, int pageSize = 10,
            string? search = null,
            DateTime? hiredFrom = null, DateTime? hiredTo = null,
            string? sortBy = null, string? sortDir = null)
        {
            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (hiredFrom.HasValue)
                queryParams.Add($"hiredFrom={hiredFrom.Value:yyyy-MM-dd}");
            if (hiredTo.HasValue)
                queryParams.Add($"hiredTo={hiredTo.Value:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(sortBy))
                queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
            if (!string.IsNullOrWhiteSpace(sortDir))
                queryParams.Add($"sortDir={Uri.EscapeDataString(sortDir)}");

            var url = $"api/staff?{string.Join("&", queryParams)}";

            var result = await _http.GetFromJsonAsync<PagedResult<UserDTO>>(url);
            return result ?? new PagedResult<UserDTO>();
        }

        /// <summary>Fetches a single staff member by ID.</summary>
        public async Task<UserDTO?> GetStaffMemberAsync(string id)
        {
            var response = await _http.GetAsync($"api/staff/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserDTO>();
        }

        /// <summary>Creates a new staff member via POST api/staff.</summary>
        public async Task<bool> CreateStaffAsync(CreateStaffViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/staff", new
            {
                model.Email,
                model.FirstName,
                model.LastName,
                model.Password,
                model.EmployeeId,
                model.Role,
                model.PhoneNumber
            });
            return response.IsSuccessStatusCode;
        }

        /// <summary>Updates a staff member's details via PUT api/staff/{id}.</summary>
        public async Task<bool> UpdateStaffAsync(string id, EditStaffViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/staff/{id}", new
            {
                model.Email,
                model.FirstName,
                model.LastName,
                model.EmployeeId,
                model.BranchId,
                model.PhoneNumber
            });
            return response.IsSuccessStatusCode;
        }

        /// <summary>Deletes a staff member by ID.</summary>
        public async Task<bool> DeleteStaffAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/staff/{id}");
            return response.IsSuccessStatusCode;
        }

        /// <summary>Fetches all staff (for dropdowns).</summary>
        public async Task<List<UserDTO>> GetAllStaffAsync()
        {
            var result = await _http.GetFromJsonAsync<PagedResult<UserDTO>>(
                "api/staff?page=1&pageSize=999");
            return result?.Items ?? new List<UserDTO>();
        }

        /// <summary>
        /// Combines all staff and all trainers into a single "employees" list.
        /// Used for schedule dropdowns where any employee can be assigned.
        /// Duplicates (if someone is both staff and trainer) are removed by ID.
        /// </summary>
        public async Task<List<UserDTO>> GetAllEmployeesAsync()
        {
            var staff = await GetAllStaffAsync();
            var trainers = await GetAllTrainersAsync();

            var combined = new Dictionary<string, UserDTO>();
            foreach (var s in staff) combined[s.Id] = s;
            foreach (var t in trainers) combined.TryAdd(t.Id, t);

            return combined.Values.ToList();
        }

        /// <summary>Filters the combined employees list to only those in a specific branch.</summary>
        public async Task<List<UserDTO>> GetEmployeesByBranchAsync(int branchId)
        {
            var all = await GetAllEmployeesAsync();
            return all.Where(e => e.BranchId == branchId).ToList();
        }

        /// <summary>Gets the total member count for the dashboard summary card.</summary>
        public async Task<CountResponse> GetTotalMembersAsync()
        {
            var response = await _http.GetAsync("api/member/total");
            if (!response.IsSuccessStatusCode)
                return new CountResponse();
            return await response.Content.ReadFromJsonAsync<CountResponse>() ?? new CountResponse();
        }

        /// <summary>Gets the total staff count for the dashboard summary card.</summary>
        public async Task<CountResponse> GetTotalStaffAsync()
        {
            var response = await _http.GetAsync("api/staff/total");
            if (!response.IsSuccessStatusCode)
                return new CountResponse();
            return await response.Content.ReadFromJsonAsync<CountResponse>() ?? new CountResponse();
        }

        /// <summary>Gets the most recent member sign-ups for the dashboard.</summary>
        public async Task<List<UserDTO>> GetRecentSignupsAsync()
        {
            var result = await _http.GetFromJsonAsync<List<UserDTO>>("api/member/recents");
            return result ?? new List<UserDTO>();
        }

        // =================================================================
        // BRANCHES — CRUD
        // =================================================================

        /// <summary>Fetches a paged list of gym branches.</summary>
        public async Task<PagedResult<BranchDTO>> GetBranchesAsync(int page = 1, int pageSize = 10,
            string? search = null,
            string? sortBy = null, string? sortDir = null)
        {
            var queryParams = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (!string.IsNullOrWhiteSpace(sortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
            if (!string.IsNullOrWhiteSpace(sortDir)) queryParams.Add($"sortDir={Uri.EscapeDataString(sortDir)}");
            var result = await _http.GetFromJsonAsync<PagedResult<BranchDTO>>($"api/branch?{string.Join("&", queryParams)}");
            return result ?? new PagedResult<BranchDTO>();
        }

        /// <summary>Fetches all branches (for dropdowns).</summary>
        public async Task<List<BranchDTO>> GetAllBranchesAsync()
        {
            var result = await _http.GetFromJsonAsync<PagedResult<BranchDTO>>(
                "api/branch?page=1&pageSize=999");
            return result?.Items ?? new List<BranchDTO>();
        }

        /// <summary>Fetches a single branch by ID.</summary>
        public async Task<BranchDTO?> GetBranchAsync(int id)
        {
            var response = await _http.GetAsync($"api/branch/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<BranchDTO>();
        }

        /// <summary>Creates a new branch.</summary>
        public async Task<bool> CreateBranchAsync(CreateBranchViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/branch", new
            {
                model.Address,
                model.City,
                model.Province,
                model.PostCode,
                model.OpenDate
            });
            return response.IsSuccessStatusCode;
        }

        /// <summary>Updates an existing branch.</summary>
        public async Task<bool> UpdateBranchAsync(int id, EditBranchViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/branch/{id}", new
            {
                model.Address,
                model.City,
                model.Province,
                model.PostCode
            });
            return response.IsSuccessStatusCode;
        }

        /// <summary>Deletes a branch by ID.</summary>
        public async Task<bool> DeleteBranchAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/branch/{id}");
            return response.IsSuccessStatusCode;
        }

        // =================================================================
        // TIERS — CRUD (membership pricing plans)
        // =================================================================

        /// <summary>Fetches a paged list of membership tiers.</summary>
        public async Task<PagedResult<TierDTO>> GetTiersAsync(int page = 1, int pageSize = 10,
            string? search = null,
            string? sortBy = null, string? sortDir = null)
        {
            var queryParams = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (!string.IsNullOrWhiteSpace(sortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
            if (!string.IsNullOrWhiteSpace(sortDir)) queryParams.Add($"sortDir={Uri.EscapeDataString(sortDir)}");
            var result = await _http.GetFromJsonAsync<PagedResult<TierDTO>>($"api/tier?{string.Join("&", queryParams)}");
            return result ?? new PagedResult<TierDTO>();
        }

        /// <summary>Fetches all tiers (for dropdowns).</summary>
        public async Task<List<TierDTO>> GetAllTiersAsync()
        {
            var result = await _http.GetFromJsonAsync<PagedResult<TierDTO>>(
                "api/tier?page=1&pageSize=999");
            return result?.Items ?? new List<TierDTO>();
        }

        /// <summary>Fetches a single tier by its name (tiers use name as the key).</summary>
        public async Task<TierDTO?> GetTierAsync(string tierName)
        {
            var response = await _http.GetAsync($"api/tier/{tierName}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<TierDTO>();
        }

        /// <summary>Creates a new membership tier.</summary>
        public async Task<bool> CreateTierAsync(CreateTierViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/tier", new
            {
                model.TierName,
                model.Price
            });
            return response.IsSuccessStatusCode;
        }

        /// <summary>Updates a tier's price.</summary>
        public async Task<bool> UpdateTierAsync(string tierName, EditTierViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/tier/{tierName}", new
            {
                model.Price
            });
            return response.IsSuccessStatusCode;
        }

        /// <summary>Deletes a tier by name.</summary>
        public async Task<bool> DeleteTierAsync(string tierName)
        {
            var response = await _http.DeleteAsync($"api/tier/{tierName}");
            return response.IsSuccessStatusCode;
        }

        // =================================================================
        // TRAINERS — CRUD + totals
        // =================================================================

        /// <summary>Gets the total trainer count for the dashboard.</summary>
        public async Task<CountResponse> GetTotalTrainersAsync()
        {
            var response = await _http.GetAsync("api/trainer/total");
            if (!response.IsSuccessStatusCode)
                return new CountResponse();
            return await response.Content.ReadFromJsonAsync<CountResponse>() ?? new CountResponse();
        }

        /// <summary>Fetches a paged list of trainers with optional search and hire-date filters.</summary>
        public async Task<PagedResult<UserDTO>> GetTrainersAsync(int page = 1, int pageSize = 10,
    string? search = null,
    DateTime? hiredFrom = null, DateTime? hiredTo = null,
    string? sortBy = null, string? sortDir = null)
        {
            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (hiredFrom.HasValue)
                queryParams.Add($"hiredFrom={hiredFrom.Value:yyyy-MM-dd}");
            if (hiredTo.HasValue)
                queryParams.Add($"hiredTo={hiredTo.Value:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(sortBy))
                queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
            if (!string.IsNullOrWhiteSpace(sortDir))
                queryParams.Add($"sortDir={Uri.EscapeDataString(sortDir)}");

            var url = $"api/trainer?{string.Join("&", queryParams)}";

            var result = await _http.GetFromJsonAsync<PagedResult<UserDTO>>(url);
            return result ?? new PagedResult<UserDTO>();
        }

        /// <summary>Fetches a single trainer by ID.</summary>
        public async Task<UserDTO?> GetTrainerAsync(string id)
        {
            var response = await _http.GetAsync($"api/trainer/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserDTO>();
        }

        /// <summary>Creates a new trainer.</summary>
        public async Task<bool> CreateTrainerAsync(CreateTrainerViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/trainer", new
            {
                model.Email,
                model.FirstName,
                model.LastName,
                model.Password,
                model.EmployeeId,
                model.BranchId,
                model.PhoneNumber
            });
            return response.IsSuccessStatusCode;
        }

        /// <summary>Updates an existing trainer's details.</summary>
        public async Task<bool> UpdateTrainerAsync(string id, EditTrainerViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/trainer/{id}", new
            {
                model.Email,
                model.FirstName,
                model.LastName,
                model.EmployeeId,
                model.BranchId,
                model.PhoneNumber
            });
            return response.IsSuccessStatusCode;
        }

        /// <summary>Deletes a trainer by ID.</summary>
        public async Task<bool> DeleteTrainerAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/trainer/{id}");
            return response.IsSuccessStatusCode;
        }

        /// <summary>Fetches all trainers (for dropdowns).</summary>
        public async Task<List<UserDTO>> GetAllTrainersAsync()
        {
            var result = await _http.GetFromJsonAsync<PagedResult<UserDTO>>(
                "api/trainer?page=1&pageSize=999");
            return result?.Items ?? new List<UserDTO>();
        }

        // =================================================================
        // ROOMS — CRUD
        // =================================================================

        /// <summary>Fetches a paged list of rooms with optional branch/room-number filters.</summary>
        public async Task<PagedResult<RoomDTO>> GetRoomsAsync(int page = 1, int pageSize = 10,
            int? branchId = null, int? roomNumber = null,
            string? sortBy = null, string? sortDir = null)
        {
            var queryParams = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (branchId.HasValue) queryParams.Add($"branchId={branchId.Value}");
            if (roomNumber.HasValue) queryParams.Add($"roomNumber={roomNumber.Value}");
            if (!string.IsNullOrWhiteSpace(sortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
            if (!string.IsNullOrWhiteSpace(sortDir)) queryParams.Add($"sortDir={Uri.EscapeDataString(sortDir)}");
            var result = await _http.GetFromJsonAsync<PagedResult<RoomDTO>>($"api/room?{string.Join("&", queryParams)}");
            return result ?? new PagedResult<RoomDTO>();
        }

        /// <summary>Fetches a single room by ID.</summary>
        public async Task<RoomDTO?> GetRoomAsync(int id)
        {
            var response = await _http.GetAsync($"api/room/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<RoomDTO>();
        }

        /// <summary>Creates a new room.</summary>
        public async Task<bool> CreateRoomAsync(CreateRoomViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/room", new
            {
                model.RoomNumber,
                model.BranchId,
                model.MaxCapacity
            });
            return response.IsSuccessStatusCode;
        }

        /// <summary>Updates an existing room.</summary>
        public async Task<bool> UpdateRoomAsync(int id, EditRoomViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/room/{id}", new
            {
                model.RoomNumber,
                model.BranchId,
                model.MaxCapacity
            });
            return response.IsSuccessStatusCode;
        }

        /// <summary>Deletes a room by ID.</summary>
        public async Task<bool> DeleteRoomAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/room/{id}");
            return response.IsSuccessStatusCode;
        }

        /// <summary>Fetches all rooms (for dropdowns).</summary>
        public async Task<List<RoomDTO>> GetAllRoomsAsync()
        {
            var result = await _http.GetFromJsonAsync<PagedResult<RoomDTO>>(
                "api/room?page=1&pageSize=999");
            return result?.Items ?? new List<RoomDTO>();
        }

        // =================================================================
        // CLASSES — CRUD (e.g. Yoga, Spin, Boxing)
        // =================================================================

        /// <summary>Fetches a paged list of gym classes.</summary>
        public async Task<PagedResult<ClassDTO>> GetClassesAsync(int page = 1, int pageSize = 10,
            string? search = null,
            string? sortBy = null, string? sortDir = null)
        {
            var queryParams = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (!string.IsNullOrWhiteSpace(sortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
            if (!string.IsNullOrWhiteSpace(sortDir)) queryParams.Add($"sortDir={Uri.EscapeDataString(sortDir)}");
            var result = await _http.GetFromJsonAsync<PagedResult<ClassDTO>>($"api/class?{string.Join("&", queryParams)}");
            return result ?? new PagedResult<ClassDTO>();
        }

        public async Task<ClassDTO?> GetClassAsync(int id)
        {
            var response = await _http.GetAsync($"api/class/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ClassDTO>();
        }

        public async Task<bool> CreateClassAsync(CreateClassViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/class", new
            {
                model.Subject,
                model.UserId
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateClassAsync(int id, EditClassViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/class/{id}", new
            {
                model.Subject,
                model.UserId
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteClassAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/class/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<ClassDTO>> GetAllClassesAsync()
        {
            var result = await _http.GetFromJsonAsync<PagedResult<ClassDTO>>(
                "api/class?page=1&pageSize=999");
            return result?.Items ?? new List<ClassDTO>();
        }

        // =================================================================
        // SESSIONS — CRUD (a specific occurrence of a class in a room)
        // =================================================================

        /// <summary>Fetches a paged list of sessions with optional date/room filters.</summary>
        public async Task<PagedResult<SessionDTO>> GetSessionsAsync(int page = 1, int pageSize = 10,
            string? search = null,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            int? roomId = null,
            string? sortBy = null, string? sortDir = null)
        {
            var queryParams = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (dateFrom.HasValue) queryParams.Add($"dateFrom={dateFrom.Value:yyyy-MM-dd}");
            if (dateTo.HasValue) queryParams.Add($"dateTo={dateTo.Value:yyyy-MM-dd}");
            if (roomId.HasValue) queryParams.Add($"roomId={roomId.Value}");
            if (!string.IsNullOrWhiteSpace(sortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
            if (!string.IsNullOrWhiteSpace(sortDir)) queryParams.Add($"sortDir={Uri.EscapeDataString(sortDir)}");
            var result = await _http.GetFromJsonAsync<PagedResult<SessionDTO>>($"api/session?{string.Join("&", queryParams)}");
            return result ?? new PagedResult<SessionDTO>();
        }

        public async Task<SessionDTO?> GetSessionAsync(int id)
        {
            var response = await _http.GetAsync($"api/session/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<SessionDTO>();
        }

        public async Task<bool> CreateSessionAsync(CreateSessionViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/session", new
            {
                model.Start,
                model.End,
                model.RoomId,
                model.ClassId,
                model.MaxCapacity
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateSessionAsync(int id, EditSessionViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/session/{id}", new
            {
                model.Start,
                model.End,
                model.RoomId,
                model.MaxCapacity
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteSessionAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/session/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<SessionDTO>> GetAllSessionsAsync()
        {
            var result = await _http.GetFromJsonAsync<PagedResult<SessionDTO>>(
                "api/session?page=1&pageSize=999");
            return result?.Items ?? new List<SessionDTO>();
        }

        // =================================================================
        // SUBSCRIPTIONS — CRUD (member’s plan under a tier)
        // =================================================================

        /// <summary>Fetches a paged list of subscriptions with optional state/tier/date filters.</summary>
        public async Task<PagedResult<SubscriptionDTO>> GetSubscriptionsAsync(int page = 1, int pageSize = 10,
            string? search = null, int? state = null, string? tierName = null,
            DateTime? startFrom = null, DateTime? startTo = null,
            string? sortBy = null, string? sortDir = null)
        {
            var queryParams = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (state.HasValue) queryParams.Add($"state={state.Value}");
            if (!string.IsNullOrWhiteSpace(tierName)) queryParams.Add($"tierName={Uri.EscapeDataString(tierName)}");
            if (startFrom.HasValue) queryParams.Add($"startFrom={startFrom.Value:yyyy-MM-dd}");
            if (startTo.HasValue) queryParams.Add($"startTo={startTo.Value:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(sortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
            if (!string.IsNullOrWhiteSpace(sortDir)) queryParams.Add($"sortDir={Uri.EscapeDataString(sortDir)}");
            var result = await _http.GetFromJsonAsync<PagedResult<SubscriptionDTO>>($"api/subscription?{string.Join("&", queryParams)}");
            return result ?? new PagedResult<SubscriptionDTO>();
        }

        public async Task<SubscriptionDTO?> GetSubscriptionAsync(int id)
        {
            var response = await _http.GetAsync($"api/subscription/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<SubscriptionDTO>();
        }

        public async Task<bool> CreateSubscriptionAsync(CreateSubscriptionViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/subscription", new
            {
                model.TierName,
                model.UserId
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateSubscriptionAsync(int id, EditSubscriptionViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/subscription/{id}", new
            {
                model.TierName,
                model.State,
                model.StartDate,
                model.EndDate
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteSubscriptionAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/subscription/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<SubscriptionDTO>> GetAllSubscriptionsAsync()
        {
            var result = await _http.GetFromJsonAsync<PagedResult<SubscriptionDTO>>(
                "api/subscription?page=1&pageSize=999");
            return result?.Items ?? new List<SubscriptionDTO>();
        }

        // =================================================================
        // PAYMENTS — CRUD
        // =================================================================

        /// <summary>Fetches a paged list of payments with optional date/amount filters.</summary>
        public async Task<PagedResult<PaymentDTO>> GetPaymentsAsync(int page = 1, int pageSize = 10,
            string? search = null,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            decimal? minAmount = null, decimal? maxAmount = null,
            string? sortBy = null, string? sortDir = null)
        {
            var queryParams = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (dateFrom.HasValue) queryParams.Add($"dateFrom={dateFrom.Value:yyyy-MM-dd}");
            if (dateTo.HasValue) queryParams.Add($"dateTo={dateTo.Value:yyyy-MM-dd}");
            if (minAmount.HasValue) queryParams.Add($"minAmount={minAmount.Value}");
            if (maxAmount.HasValue) queryParams.Add($"maxAmount={maxAmount.Value}");
            if (!string.IsNullOrWhiteSpace(sortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
            if (!string.IsNullOrWhiteSpace(sortDir)) queryParams.Add($"sortDir={Uri.EscapeDataString(sortDir)}");
            var result = await _http.GetFromJsonAsync<PagedResult<PaymentDTO>>($"api/payment?{string.Join("&", queryParams)}");
            return result ?? new PagedResult<PaymentDTO>();
        }

        public async Task<PaymentDTO?> GetPaymentAsync(int id)
        {
            var response = await _http.GetAsync($"api/payment/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<PaymentDTO>();
        }

        public async Task<bool> CreatePaymentAsync(CreatePaymentViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/payment", new
            {
                model.Amount,
                model.UserId,
                model.SubId
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeletePaymentAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/payment/{id}");
            return response.IsSuccessStatusCode;
        }

        // =================================================================
        // BOOKINGS — CRUD (member reservations for sessions)
        // =================================================================

        /// <summary>Fetches a paged list of bookings with optional date filters.</summary>
        public async Task<PagedResult<BookingDTO>> GetBookingsAsync(int page = 1, int pageSize = 10,
            string? search = null,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            string? sortBy = null, string? sortDir = null)
        {
            var queryParams = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (dateFrom.HasValue) queryParams.Add($"dateFrom={dateFrom.Value:yyyy-MM-dd}");
            if (dateTo.HasValue) queryParams.Add($"dateTo={dateTo.Value:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(sortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
            if (!string.IsNullOrWhiteSpace(sortDir)) queryParams.Add($"sortDir={Uri.EscapeDataString(sortDir)}");
            var result = await _http.GetFromJsonAsync<PagedResult<BookingDTO>>($"api/booking?{string.Join("&", queryParams)}");
            return result ?? new PagedResult<BookingDTO>();
        }

        public async Task<BookingDTO?> GetBookingAsync(int id)
        {
            var response = await _http.GetAsync($"api/booking/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<BookingDTO>();
        }

        public async Task<bool> CreateBookingAsync(CreateBookingViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/booking", new
            {
                model.SessionId,
                model.UserId
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteBookingAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/booking/{id}");
            return response.IsSuccessStatusCode;
        }

        // =================================================================
        // EQUIPMENT — CRUD (gym machines and gear)
        // =================================================================

        /// <summary>Fetches a paged list of equipment with optional operational/room/date filters.</summary>
        public async Task<PagedResult<EquipmentDTO>> GetEquipmentAsync(int page = 1, int pageSize = 10,
            string? search = null, bool? operational = null, int? roomId = null,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            string? sortBy = null, string? sortDir = null)
        {
            var queryParams = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (operational.HasValue) queryParams.Add($"operational={operational.Value}");
            if (roomId.HasValue) queryParams.Add($"roomId={roomId.Value}");
            if (dateFrom.HasValue) queryParams.Add($"dateFrom={dateFrom.Value:yyyy-MM-dd}");
            if (dateTo.HasValue) queryParams.Add($"dateTo={dateTo.Value:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(sortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
            if (!string.IsNullOrWhiteSpace(sortDir)) queryParams.Add($"sortDir={Uri.EscapeDataString(sortDir)}");
            var result = await _http.GetFromJsonAsync<PagedResult<EquipmentDTO>>($"api/equipment?{string.Join("&", queryParams)}");
            return result ?? new PagedResult<EquipmentDTO>();
        }

        public async Task<EquipmentDTO?> GetEquipmentItemAsync(int id)
        {
            var response = await _http.GetAsync($"api/equipment/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<EquipmentDTO>();
        }

        public async Task<bool> CreateEquipmentAsync(CreateEquipmentViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/equipment", new
            {
                model.Description,
                model.InDate,
                model.Operational,
                model.RoomId
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateEquipmentAsync(int id, EditEquipmentViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/equipment/{id}", new
            {
                model.Description,
                model.InDate,
                model.Operational,
                model.RoomId
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteEquipmentAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/equipment/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<EquipmentDTO>> GetAllEquipmentAsync()
        {
            var result = await _http.GetFromJsonAsync<PagedResult<EquipmentDTO>>(
                "api/equipment?page=1&pageSize=999");
            return result?.Items ?? new List<EquipmentDTO>();
        }

        // =================================================================
        // SCHEDULES — CRUD (employee work schedules)
        // =================================================================

        /// <summary>Fetches a paged list of employee schedules.</summary>
        public async Task<PagedResult<ScheduleDTO>> GetSchedulesAsync(int page = 1, int pageSize = 10,
            string? search = null,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            string? sortBy = null, string? sortDir = null)
        {
            var queryParams = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (dateFrom.HasValue) queryParams.Add($"dateFrom={dateFrom.Value:yyyy-MM-dd}");
            if (dateTo.HasValue) queryParams.Add($"dateTo={dateTo.Value:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(sortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
            if (!string.IsNullOrWhiteSpace(sortDir)) queryParams.Add($"sortDir={Uri.EscapeDataString(sortDir)}");
            var result = await _http.GetFromJsonAsync<PagedResult<ScheduleDTO>>($"api/schedule?{string.Join("&", queryParams)}");
            return result ?? new PagedResult<ScheduleDTO>();
        }

        public async Task<ScheduleDTO?> GetScheduleAsync(int id)
        {
            var response = await _http.GetAsync($"api/schedule/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ScheduleDTO>();
        }

        public async Task<bool> CreateScheduleAsync(CreateScheduleViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/schedule", new
            {
                model.Start,
                model.End,
                model.UserId
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateScheduleAsync(int id, EditScheduleViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/schedule/{id}", new
            {
                model.Start,
                model.End
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteScheduleAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/schedule/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<ScheduleDTO>> GetAllSchedulesAsync()
        {
            var result = await _http.GetFromJsonAsync<PagedResult<ScheduleDTO>>(
                "api/schedule?page=1&pageSize=999");
            return result?.Items ?? new List<ScheduleDTO>();
        }

        // =================================================================
        // ATTENDANCES — check-in / check-out records
        // =================================================================

        /// <summary>Fetches a paged list of attendance records.</summary>
        public async Task<PagedResult<AttendanceDTO>> GetAttendancesAsync(int page = 1, int pageSize = 10,
            string? search = null, bool? inFlag = null,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            string? sortBy = null, string? sortDir = null)
        {
            var queryParams = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (inFlag.HasValue) queryParams.Add($"inFlag={inFlag.Value}");
            if (dateFrom.HasValue) queryParams.Add($"dateFrom={dateFrom.Value:yyyy-MM-dd}");
            if (dateTo.HasValue) queryParams.Add($"dateTo={dateTo.Value:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(sortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
            if (!string.IsNullOrWhiteSpace(sortDir)) queryParams.Add($"sortDir={Uri.EscapeDataString(sortDir)}");
            var result = await _http.GetFromJsonAsync<PagedResult<AttendanceDTO>>($"api/attendance?{string.Join("&", queryParams)}");
            return result ?? new PagedResult<AttendanceDTO>();
        }

        public async Task<List<AttendanceDTO>> GetActiveAttendancesAsync()
        {
            var result = await _http.GetFromJsonAsync<List<AttendanceDTO>>("api/attendance/active");
            return result ?? new List<AttendanceDTO>();
        }

        public async Task<AttendanceDTO?> GetAttendanceAsync(int id)
        {
            var response = await _http.GetAsync($"api/attendance/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<AttendanceDTO>();
        }

        public async Task<bool> CheckInMemberAsync(string memberId)
        {
            var response = await _http.PostAsync($"api/attendance/checkin/{memberId}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CheckOutMemberAsync(string memberId)
        {
            var response = await _http.PutAsync($"api/attendance/checkout/{memberId}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAttendanceAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/attendance/{id}");
            return response.IsSuccessStatusCode;
        }

        // =================================================================
        // QR SCANNER — scan member QR codes at the front desk
        // =================================================================

        /// <summary>
        /// Sends a scanned QR token to the API for validation.
        /// Returns a view model indicating success/failure and the check-in/out result.
        /// </summary>
        public async Task<ScanResultViewModel> ScanQRCodeAsync(string token)
        {
            var response = await _http.PostAsJsonAsync("api/qrcode/scan", new { Token = token });

            if (response.IsSuccessStatusCode)
            {
                var scan = await response.Content.ReadFromJsonAsync<ScanResponse>();
                if (scan is null)
                    return new ScanResultViewModel { Success = false, ErrorMessage = "Unexpected response from server." };

                return new ScanResultViewModel
                {
                    Success = true,
                    Action = scan.Action,
                    MemberName = scan.MemberName,
                    CheckIn = scan.CheckIn,
                    CheckOut = scan.CheckOut
                };
            }

            string? message = null;
            try
            {
                var errorBody = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                errorBody?.TryGetValue("message", out message);
            }
            catch { }

            return new ScanResultViewModel
            {
                Success = false,
                ErrorMessage = message ?? "Failed to process QR code."
            };
        }
    }
}
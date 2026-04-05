using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Services
{
    public class ManagementApiService : IManagementApiService
    {
        private readonly HttpClient _http;

        public ManagementApiService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("GymApi");
        }

        // --- Auth ---

        public async Task<LoginResponse?> LoginAsync(string email, string password)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new { EmailOrUserName = email, password });

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }

        // --- Members ---

        public async Task<PagedResult<UserDTO>> GetMembersAsync(int page = 1, int pageSize = 10)
        {
            var result = await _http.GetFromJsonAsync<PagedResult<UserDTO>>(
                $"api/member?page={page}&pageSize={pageSize}");
            return result ?? new PagedResult<UserDTO>();
        }

        public async Task<UserDTO?> GetMemberAsync(string id)
        {
            var response = await _http.GetAsync($"api/member/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserDTO>();
        }

        public async Task<bool> CreateMemberAsync(CreateMemberViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/member", new
            {
                model.Email,
                UserName = model.Username,
                model.FirstName,
                model.LastName,
                model.Password
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateMemberAsync(string id, EditMemberViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/member/{id}", new
            {
                model.FirstName,
                model.LastName,
                model.Active
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ToggleMemberActiveAsync(string id)
        {
            var response = await _http.PostAsync($"api/member/{id}/toggle-active", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteMemberAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/member/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<UserDTO>> GetAllMembersAsync()
        {
            var result = await _http.GetFromJsonAsync<PagedResult<UserDTO>>(
                "api/member?page=1&pageSize=999");
            return result?.Items ?? new List<UserDTO>();
        }

        // --- Staff ---

        public async Task<PagedResult<UserDTO>> GetStaffAsync(int page = 1, int pageSize = 10)
        {
            var result = await _http.GetFromJsonAsync<PagedResult<UserDTO>>(
                $"api/staff?page={page}&pageSize={pageSize}");
            return result ?? new PagedResult<UserDTO>();
        }

        public async Task<UserDTO?> GetStaffMemberAsync(string id)
        {
            var response = await _http.GetAsync($"api/staff/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserDTO>();
        }

        public async Task<bool> CreateStaffAsync(CreateStaffViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/staff", new
            {
                model.Email,
                model.FirstName,
                model.LastName,
                model.Password,
                model.EmployeeId,
                model.Role
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateStaffAsync(string id, EditStaffViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/staff/{id}", new
            {
                model.Email,
                model.FirstName,
                model.LastName,
                model.EmployeeId,
                model.BranchId
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteStaffAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/staff/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<UserDTO>> GetAllStaffAsync()
        {
            var result = await _http.GetFromJsonAsync<PagedResult<UserDTO>>(
                "api/staff?page=1&pageSize=999");
            return result?.Items ?? new List<UserDTO>();
        }

        public async Task<List<UserDTO>> GetAllEmployeesAsync()
        {
            var staff = await GetAllStaffAsync();
            var trainers = await GetAllTrainersAsync();

            var combined = new Dictionary<string, UserDTO>();
            foreach (var s in staff) combined[s.Id] = s;
            foreach (var t in trainers) combined.TryAdd(t.Id, t);

            return combined.Values.ToList();
        }

        public async Task<List<UserDTO>> GetEmployeesByBranchAsync(int branchId)
        {
            var all = await GetAllEmployeesAsync();
            return all.Where(e => e.BranchId == branchId).ToList();
        }

        public async Task<CountResponse> GetTotalMembersAsync()
        {
            var response = await _http.GetAsync("api/member/total");
            if (!response.IsSuccessStatusCode)
                return new CountResponse();
            return await response.Content.ReadFromJsonAsync<CountResponse>() ?? new CountResponse();
        }

        public async Task<CountResponse> GetTotalStaffAsync()
        {
            var response = await _http.GetAsync("api/staff/total");
            if (!response.IsSuccessStatusCode)
                return new CountResponse();
            return await response.Content.ReadFromJsonAsync<CountResponse>() ?? new CountResponse();
        }

        public async Task<List<UserDTO>> GetRecentSignupsAsync()
        {
            var result = await _http.GetFromJsonAsync<List<UserDTO>>("api/member/recents");
            return result ?? new List<UserDTO>();
        }

        public async Task LogoutAsync()
        {
            await _http.PostAsync("api/auth/logout", null);
        }

        // --- Branches ---

        public async Task<PagedResult<BranchDTO>> GetBranchesAsync(int page = 1, int pageSize = 10)
        {
            var result = await _http.GetFromJsonAsync<PagedResult<BranchDTO>>(
                $"api/branch?page={page}&pageSize={pageSize}");
            return result ?? new PagedResult<BranchDTO>();
        }

        public async Task<List<BranchDTO>> GetAllBranchesAsync()
        {
            var result = await _http.GetFromJsonAsync<PagedResult<BranchDTO>>(
                "api/branch?page=1&pageSize=999");
            return result?.Items ?? new List<BranchDTO>();
        }

        public async Task<BranchDTO?> GetBranchAsync(int id)
        {
            var response = await _http.GetAsync($"api/branch/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<BranchDTO>();
        }

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

        public async Task<bool> DeleteBranchAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/branch/{id}");
            return response.IsSuccessStatusCode;
        }

        // --- Tiers ---

        public async Task<PagedResult<TierDTO>> GetTiersAsync(int page = 1, int pageSize = 10)
        {
            var result = await _http.GetFromJsonAsync<PagedResult<TierDTO>>(
                $"api/tier?page={page}&pageSize={pageSize}");
            return result ?? new PagedResult<TierDTO>();
        }

        public async Task<List<TierDTO>> GetAllTiersAsync()
        {
            var result = await _http.GetFromJsonAsync<PagedResult<TierDTO>>(
                "api/tier?page=1&pageSize=999");
            return result?.Items ?? new List<TierDTO>();
        }

        public async Task<TierDTO?> GetTierAsync(string tierName)
        {
            var response = await _http.GetAsync($"api/tier/{tierName}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<TierDTO>();
        }

        public async Task<bool> CreateTierAsync(CreateTierViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/tier", new
            {
                model.TierName,
                model.Price
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTierAsync(string tierName, EditTierViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/tier/{tierName}", new
            {
                model.Price
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTierAsync(string tierName)
        {
            var response = await _http.DeleteAsync($"api/tier/{tierName}");
            return response.IsSuccessStatusCode;
        }

        // --- Trainers ---
        public async Task<CountResponse> GetTotalTrainersAsync()
        {
            var response = await _http.GetAsync("api/trainer/total");
            if (!response.IsSuccessStatusCode)
                return new CountResponse();
            return await response.Content.ReadFromJsonAsync<CountResponse>() ?? new CountResponse();
        }

        public async Task<PagedResult<UserDTO>> GetTrainersAsync(int page = 1, int pageSize = 10)
        {
            var result = await _http.GetFromJsonAsync<PagedResult<UserDTO>>(
                $"api/trainer?page={page}&pageSize={pageSize}");
            return result ?? new PagedResult<UserDTO>();
        }

        public async Task<UserDTO?> GetTrainerAsync(string id)
        {
            var response = await _http.GetAsync($"api/trainer/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserDTO>();
        }

        public async Task<bool> CreateTrainerAsync(CreateTrainerViewModel model)
        {
            var response = await _http.PostAsJsonAsync("api/trainer", new
            {
                model.Email,
                model.FirstName,
                model.LastName,
                model.Password,
                model.EmployeeId,
                model.BranchId
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTrainerAsync(string id, EditTrainerViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/trainer/{id}", new
            {
                model.Email,
                model.FirstName,
                model.LastName,
                model.EmployeeId,
                model.BranchId
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTrainerAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/trainer/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<UserDTO>> GetAllTrainersAsync()
        {
            var result = await _http.GetFromJsonAsync<PagedResult<UserDTO>>(
                "api/trainer?page=1&pageSize=999");
            return result?.Items ?? new List<UserDTO>();
        }

        // --- Rooms ---
        public async Task<PagedResult<RoomDTO>> GetRoomsAsync(int page = 1, int pageSize = 10)
        {
            var result = await _http.GetFromJsonAsync<PagedResult<RoomDTO>>(
                $"api/room?page={page}&pageSize={pageSize}");
            return result ?? new PagedResult<RoomDTO>();
        }

        public async Task<RoomDTO?> GetRoomAsync(int id)
        {
            var response = await _http.GetAsync($"api/room/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<RoomDTO>();
        }

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

        public async Task<bool> DeleteRoomAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/room/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<RoomDTO>> GetAllRoomsAsync()
        {
            var result = await _http.GetFromJsonAsync<PagedResult<RoomDTO>>(
                "api/room?page=1&pageSize=999");
            return result?.Items ?? new List<RoomDTO>();
        }

        // --- Classes ---

        public async Task<PagedResult<ClassDTO>> GetClassesAsync(int page = 1, int pageSize = 10)
        {
            var result = await _http.GetFromJsonAsync<PagedResult<ClassDTO>>(
                $"api/class?page={page}&pageSize={pageSize}");
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

        // --- Sessions ---

        public async Task<PagedResult<SessionDTO>> GetSessionsAsync(int page = 1, int pageSize = 10)
        {
            var result = await _http.GetFromJsonAsync<PagedResult<SessionDTO>>(
                $"api/session?page={page}&pageSize={pageSize}");
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

        // --- Subscriptions ---

        public async Task<PagedResult<SubscriptionDTO>> GetSubscriptionsAsync(int page = 1, int pageSize = 10)
        {
            var result = await _http.GetFromJsonAsync<PagedResult<SubscriptionDTO>>(
                $"api/subscription?page={page}&pageSize={pageSize}");
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

        // --- Payments ---

        public async Task<PagedResult<PaymentDTO>> GetPaymentsAsync(int page = 1, int pageSize = 10)
        {
            var result = await _http.GetFromJsonAsync<PagedResult<PaymentDTO>>(
                $"api/payment?page={page}&pageSize={pageSize}");
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

        // --- Bookings ---

        public async Task<PagedResult<BookingDTO>> GetBookingsAsync(int page = 1, int pageSize = 10)
        {
            var result = await _http.GetFromJsonAsync<PagedResult<BookingDTO>>(
                $"api/booking?page={page}&pageSize={pageSize}");
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

        // --- Equipment ---

        public async Task<PagedResult<EquipmentDTO>> GetEquipmentAsync(int page = 1, int pageSize = 10)
        {
            var result = await _http.GetFromJsonAsync<PagedResult<EquipmentDTO>>(
                $"api/equipment?page={page}&pageSize={pageSize}");
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

        // --- Schedules ---

        public async Task<PagedResult<ScheduleDTO>> GetSchedulesAsync(int page = 1, int pageSize = 10)
        {
            var result = await _http.GetFromJsonAsync<PagedResult<ScheduleDTO>>(
                $"api/schedule?page={page}&pageSize={pageSize}");
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

        // --- Attendances ---

        public async Task<List<AttendanceDTO>> GetAttendancesAsync()
        {
            var result = await _http.GetFromJsonAsync<List<AttendanceDTO>>("api/attendance");
            return result ?? new List<AttendanceDTO>();
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

        // --- QR Scanner ---

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
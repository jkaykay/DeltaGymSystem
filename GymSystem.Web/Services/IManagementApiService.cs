using GymSystem.Web.Areas.Management.ViewModels;
using GymSystem.Shared.DTOs;
using GymSystem.Shared.Enums;

namespace GymSystem.Web.Services
{
    /// <summary>
    /// Interface for the Management API service.
    /// Defines every operation the Management area controllers can perform:
    /// CRUD (Create, Read, Update, Delete) for members, staff, trainers, branches,
    /// tiers, rooms, classes, sessions, subscriptions, payments, bookings,
    /// equipment, schedules, attendances, and QR code scanning.
    /// Each method calls the backend REST API and returns data or success/failure.
    /// </summary>
    public interface IManagementApiService
    {
        // Members
        Task<bool> CreateMemberAsync(CreateMemberViewModel model);
        Task<PagedResult<UserDTO>> GetMembersAsync(int page = 1, int pageSize = 10,
    string? search = null, bool? active = null,
    DateTime? joinedFrom = null, DateTime? joinedTo = null,
    string? sortBy = null, string? sortDir = null);
        Task<UserDTO?> GetMemberAsync(string id);
        Task<bool> UpdateMemberAsync(string id, EditMemberViewModel model);
        Task<bool> ToggleMemberActiveAsync(string id);
        Task<bool> DeleteMemberAsync(string id);
        Task<CountResponse> GetTotalMembersAsync();
        Task<List<UserDTO>> GetRecentSignupsAsync();

        Task<List<UserDTO>> GetAllMembersAsync(); // for dropdowns

        // Staff
        Task<PagedResult<UserDTO>> GetStaffAsync(int page = 1, int pageSize = 10,
            string? search = null,
            DateTime? hiredFrom = null, DateTime? hiredTo = null,
            string? sortBy = null, string? sortDir = null);
        Task<UserDTO?> GetStaffMemberAsync(string id);
        Task<bool> CreateStaffAsync(CreateStaffViewModel model);
        Task<bool> UpdateStaffAsync(string id, EditStaffViewModel model);
        Task<bool> DeleteStaffAsync(string id);
        Task<CountResponse> GetTotalStaffAsync();
        Task<List<UserDTO>> GetAllStaffAsync(); // for dropdowns

        // Employees (staff + trainers combined, for schedule dropdowns)
        Task<List<UserDTO>> GetAllEmployeesAsync();
        Task<List<UserDTO>> GetEmployeesByBranchAsync(int branchId);

        // Trainers
        Task<PagedResult<UserDTO>> GetTrainersAsync(int page = 1, int pageSize = 10,
            string? search = null,
            DateTime? hiredFrom = null, DateTime? hiredTo = null,
            string? sortBy = null, string? sortDir = null);
        Task<UserDTO?> GetTrainerAsync(string id);
        Task<bool> CreateTrainerAsync(CreateTrainerViewModel model);
        Task<bool> UpdateTrainerAsync(string id, EditTrainerViewModel model);
        Task<bool> DeleteTrainerAsync(string id);
        Task<CountResponse> GetTotalTrainersAsync();
        Task<List<UserDTO>> GetAllTrainersAsync(); // for dropdowns

        // Branches
        Task<PagedResult<BranchDTO>> GetBranchesAsync(int page = 1, int pageSize = 10,
            string? search = null,
            string? sortBy = null, string? sortDir = null);
        Task<List<BranchDTO>> GetAllBranchesAsync();  // for dropdowns
        Task<BranchDTO?> GetBranchAsync(int id);
        Task<bool> CreateBranchAsync(CreateBranchViewModel model);
        Task<bool> UpdateBranchAsync(int id, EditBranchViewModel model);
        Task<bool> DeleteBranchAsync(int id);

        // Tiers
        Task<PagedResult<TierDTO>> GetTiersAsync(int page = 1, int pageSize = 10,
            string? search = null,
            string? sortBy = null, string? sortDir = null);
        Task<List<TierDTO>> GetAllTiersAsync();  // for dropdowns
        Task<TierDTO?> GetTierAsync(string tierName);
        Task<bool> CreateTierAsync(CreateTierViewModel model);
        Task<bool> UpdateTierAsync(string tierName, EditTierViewModel model);
        Task<bool> DeleteTierAsync(string tierName);

        // Rooms
        Task<PagedResult<RoomDTO>> GetRoomsAsync(int page = 1, int pageSize = 10,
            int? branchId = null, int? roomNumber = null,
            string? sortBy = null, string? sortDir = null);
        Task<RoomDTO?> GetRoomAsync(int id);
        Task<bool> CreateRoomAsync(CreateRoomViewModel model);
        Task<bool> UpdateRoomAsync(int id, EditRoomViewModel model);
        Task<bool> DeleteRoomAsync(int id);
        Task<List<RoomDTO>> GetAllRoomsAsync();    // for dropdowns

        // Classes
        Task<PagedResult<ClassDTO>> GetClassesAsync(int page = 1, int pageSize = 10,
            string? search = null,
            string? sortBy = null, string? sortDir = null);
        Task<ClassDTO?> GetClassAsync(int id);
        Task<bool> CreateClassAsync(CreateClassViewModel model);
        Task<bool> UpdateClassAsync(int id, EditClassViewModel model);
        Task<bool> DeleteClassAsync(int id);
        Task<List<ClassDTO>> GetAllClassesAsync(); // for dropdowns

        // Sessions
        Task<PagedResult<SessionDTO>> GetSessionsAsync(int page = 1, int pageSize = 10,
            string? search = null,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            int? roomId = null,
            string? sortBy = null, string? sortDir = null);
        Task<SessionDTO?> GetSessionAsync(int id);
        Task<bool> CreateSessionAsync(CreateSessionViewModel model);
        Task<bool> UpdateSessionAsync(int id, EditSessionViewModel model);
        Task<bool> DeleteSessionAsync(int id);
        Task<List<SessionDTO>> GetAllSessionsAsync();

        // Subscriptions
        Task<PagedResult<SubscriptionDTO>> GetSubscriptionsAsync(int page = 1, int pageSize = 10,
            string? search = null, int? state = null, string? tierName = null,
            DateTime? startFrom = null, DateTime? startTo = null,
            string? sortBy = null, string? sortDir = null);
        Task<SubscriptionDTO?> GetSubscriptionAsync(int id);
        Task<bool> CreateSubscriptionAsync(CreateSubscriptionViewModel model);
        Task<bool> UpdateSubscriptionAsync(int id, EditSubscriptionViewModel model);
        Task<bool> DeleteSubscriptionAsync(int id);
        Task<List<SubscriptionDTO>> GetAllSubscriptionsAsync();

        // Payments
        Task<PagedResult<PaymentDTO>> GetPaymentsAsync(int page = 1, int pageSize = 10,
            string? search = null,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            decimal? minAmount = null, decimal? maxAmount = null,
            string? sortBy = null, string? sortDir = null);
        Task<PaymentDTO?> GetPaymentAsync(int id);
        Task<bool> CreatePaymentAsync(CreatePaymentViewModel model);
        Task<bool> DeletePaymentAsync(int id);

        // Bookings
        Task<PagedResult<BookingDTO>> GetBookingsAsync(int page = 1, int pageSize = 10,
            string? search = null,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            string? sortBy = null, string? sortDir = null);
        Task<BookingDTO?> GetBookingAsync(int id);
        Task<bool> CreateBookingAsync(CreateBookingViewModel model);
        Task<bool> DeleteBookingAsync(int id);

        // Equipment
        Task<PagedResult<EquipmentDTO>> GetEquipmentAsync(int page = 1, int pageSize = 10,
            string? search = null, bool? operational = null, int? roomId = null,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            string? sortBy = null, string? sortDir = null);
        Task<EquipmentDTO?> GetEquipmentItemAsync(int id);
        Task<bool> CreateEquipmentAsync(CreateEquipmentViewModel model);
        Task<bool> UpdateEquipmentAsync(int id, EditEquipmentViewModel model);
        Task<bool> DeleteEquipmentAsync(int id);
        Task<List<EquipmentDTO>> GetAllEquipmentAsync();

        // Schedules
        Task<PagedResult<ScheduleDTO>> GetSchedulesAsync(int page = 1, int pageSize = 10,
            string? search = null,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            string? sortBy = null, string? sortDir = null);
        Task<ScheduleDTO?> GetScheduleAsync(int id);
        Task<bool> CreateScheduleAsync(CreateScheduleViewModel model);
        Task<bool> UpdateScheduleAsync(int id, EditScheduleViewModel model);
        Task<bool> DeleteScheduleAsync(int id);
        Task<List<ScheduleDTO>> GetAllSchedulesAsync();

        // Attendances
        Task<PagedResult<AttendanceDTO>> GetAttendancesAsync(int page = 1, int pageSize = 10,
            string? search = null, bool? inFlag = null,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            string? sortBy = null, string? sortDir = null);
        Task<List<AttendanceDTO>> GetActiveAttendancesAsync();
        Task<AttendanceDTO?> GetAttendanceAsync(int id);
        Task<bool> CheckInMemberAsync(string memberId);
        Task<bool> CheckOutMemberAsync(string memberId);
        Task<bool> DeleteAttendanceAsync(int id);

        // QR Scanner
        Task<ScanResultViewModel> ScanQRCodeAsync(string token);
    }
}
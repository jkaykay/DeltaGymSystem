using GymSystem.Shared.DTOs;

namespace GymSystem.Web.Areas.Member.ViewModels;

public class AttendanceHistoryViewModel
{
    public List<AttendanceDTO> Attendances { get; set; } = [];
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}

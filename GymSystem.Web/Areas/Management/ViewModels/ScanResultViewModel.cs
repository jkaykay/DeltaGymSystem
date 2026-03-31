namespace GymSystem.Web.Areas.Management.ViewModels;

public class ScanResultViewModel
{
    public bool Success { get; set; }
    public string? Action { get; set; }       // "checkin" or "checkout"
    public string? MemberName { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public string? ErrorMessage { get; set; }
}
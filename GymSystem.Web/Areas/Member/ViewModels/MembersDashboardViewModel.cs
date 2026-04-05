namespace GymSystem.Web.Areas.Member.ViewModels
{
    public class DashboardViewModel
    {
        public string Username { get; set; } = "";
        public int UpcomingClasses { get; set; } = 0;
        public int TotalBookings { get; set; } = 0;
        public int ClassesAttended { get; set; } = 0;
        public List<LogItem> LogHistory { get; set; } = new();
        public List<PaymentItem> PaymentHistory { get; set; } = new();
    }

    public class LogItem
    {
        public string Name { get; set; } = "";
        public string TimeFrom { get; set; } = "";
        public string TimeTo { get; set; } = "";
    }

    public class PaymentItem
    {
        public string Month { get; set; } = "";
        public string Amount { get; set; } = "";
    }
}
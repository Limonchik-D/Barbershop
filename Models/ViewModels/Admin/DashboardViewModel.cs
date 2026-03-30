namespace Barbershop.Models.ViewModels.Admin;

public class DashboardViewModel
{
    public int TotalAppointmentsToday { get; set; }
    public int PendingAppointments { get; set; }
    public int ConfirmedAppointments { get; set; }
    public int TotalClientsCount { get; set; }
    public int ActiveBarbersCount { get; set; }
    public decimal RevenueToday { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public List<RecentAppointmentItem> RecentAppointments { get; set; } = [];
    public List<BarberStatItem> BarberStats { get; set; } = [];
}

public class RecentAppointmentItem
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string BarberName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public string TimeRange { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusClass { get; set; } = string.Empty;
}

public class BarberStatItem
{
    public string BarberName { get; set; } = string.Empty;
    public int AppointmentsThisMonth { get; set; }
    public decimal RevenueThisMonth { get; set; }
}

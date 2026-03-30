using Barbershop.Models.Domain;

namespace Barbershop.Services;

public interface IAppointmentService
{
    Task<List<TimeOnly>> GetAvailableSlotsAsync(int barberId, DateOnly date, int serviceId);
    Task<bool> IsSlotAvailableAsync(int barberId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int? excludeId = null);
    Task<Appointment> CreateAppointmentAsync(string clientId, int barberId, int serviceId, DateOnly date, TimeOnly startTime, string? notes);
    Task<bool> CancelAppointmentAsync(int appointmentId, string userId, bool isAdmin, string? reason);
    Task<bool> ConfirmAppointmentAsync(int appointmentId);
    Task<bool> CompleteAppointmentAsync(int appointmentId);
}

using Barbershop.Data;
using Barbershop.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barbershop.Services;

public class AppointmentService : IAppointmentService
{
    private readonly ApplicationDbContext _db;

    public AppointmentService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<TimeOnly>> GetAvailableSlotsAsync(int barberId, DateOnly date, int serviceId)
    {
        var service = await _db.Services.FindAsync(serviceId);
        if (service == null) return [];

        // Проверяем переопределение расписания на конкретную дату
        var schedule = await _db.Schedules
            .FirstOrDefaultAsync(s => s.BarberId == barberId && s.Date == date);

        TimeOnly workStart, workEnd;
        int slotDuration;

        if (schedule != null)
        {
            if (!schedule.IsAvailable) return [];
            workStart = schedule.StartTime;
            workEnd = schedule.EndTime;
            slotDuration = 30;
        }
        else
        {
            // Берём стандартный рабочий день по шаблону
            var workDay = await _db.WorkDays
                .FirstOrDefaultAsync(w => w.BarberId == barberId && w.DayOfWeek == date.DayOfWeek);

            if (workDay == null || !workDay.IsWorking) return [];
            workStart = workDay.StartTime;
            workEnd = workDay.EndTime;
            slotDuration = workDay.SlotDurationMinutes;
        }

        // Загружаем уже занятые слоты
        var existingAppointments = await _db.Appointments
            .Where(a => a.BarberId == barberId
                        && a.AppointmentDate == date
                        && a.Status != AppointmentStatus.Cancelled)
            .Select(a => new { a.StartTime, a.EndTime })
            .ToListAsync();

        var slots = new List<TimeOnly>();
        var current = workStart;
        var serviceDuration = TimeSpan.FromMinutes(service.Duration);

        while (current.Add(serviceDuration) <= workEnd)
        {
            var slotEnd = current.Add(serviceDuration);
            var isOccupied = existingAppointments.Any(a =>
                current < a.EndTime && slotEnd > a.StartTime);

            if (!isOccupied)
                slots.Add(current);

            current = current.Add(TimeSpan.FromMinutes(slotDuration));
        }

        return slots;
    }

    public async Task<bool> IsSlotAvailableAsync(int barberId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int? excludeId = null)
    {
        var query = _db.Appointments
            .Where(a => a.BarberId == barberId
                        && a.AppointmentDate == date
                        && a.Status != AppointmentStatus.Cancelled
                        && a.StartTime < endTime
                        && a.EndTime > startTime);

        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);

        return !await query.AnyAsync();
    }

    public async Task<Appointment> CreateAppointmentAsync(string clientId, int barberId, int serviceId, DateOnly date, TimeOnly startTime, string? notes)
    {
        var service = await _db.Services.FindAsync(serviceId)
            ?? throw new InvalidOperationException("Услуга не найдена");

        var endTime = startTime.Add(TimeSpan.FromMinutes(service.Duration));

        if (!await IsSlotAvailableAsync(barberId, date, startTime, endTime))
            throw new InvalidOperationException("Выбранное время уже занято");

        var appointment = new Appointment
        {
            ClientId = clientId,
            BarberId = barberId,
            ServiceId = serviceId,
            AppointmentDate = date,
            StartTime = startTime,
            EndTime = endTime,
            TotalPrice = service.Price,
            Notes = notes,
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();
        return appointment;
    }

    public async Task<bool> CancelAppointmentAsync(int appointmentId, string userId, bool isAdmin, string? reason)
    {
        var appointment = await _db.Appointments.FindAsync(appointmentId);
        if (appointment == null) return false;

        if (!isAdmin && appointment.ClientId != userId) return false;

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            return false;

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.CancelReason = reason;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ConfirmAppointmentAsync(int appointmentId)
    {
        var appointment = await _db.Appointments.FindAsync(appointmentId);
        if (appointment == null || appointment.Status != AppointmentStatus.Pending) return false;

        appointment.Status = AppointmentStatus.Confirmed;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteAppointmentAsync(int appointmentId)
    {
        var appointment = await _db.Appointments.FindAsync(appointmentId);
        if (appointment == null || appointment.Status == AppointmentStatus.Cancelled) return false;

        appointment.Status = AppointmentStatus.Completed;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}

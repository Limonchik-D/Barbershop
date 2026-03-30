namespace Barbershop.Models.Domain;

/// <summary>
/// Шаблон рабочей недели мастера (регулярное расписание)
/// </summary>
public class WorkDay
{
    public int Id { get; set; }
    public int BarberId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsWorking { get; set; } = true;
    public int SlotDurationMinutes { get; set; } = 30;

    public Barber Barber { get; set; } = null!;
}

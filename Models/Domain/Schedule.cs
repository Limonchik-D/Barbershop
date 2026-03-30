namespace Barbershop.Models.Domain;

/// <summary>
/// Переопределение расписания на конкретную дату (выходные, особые дни, отпуск)
/// </summary>
public class Schedule
{
    public int Id { get; set; }
    public int BarberId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? Note { get; set; }

    public Barber Barber { get; set; } = null!;
}

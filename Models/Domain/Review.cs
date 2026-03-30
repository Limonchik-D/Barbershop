namespace Barbershop.Models.Domain;

public class Review
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public int BarberId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
    public bool IsApproved { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Appointment Appointment { get; set; } = null!;
    public ApplicationUser Client { get; set; } = null!;
    public Barber Barber { get; set; } = null!;
}

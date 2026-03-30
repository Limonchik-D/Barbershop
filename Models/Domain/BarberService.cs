namespace Barbershop.Models.Domain;

public class BarberService
{
    public int BarberId { get; set; }
    public int ServiceId { get; set; }

    public Barber Barber { get; set; } = null!;
    public Service Service { get; set; } = null!;
}

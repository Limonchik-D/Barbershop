namespace Barbershop.Models.Domain;

public enum AppointmentStatus
{
    Pending = 0,
    Confirmed = 1,
    Completed = 2,
    Cancelled = 3,
    NoShow = 4
}

public enum PaymentMethod
{
    Cash = 0,
    Card = 1,
    Online = 2
}

public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Refunded = 2,
    Failed = 3
}

public enum NotificationType
{
    Appointment = 0,
    Payment = 1,
    Reminder = 2,
    System = 3
}

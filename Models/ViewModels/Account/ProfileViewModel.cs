using System.ComponentModel.DataAnnotations;

namespace Barbershop.Models.ViewModels.Account;

public class ProfileViewModel
{
    [Required]
    [Display(Name = "Имя")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Телефон")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Дата рождения")]
    public DateTime? DateOfBirth { get; set; }
}

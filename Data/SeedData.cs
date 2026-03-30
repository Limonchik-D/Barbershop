using Barbershop.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Barbershop.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await db.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
        await SeedCategoriesAndServicesAsync(db);
        await SeedBarbersAsync(db, userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "Manager", "Barber", "Client"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // Admin
        if (await userManager.FindByEmailAsync("admin@barbershop.ru") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@barbershop.ru",
                Email = "admin@barbershop.ru",
                FirstName = "Администратор",
                LastName = "Системы",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Manager
        if (await userManager.FindByEmailAsync("manager@barbershop.ru") == null)
        {
            var manager = new ApplicationUser
            {
                UserName = "manager@barbershop.ru",
                Email = "manager@barbershop.ru",
                FirstName = "Менеджер",
                LastName = "Иванов",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(manager, "Manager123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(manager, "Manager");
        }
    }

    private static async Task SeedCategoriesAndServicesAsync(ApplicationDbContext db)
    {
        if (await db.ServiceCategories.AnyAsync()) return;

        var categories = new List<ServiceCategory>
        {
            new() { Name = "Стрижки", Description = "Классические и модные стрижки", SortOrder = 1, IsActive = true },
            new() { Name = "Борода", Description = "Уход за бородой и усами", SortOrder = 2, IsActive = true },
            new() { Name = "Комплексный уход", Description = "Стрижка + борода и другие комплексы", SortOrder = 3, IsActive = true },
            new() { Name = "Укладка", Description = "Профессиональная укладка волос", SortOrder = 4, IsActive = true }
        };
        db.ServiceCategories.AddRange(categories);
        await db.SaveChangesAsync();

        var services = new List<Service>
        {
            new() { CategoryId = categories[0].Id, Name = "Классическая стрижка", Description = "Стрижка машинкой и ножницами", Duration = 30, Price = 800, IsActive = true },
            new() { CategoryId = categories[0].Id, Name = "Мужская стрижка", Description = "Современная мужская стрижка с укладкой", Duration = 45, Price = 1200, IsActive = true },
            new() { CategoryId = categories[0].Id, Name = "Детская стрижка", Description = "Стрижка для детей до 12 лет", Duration = 30, Price = 600, IsActive = true },
            new() { CategoryId = categories[1].Id, Name = "Оформление бороды", Description = "Стрижка и оформление контура бороды", Duration = 30, Price = 700, IsActive = true },
            new() { CategoryId = categories[1].Id, Name = "Бритьё опасной бритвой", Description = "Классическое бритьё с горячим полотенцем", Duration = 40, Price = 1000, IsActive = true },
            new() { CategoryId = categories[2].Id, Name = "Комбо: стрижка + борода", Description = "Стрижка и уход за бородой", Duration = 60, Price = 1700, IsActive = true },
            new() { CategoryId = categories[3].Id, Name = "Укладка волос", Description = "Профессиональная укладка с использованием средств", Duration = 20, Price = 500, IsActive = true }
        };
        db.Services.AddRange(services);
        await db.SaveChangesAsync();
    }

    private static async Task SeedBarbersAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        if (await db.Barbers.AnyAsync()) return;

        // Создаём пользователей-мастеров
        async Task<ApplicationUser?> CreateBarberUser(string email, string firstName, string lastName)
        {
            if (await userManager.FindByEmailAsync(email) != null) return null;
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(user, "Barber123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Barber");
                return user;
            }
            return null;
        }

        var user1 = await CreateBarberUser("alex@barbershop.ru", "Александр", "Петров");
        var user2 = await CreateBarberUser("dmitry@barbershop.ru", "Дмитрий", "Козлов");
        var user3 = await CreateBarberUser("sergey@barbershop.ru", "Сергей", "Новиков");

        var barbers = new List<Barber>();
        if (user1 != null)
        {
            barbers.Add(new Barber
            {
                UserId = user1.Id, FirstName = "Александр", LastName = "Петров",
                Patronymic = "Иванович", Bio = "Мастер с 5-летним стажем. Специализируюсь на классических и современных мужских стрижках.",
                Phone = "+7 (900) 111-22-33", Email = "alex@barbershop.ru", ExperienceYears = 5, IsActive = true
            });
        }
        if (user2 != null)
        {
            barbers.Add(new Barber
            {
                UserId = user2.Id, FirstName = "Дмитрий", LastName = "Козлов",
                Patronymic = "Сергеевич", Bio = "Профессиональный барбер, призёр городских чемпионатов по барберингу.",
                Phone = "+7 (900) 444-55-66", Email = "dmitry@barbershop.ru", ExperienceYears = 7, IsActive = true
            });
        }
        if (user3 != null)
        {
            barbers.Add(new Barber
            {
                UserId = user3.Id, FirstName = "Сергей", LastName = "Новиков",
                Patronymic = "Алексеевич", Bio = "Мастер по работе с бородой и усами. Специалист по горячему бритью.",
                Phone = "+7 (900) 777-88-99", Email = "sergey@barbershop.ru", ExperienceYears = 3, IsActive = true
            });
        }

        if (barbers.Count == 0) return;

        db.Barbers.AddRange(barbers);
        await db.SaveChangesAsync();

        // Привязываем услуги к мастерам
        var services = await db.Services.ToListAsync();
        var barberServiceLinks = new List<BarberService>();
        foreach (var barber in barbers)
        {
            foreach (var service in services)
                barberServiceLinks.Add(new BarberService { BarberId = barber.Id, ServiceId = service.Id });
        }
        db.BarberServices.AddRange(barberServiceLinks);

        // Рабочие дни (шаблон): пн-пт 10:00-20:00, сб 10:00-18:00
        var workDays = new List<WorkDay>();
        foreach (var barber in barbers)
        {
            var weekDays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
            foreach (var day in weekDays)
            {
                workDays.Add(new WorkDay
                {
                    BarberId = barber.Id, DayOfWeek = day,
                    StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(20, 0),
                    IsWorking = true, SlotDurationMinutes = 30
                });
            }
            workDays.Add(new WorkDay
            {
                BarberId = barber.Id, DayOfWeek = DayOfWeek.Saturday,
                StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(18, 0),
                IsWorking = true, SlotDurationMinutes = 30
            });
        }
        db.WorkDays.AddRange(workDays);
        await db.SaveChangesAsync();
    }
}

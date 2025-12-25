using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LeaveManagementSystem.Data;
using LeaveManagementSystem.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

   
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    Console.WriteLine("Deleting existing database...");
    context.Database.EnsureDeleted();

    Console.WriteLine("Creating new database...");
    context.Database.EnsureCreated();

   
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
        Console.WriteLine("Admin role created");
    }

    if (!await roleManager.RoleExistsAsync("Employee"))
    {
        await roleManager.CreateAsync(new IdentityRole("Employee"));
        Console.WriteLine("Employee role created");
    }

    
    var adminEmail = "ibi@gmail.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "Ibrahim@111");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine("Admin user created: ibi@gmail.com / Ibrahim@111");
        }
    }

   
    var empEmail = "babar@gmail.com";
    if (await userManager.FindByEmailAsync(empEmail) == null)
    {
        var empUser = new IdentityUser
        {
            UserName = empEmail,
            Email = empEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(empUser, "Babar@056");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(empUser, "Employee");

            var employee = new Employee
            {
                Name = "Babar",
                Email = empEmail,
                CNIC = "12345-1234567-1",
                Department = "AI",
                Designation = "Analyst",
                IsActive = true,
                IdentityUserId = empUser.Id
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            Console.WriteLine("Test employee created: babar@gmail.com / Babar@056");
        }
    }
}

app.Run();
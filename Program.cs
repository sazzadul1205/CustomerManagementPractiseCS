using CustomerManagementPractiseCS.Data;
using CustomerManagementPractiseCS.Services;
using CustomerManagementPractiseCS.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddDefaultIdentity<IdentityUser>() // Register a Service for DI Injection 
//    .AddEntityFrameworkStores<AppDbContext>(); // It says to My EFCore where to Store data 

// IF i want to Customize the Lockout time ans Cookie Duration 
//builder.Services.AddDefaultIdentity<IdentityUser>(options =>
//{
//    options.Lockout.MaxFailedAccessAttempts = 5;
//    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
//})
//.AddEntityFrameworkStores<AppDbContext>();


// i did not use DefaultIdentity Because i would not like to sue the MS Default System 
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders(); // This Provides me the Token Generator and Validator of Identity

// This Provides the Customization of the Authentication Cookie 
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IProfileCompletenessService, ProfileCompletenessService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();  
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // Asks Who is the User
app.UseAuthorization(); // Asks can the user Access 

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

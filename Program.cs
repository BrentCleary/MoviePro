using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using MoviePro.Data;
using MoviePro.Models.Settings;
using MoviePro.Services;
using MoviePro.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add services to the container.
var connectionString = ConnectionService.GetConnectionString(builder.Configuration) ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddScoped<SeedService>();
builder.Services.AddSingleton<IImageService, BasicImageService>();
builder.Services.AddScoped<IRemoteMovieService, TMDBMovieService>();
builder.Services.AddScoped<IDataMappingService, TMDBMappingService>();


//builder.Services.AddIdentity(options => options.SignIn.RequireConfirmedAccount = true)
//.AddEntityFrameworkStores();

// Load both appsettings.json and secrets.json configurations
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);


builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("appsettings"));

var app = builder.Build();

// Resolve DataService and run initialization ManageDataAsync()
using (var scope = app.Services.CreateScope())
{
    //DataService
    var serviceProvider = scope.ServiceProvider;
    var seedService = serviceProvider.GetRequiredService<SeedService>();
    await seedService.ManageDataAsync();
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
